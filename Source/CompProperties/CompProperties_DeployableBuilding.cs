using Verse;

namespace CrimsonGridFramework
{
	public class CompProperties_DeployableBuilding : CompProperties
	{
		public ThingDef inactiveDef;
		public int lifespanTicks;
		public bool turnToInactiveWhenDestroyed;
		public int maxShots = -1;
		public SoundDef soundDeploy;
		public SoundDef soundExpire;

		public CompProperties_DeployableBuilding()
		{
			compClass = typeof(CompDeployableBuilding);
		}
	}
}
