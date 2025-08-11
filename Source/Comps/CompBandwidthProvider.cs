using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework
{
    public class CompProperties_BandwidthProvider : CompProperties
    {
        public int bandwidthAmount = 5;
        public CompProperties_BandwidthProvider()
        {
            compClass = typeof(CompBandwidthProvider);
        }
    }
    public class CompBandwidthProvider : ThingComp
    {
        public CompProperties_BandwidthProvider Props => (CompProperties_BandwidthProvider)props;
        WorldComponent_GridBandwidth gridBandwidth => WorldComponent_GridBandwidth.Instance;
        public int bandwidthAmount => Props.bandwidthAmount;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            gridBandwidth.TryRegisterProvider(this);
        }
        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map, mode);
            gridBandwidth.TryUnregisterProvider(this);
        }
    }
}
