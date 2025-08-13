using System;
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
        Pawn pawn => (Pawn)parent;
        public CompProperties_BandwidthConsumer Props => (CompProperties_BandwidthConsumer)props;
        WorldComponent_GridBandwidth gridBandwidth => WorldComponent_GridBandwidth.Instance;
        public int bandwidthAmount => Props.bandwidthAmount;

        public CompBandwidthRelay connectedRelay;
        public bool DeadOrDowned => pawn.DeadOrDowned;
        public bool IsConnected => connectedRelay != null;
        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);
            if(IsConnected)
            {
                connectedRelay.TryDisconnectConsumer(this);
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref connectedRelay, "relay");
        }
        //TODO
        // Add gizmo for disconnecting and powering down
    }
}
