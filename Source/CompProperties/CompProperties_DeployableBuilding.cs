using Verse;

namespace CrimsonGridFramework
{
    public class CompProperties_DeployableBuilding : CompProperties
    {
        public ThingDef inactiveDef;
        public int lifespanTicks;
        public bool turnToInactiveWhenDestroyed;

        public CompProperties_DeployableBuilding()
        {
            compClass = typeof(CompDeployableBuilding);
        }
    }
}
