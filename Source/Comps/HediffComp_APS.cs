using RimWorld;
using RimWorld.Utility;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;

namespace CrimsonGridFramework
{
    public class HediffComp_APS : HediffComp, IReloadableComp, ICompWithCharges
    {
        private int tickCounter = 0;
        private float interceptRadiusSquared;
        private readonly List<Projectile> toDeleteProjectiles = new List<Projectile>();
        private int cooldownTicksRemaining = 0;
        private int remainingCharges;
        private bool enabledByPlayer = true;
        private bool blockFriendlyFire = false;
        private bool disabled = false;
        private const int ONE_SECOND_IN_TICKS = 60;

        public HediffCompProperties_APS Props => (HediffCompProperties_APS)props;
        public Thing ReloadableThing => parent.pawn;
        public ThingDef AmmoDef => Props.ammoDef;
        public int MaxCharges => Props.maxCharges;
        public int BaseReloadTicks => Props.baseReloadTicks;
        public string LabelRemaining => $"{remainingCharges} / {MaxCharges}";
        public int RemainingCharges
        {
            get => remainingCharges;
            set => remainingCharges = value;
        }
        public int CooldownTicksRemaining => cooldownTicksRemaining;
        public bool Enabled => enabledByPlayer;
        public bool Disabled => disabled;

        public override void CompPostMake()
        {
            base.CompPostMake();
            remainingCharges = MaxCharges;
            interceptRadiusSquared = Props.interceptRadius * Props.interceptRadius;
        }

        public override void CompPostTick(ref float sev)
        {
            if (Find.TickManager.TicksGame % ONE_SECOND_IN_TICKS == 0)
            {
                disabled = !CanBeUsed(out _);
            }

            if (cooldownTicksRemaining > 0)
            {
                cooldownTicksRemaining--;
            }

            if (!enabledByPlayer || remainingCharges <= 0 || cooldownTicksRemaining > 0)
            {
                return;
            }

            tickCounter++;
            if (tickCounter < Props.tickInterval)
            {
                return;
            }
            tickCounter = 0;

            var tracker = parent.pawn.Map?.GetComponent<MapComponent_ProjectileTracker>();

            foreach (var projectile in tracker.ExplosiveProjectiles)
            {
                if (ShouldInterceptProjectile(projectile))
                {
                    toDeleteProjectiles.Add(projectile);
                }
            }

            if (toDeleteProjectiles.Count > 0)
            {
                remainingCharges--;
                cooldownTicksRemaining = Props.cooldownTicks;

                foreach (Projectile proj in toDeleteProjectiles)
                {
                    InterceptProjectile(proj);
                }
                toDeleteProjectiles.Clear();
            }
        }

        private bool ShouldInterceptProjectile(Projectile projectile)
        {
            if (projectile == null || projectile.Destroyed || !projectile.Spawned)
            {
                return false;
            }

            // Don't intercept own projectiles
            if (projectile.Launcher == parent.pawn)
            {
                return false;
            }

            // Don't intercept projectiles launched by allies
            if (projectile.Launcher.Faction == parent.pawn.Faction && !blockFriendlyFire)
            {
                return false;
            }

            Vector3 projectilePos = projectile.ExactPosition;
            Vector3 pawnPos = parent.pawn.DrawPos;

            float distSqProjectileToPawn = (projectilePos - pawnPos).sqrMagnitude;

            return distSqProjectileToPawn <= interceptRadiusSquared;
        }

        private void InterceptProjectile(Projectile projectile)
        {
            Vector3 interceptPos = projectile.ExactPosition;

            if (Props.showInterceptionText)
            {
                MoteMaker.ThrowText(
                    interceptPos,
                    projectile.Map,
                    "CGF_APS_Activated".Translate(),
                    Color.cyan,
                    2f
                );
            }

            FleckMaker.ThrowMicroSparks(interceptPos, projectile.Map);

            projectile.Destroy(DestroyMode.Vanish);
        }

        public bool NeedsReload(bool allowForcedReload)
        {
            if (!allowForcedReload)
            {
                return remainingCharges == 0;
            }
            return remainingCharges != MaxCharges;
        }

        public void ReloadFrom(Thing ammo)
        {
            if (!NeedsReload(allowForcedReload: true))
            {
                return;
            }

            if (ammo.stackCount < Props.ammoCountPerCharge)
            {
                return;
            }

            int numCharges = Mathf.Clamp(ammo.stackCount / Props.ammoCountPerCharge, 0, MaxCharges - remainingCharges);
            ammo.SplitOff(numCharges * Props.ammoCountPerCharge).Destroy();
            remainingCharges += numCharges;
        }

        public int MinAmmoNeeded(bool allowForcedReload)
        {
            if (!NeedsReload(allowForcedReload))
            {
                return 0;
            }
            return Props.ammoCountPerCharge;
        }

        public int MaxAmmoNeeded(bool allowForcedReload)
        {
            if (!NeedsReload(allowForcedReload))
            {
                return 0;
            }
            return Props.ammoCountPerCharge * (MaxCharges - remainingCharges);
        }

        public int MaxAmmoAmount()
        {
            return Props.ammoCountPerCharge * MaxCharges;
        }

        public string DisabledReason(int minNeeded, int maxNeeded)
        {
            if (minNeeded == maxNeeded)
            {
                return "CGF_APS_NeedAmmoSingle".Translate(minNeeded, Props.ammoDef.label);
            }
            return "CGF_APS_NeedAmmoRange".Translate(minNeeded, maxNeeded, Props.ammoDef.label);
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            yield return new Command_APSToggle
            {
                comp = this,
                defaultLabel = "CGF_APS_GizmoLabel".Translate(),
                defaultDesc = "CGF_APS_GizmoDesc".Translate(),
                icon = BaseContent.BadTex,
                toggleAction = () => enabledByPlayer = !enabledByPlayer,
                Order = 100f
            };

            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Toggle friendly fire",
                    action = delegate
                    {
                        blockFriendlyFire = !blockFriendlyFire;
                        if (blockFriendlyFire)
                        {
                            Messages.Message("Blocking friendly fire", MessageTypeDefOf.NeutralEvent);
                        }
                        else
                        {
                            Messages.Message("Ignoring friendly fire", MessageTypeDefOf.NeutralEvent);
                        }
                    }
                };
            }
        }

        public bool CanBeUsed(out string reason)
        {
            reason = null;
            if (RemainingCharges <= 0)
            {
                reason = DisabledReason(MinAmmoNeeded(allowForcedReload: false), MaxAmmoNeeded(allowForcedReload: false));
                return false;
            }
            return true;
        }

        public override string CompLabelInBracketsExtra => enabledByPlayer
            ? "CGF_APS_ChargesLabel".Translate(remainingCharges, MaxCharges)
            : "CGF_APS_Off".Translate();

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref cooldownTicksRemaining, "cooldownTicksRemaining", 0);
            Scribe_Values.Look(ref remainingCharges, "remainingCharges", MaxCharges);
            Scribe_Values.Look(ref enabledByPlayer, "enabled", true);
            Scribe_Values.Look(ref disabled, "disabled", false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                interceptRadiusSquared = Props.interceptRadius * Props.interceptRadius;
            }
        }
    }
}