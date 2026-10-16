using Verse;

namespace CrimsonGridFramework
{
    public class HediffCompProperties_FlamethrowerHeat : HediffCompProperties
    {
        public float heatDecayPerTick;
        public int decayCooldownTicks;
        public float randomBurnDamage;
        public int randomBurnIntervalTicks;
        public float randomBurnChance;
        public float severeBurnDamage;
        public int severeBurnIntervalTicks;
        public float severeBurnChance;
        public float meltdownExplosionRadius;
        public float meltdownFireRadius;
        public float meltdownNapalmRadius;
        public DamageDef burnDamageDef;
        public DamageDef meltdownExplosionDamageDef;
        public DamageDef meltdownFireDamageDef;
        public int meltdownWarningIntervalTicks;
        public int heatstrokeMessageIntervalTicks;

        public float pressurizedThreshold;
        public float meltdownThreshold;
        public float pressurizedGlowSize;
        public float meltdownGlowSize;
        public int pressurizedGlowInterval;
        public int meltdownGlowInterval;
        public float meltdownKillDamage;
        public float meltdownKillArmorPenetration;

        public HediffCompProperties_FlamethrowerHeat()
        {
            compClass = typeof(HediffComp_FlamethrowerHeat);
        }
    }
}