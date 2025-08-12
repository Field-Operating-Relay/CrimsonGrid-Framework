/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework
{
    public class CompProperties_BandwidthConsumer : CompProperties
    {
        public int bandwidthAmount = 5;
        public CompProperties_BandwidthConsumer()
        {
            compClass = typeof(CompBandwidthConsumer);
        }
    }
    public class CompBandwidthConsumer : ThingComp
    {
        public CompProperties_BandwidthConsumer Props => (CompProperties_BandwidthConsumer)props;
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
        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);
        }
    }
}
*/