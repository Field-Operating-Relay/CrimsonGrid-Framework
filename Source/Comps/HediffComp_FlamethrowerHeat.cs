using RimWorld;
using UnityEngine;
using Verse;

namespace CrimsonGridFramework
{
    public class HediffComp_FlamethrowerHeat : HediffComp
    {
        private int ticksSinceLastShot;
        private int ticksSinceMeltdownWarning;
        private int ticksSinceHeatstrokeMessage;
        private bool meltdownTriggered;

        public HediffCompProperties_FlamethrowerHeat Props => (HediffCompProperties_FlamethrowerHeat)props;

        public void AddHeat(float amount)
        {
            parent.Severity = Mathf.Clamp(parent.Severity + amount, 0f, parent.def.maxSeverity);
            ticksSinceLastShot = 0;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            ticksSinceLastShot++;
            ticksSinceMeltdownWarning++;
            ticksSinceHeatstrokeMessage++;

            if (ticksSinceLastShot > Props.decayCooldownTicks)
                severityAdjustment -= Props.heatDecayPerTick;

            if (Pawn.Map == null) return;
            var sev = parent.Severity;

            if (sev >= parent.def.maxSeverity)
            {
                TriggerMeltdown();
                return;
            }

            if (sev >= Props.meltdownThreshold)
                HandleMeltdownImminent();
            else if (sev >= Props.pressurizedThreshold)
                HandlePressurized();
        }

        private void HandlePressurized()
        {
            var pawn = parent.pawn;

            if (pawn.IsHashIntervalTick(Props.pressurizedGlowInterval))
                FleckMaker.ThrowFireGlow(pawn.DrawPos, pawn.Map, Props.pressurizedGlowSize);

            if (pawn.IsHashIntervalTick(Props.randomBurnIntervalTicks) && Rand.Chance(Props.randomBurnChance))
                pawn.TakeDamage(new DamageInfo(Props.burnDamageDef, Props.randomBurnDamage, 0f, -1f, pawn));

            if (ticksSinceHeatstrokeMessage >= Props.heatstrokeMessageIntervalTicks)
            {
                ticksSinceHeatstrokeMessage = 0;
                Messages.Message("CG_FlamethrowerHeat_Pressurized".Translate(pawn.LabelShort), pawn, MessageTypeDefOf.CautionInput);
            }
        }

        private void HandleMeltdownImminent()
        {
            var pawn = parent.pawn;

            if (pawn.IsHashIntervalTick(Props.meltdownGlowInterval))
                FleckMaker.ThrowFireGlow(pawn.DrawPos, pawn.Map, Props.meltdownGlowSize);

            if (pawn.IsHashIntervalTick(Props.severeBurnIntervalTicks) && Rand.Chance(Props.severeBurnChance))
                pawn.TakeDamage(new DamageInfo(Props.burnDamageDef, Props.severeBurnDamage, 0f, -1f, pawn));

            if (ticksSinceMeltdownWarning >= Props.meltdownWarningIntervalTicks)
            {
                ticksSinceMeltdownWarning = 0;
                Find.LetterStack.ReceiveLetter(
                    "CG_FlamethrowerMeltdown".Translate(),
                    "CG_FlamethrowerMeltdownDesc".Translate(pawn.LabelShort),
                    LetterDefOf.ThreatBig,
                    pawn
                );
            }
        }

        private void TriggerMeltdown()
        {
            if (meltdownTriggered) return;
            meltdownTriggered = true;

            var pawn = parent.pawn;
            var map = pawn.Map;
            var pos = pawn.Position;
            var flamethrowerComp = pawn.equipment?.Primary?.GetComp<CompFlamethrower>();

            pawn.health.RemoveHediff(parent);
            GenExplosion.DoExplosion(pos, map, Props.meltdownExplosionRadius, Props.meltdownExplosionDamageDef, pawn);
            GenExplosion.DoExplosion(pos, map, Props.meltdownFireRadius, Props.meltdownFireDamageDef, pawn);

            if (flamethrowerComp != null)
            {
                var props = flamethrowerComp.Props;
                foreach (var cell in GenRadial.RadialCellsAround(pos, Props.meltdownNapalmRadius, true))
                {
                    if (cell.InBounds(map))
                    {
                        FilthMaker.TryMakeFilth(cell, map, props.filthDef);
                        if (Rand.Chance(props.fireStartChance))
                            FireUtility.TryStartFireIn(cell, map, 0.1f, null, null);
                    }
                }
            }

            if (!pawn.Dead)
                pawn.Kill(new DamageInfo(Props.meltdownExplosionDamageDef, Props.meltdownKillDamage, Props.meltdownKillArmorPenetration, -1f, pawn));
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticksSinceLastShot, "ticksSinceLastShot", 0);
            Scribe_Values.Look(ref ticksSinceMeltdownWarning, "ticksSinceMeltdownWarning", 0);
            Scribe_Values.Look(ref ticksSinceHeatstrokeMessage, "ticksSinceHeatstrokeMessage", 0);
            Scribe_Values.Look(ref meltdownTriggered, "meltdownTriggered", false);
        }
    }
}
