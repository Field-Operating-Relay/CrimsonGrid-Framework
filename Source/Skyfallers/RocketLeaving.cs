using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework
{
    public class RocketLeaving : FlyShipLeaving
    {
        //Fire and Smoke animations should go here maybe no sure yet
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            EffecterDefOf.ImpactDustCloud.Spawn(Position, map);

        }
        protected override void Tick()
        {
            base.Tick();
        }

    }
}
