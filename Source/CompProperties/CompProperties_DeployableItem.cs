using Verse;

namespace CrimsonGridFramework
{
    public class CompProperties_DeployableItem : CompProperties
    {
        public ThingDef buildingDef;
        public int capacityCost;
        public int installDuration;

        public CompProperties_DeployableItem()
        {
            compClass = typeof(CompDeployableItem);
        }
    }
}
