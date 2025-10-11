using Verse;

namespace CrimsonGridFramework
{
    public class HediffCompProperties_APS : HediffCompProperties
    {
        public float interceptRadius = 10f;
        public int tickInterval = 10;
        public bool showInterceptionText = true;
        public int cooldownTicks = 1200;
        public int maxCharges = 3;
        public ThingDef ammoDef;
        public int baseReloadTicks = 60;
        public int ammoCountPerCharge = 1;

        public HediffCompProperties_APS()
        {
            compClass = typeof(HediffComp_APS);
        }
    }
}