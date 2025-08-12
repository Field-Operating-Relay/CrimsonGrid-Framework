using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework
{
    public class MapComponent_LocalBandwidth : MapComponent
    {
        WorldComponent_GridBandwidth gridBandwidth => WorldComponent_GridBandwidth.Instance;
        public HashSet<CompBandwidthRelay> bandwidthRelays;
        public MapComponent_LocalBandwidth(Map map) : base(map)
        {
        }
        public bool TryRegisterRelay(CompBandwidthRelay relay)
        {
            if (relay == null)
            {
                Logger.Error("relay is null");
                return false;
            }
            if (bandwidthRelays == null)
            {
                Logger.Error("Bandwidthrelays is null");
                return false;
            }
            if (relay.parent.Faction == Find.FactionManager.OfPlayer)
            {
                return false;
            }
            if (relay.relayAmount == 0)
            {
                Logger.Error("Bandwidth Amount must not be 0");
                return false;
            }
            if (!bandwidthRelays.Add(relay))
            {
                Logger.Error("Relay already present");
                return false;
            }
            Logger.Message($"Registered {relay.parent.Label}");
            return true;
        }
        public bool TryUnregisterRelay(CompBandwidthRelay relay)
        {
            if (relay == null)
            {
                Logger.Error("Relay is null");
                return false;
            }
            if (bandwidthRelays == null)
            {
                Logger.Error("Bandwidth Relays is null");
                return false;
            }
            if (!bandwidthRelays.Remove(relay))
            {
                Logger.Error("relay already removed");
                return false;
            }
            Logger.Message($"Unregistered {relay.parent.Label}");
            return true;
        }


    }
}
