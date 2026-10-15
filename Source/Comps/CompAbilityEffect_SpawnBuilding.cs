using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CrimsonGridFramework
{
    public class CompAbilityEffect_SpawnBuilding : CompAbilityEffect
    {
        public new CompProperties_SpawnBuilding Props => (CompProperties_SpawnBuilding)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (Props?.placedBuilding != null && parent?.verb?.caster?.Map != null)
            {
                if (!GenSpawn.TrySpawn(Props.placedBuilding, target.Cell, parent.verb.caster.Map, out Thing t, canWipeEdifices: false))
                {
                    parent.RemainingCharges++;
                }
                else if (parent?.pawn?.HostFaction != null)
                {
                    t.SetFaction(parent.pawn.HostFaction);
                }
                else if (parent?.pawn?.Faction != null)
                {
                    t.SetFaction(parent.pawn.Faction);
                }
            }
        }
    }
}
