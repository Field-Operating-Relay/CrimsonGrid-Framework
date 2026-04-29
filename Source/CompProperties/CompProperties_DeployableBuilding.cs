using Verse;

namespace CrimsonGridFramework
{
    public class CompProperties_DeployableBuilding : CompProperties
    {
        public ThingDef inactiveDef;
        public int lifespanTicks;

        public CompProperties_DeployableBuilding()
        {
            compClass = typeof(CompDeployableBuilding);
        }
    }
}
