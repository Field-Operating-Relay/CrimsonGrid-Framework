using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace CrimsonGridFramework
{
    public class TransportersArrivalAction_SpawnRocketProjectileOnTarget : TransportersArrivalAction
    {
        public override bool GeneratesMap => false;
        public ThingDef ProjectileToSpawn;
        public GlobalTargetInfo GlobalTargetInfo;
        public Thing launcher;

        public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
        {
            IntVec3 spawnCell = new IntVec3(GlobalTargetInfo.Cell.x, 0, CellRect.WholeMap(GlobalTargetInfo.Map).maxZ);
            Projectile projectile2 = (Projectile)GenSpawn.Spawn(ProjectileToSpawn, spawnCell, GlobalTargetInfo.Map);
            projectile2.Launch(launcher, GlobalTargetInfo.Cell, GlobalTargetInfo.Cell, ProjectileHitFlags.None, preventFriendlyFire: false);

        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref ProjectileToSpawn, "ProjectileToSpawn");
            Scribe_TargetInfo.Look(ref GlobalTargetInfo, "GlobalTargetInfo");
            Scribe_References.Look(ref launcher, "launcher");
        }
    }
}
