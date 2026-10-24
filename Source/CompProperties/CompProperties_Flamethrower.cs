using Verse;

namespace CrimsonGridFramework
{
    public class CompProperties_Flamethrower : CompProperties
    {
        public float heatPerShot;
        public float fireStartChance;
        public HediffDef heatHediff;
        public ThingDef filthDef;
        public float napalmSpawnChance;

        public CompProperties_Flamethrower()
        {
            compClass = typeof(CompFlamethrower);
        }
    }
}
