using Verse;

namespace CrimsonGridFramework
{
    public class CompFlamethrower : ThingComp
    {
        public CompProperties_Flamethrower Props => (CompProperties_Flamethrower)props;

        public void OnShot(Pawn shooter)
        {
            if (shooter == null) return;

            var hediff = shooter.health.GetOrAddHediff(Props.heatHediff);
            hediff.TryGetComp<HediffComp_FlamethrowerHeat>().AddHeat(Props.heatPerShot);
        }
    }
}
