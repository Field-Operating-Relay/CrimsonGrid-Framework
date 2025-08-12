using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework
{
    public class CompProperties_BandwidthRelay : CompProperties
    {
        public int bandwidthRelayAmount = 5;
    }
    public abstract class CompBandwidthRelay : ThingComp
    {
        CompProperties_BandwidthRelay Props => (CompProperties_BandwidthRelay)props;
        public int relayAmount => Props.bandwidthRelayAmount;
        public virtual bool IsEnabled => true;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            parent?.Map?.GetComponent<MapComponent_LocalBandwidth>().TryRegisterRelay(this);
        }
        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map, mode);
            parent?.Map?.GetComponent<MapComponent_LocalBandwidth>().TryUnregisterRelay(this);
        }
        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);
            parent?.Map?.GetComponent<MapComponent_LocalBandwidth>().TryUnregisterRelay(this);
        }
        public override void Notify_MapRemoved()
        {
            base.Notify_MapRemoved();
            parent?.Map?.GetComponent<MapComponent_LocalBandwidth>().TryUnregisterRelay(this);
        }
    }

    public class CompBandwidthRelayBuilding : CompBandwidthRelay
    {

    }

}
