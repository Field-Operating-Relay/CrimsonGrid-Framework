using RimWorld;
using UnityEngine;
using Verse;

namespace CrimsonGridFramework
{
    public class CompDeployableBuilding : ThingComp
    {
        public CompProperties_DeployableBuilding Props => (CompProperties_DeployableBuilding)props;
        private int age;
        private bool expiring;

        public override void CompTick()
        {
            if (Props.lifespanTicks > 0)
            {
                age++;
                if (age >= Props.lifespanTicks)
                {
                    Expire();
                }
            }
        }

        private void Expire()
        {
            expiring = true;
            var hp = parent.HitPoints;
            var map = parent.Map;
            var pos = parent.Position;
            var faction = parent.Faction;
            parent.Destroy();
            if (Props.inactiveDef != null && map != null && pos.InBounds(map))
            {
                var newBuilding = GenSpawn.Spawn(Props.inactiveDef, pos, map);
                newBuilding.SetFaction(faction);
                newBuilding.HitPoints = hp;
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            var pos = parent.PositionHeld;
            var faction = parent.Faction;
            base.PostDestroy(mode, previousMap);
            if (expiring is false && Props.inactiveDef != null && Props.turnToInactiveWhenDestroyed && previousMap != null && pos.InBounds(previousMap))
            {
                var newBuilding = GenSpawn.Spawn(Props.inactiveDef, pos, previousMap);
                newBuilding.SetFaction(faction);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref age, "age");
        }

        public override string CompInspectStringExtra()
        {
            if (Props.lifespanTicks <= 0)
            {
                return base.CompInspectStringExtra();
            }
            var remainingTicks = Props.lifespanTicks - age;
            return "CGF_DeployableTimeLeft".Translate(GenDate.ToStringTicksToPeriod(remainingTicks));
        }
    }
}
