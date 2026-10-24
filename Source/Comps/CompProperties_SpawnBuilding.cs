using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CrimsonGridFramework
{
    public class CompProperties_SpawnBuilding : CompProperties_AbilityEffect
    {
        public ThingDef placedBuilding;

        public CompProperties_SpawnBuilding()
        {
            compClass = typeof(CompAbilityEffect_SpawnBuilding);
        }
    }
}
