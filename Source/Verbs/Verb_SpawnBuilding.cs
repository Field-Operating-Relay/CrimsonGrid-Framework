using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CrimsonGridFramework
{
    public class Verb_SpawnBuilding : Verb_CastAbility
    {
        public override bool CanHitTarget(LocalTargetInfo targ)
        {
            ThingDef building = Ability?.CompOfType<CompAbilityEffect_SpawnBuilding>()?.Props?.placedBuilding;
            if (building == null || !GenSpawn.CanSpawnAt(building, targ.Cell, caster.Map, building.defaultPlacingRot, false))
            {
                return false;
            }
            return base.CanHitTarget(targ);
        }
    }
}
