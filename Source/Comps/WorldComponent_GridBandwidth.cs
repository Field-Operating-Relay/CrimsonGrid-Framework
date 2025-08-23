using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework
{
    public class WorldComponent_GridBandwidth : WorldComponent
    {
        public WorldComponent_GridBandwidth(World world) : base(world)
        {
        }
        #region Singleton
        private static WorldComponent_GridBandwidth _instance;
        public static WorldComponent_GridBandwidth Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WorldComponent_GridBandwidth(Find.World);
                }
                return _instance;
            }
        }
        #endregion

        public HashSet<CompBandwidthProvider> bandwidthProviders = [];
        public HashSet<CompBandwidthRelay> relays = [];
        public int TotalBandwidth
        {
            get
            {
                int val = 0;
                foreach (CompBandwidthProvider provider in bandwidthProviders)
                {
                    val += provider.bandwidthAmount;
                }
                return val;
            }
        }
        public int TotalBandwidthInUse
        {
            get
            {
                int val = 0;
                foreach(var relay in relays)
                {
                    if (!relay.IsEnabled)
                    {
                        continue;
                    }
                    if(relay.consumers.Count == 0)
                    {
                        continue;
                    }
                    val += relay.RelayBandwidthInUse;
                }
                return val;
            }
        }
        public int UnusuedBandwidth => TotalBandwidth - TotalBandwidthInUse;
        public bool IsOverdraw => TotalBandwidthInUse > TotalBandwidth;
        public float OverDrawAmount => TotalBandwidthInUse / TotalBandwidth;

        public bool HasProviders => bandwidthProviders.Count > 0;
        public bool HasRelays => relays.Count > 0;
        public IEnumerable<CompBandwidthRelay> relaysInMap(Map map)
        {
            foreach(var relay in relays)
            {
                if(relay.parent.Map == map)
                {
                    yield return relay;
                }
            }
        }
        public bool TryRegisterProvider(CompBandwidthProvider provider)
        {
            if (provider == null)
            {
                Logger.Error("provider is null");
                return false;
            }
            if (bandwidthProviders == null)
            {
                Logger.Error("BandwidthProviders is null");
                return false;
            }
            if (provider.parent.Faction != Find.FactionManager.OfPlayer)
            {
                Logger.Error("Provider is not of player faction");
                return false;
            }
            if (provider.bandwidthAmount == 0)
            {
                Logger.Error("Bandwidth Amount must not be 0");
                return false;
            }
            if (!bandwidthProviders.Add(provider))
            {
                Logger.Error("Provider already present");
                return false;
            }
            Logger.Message($"Registered {provider.parent.Label}");
            return true;
        }
        public bool TryUnregisterProvider(CompBandwidthProvider provider)
        {
            if (provider == null)
            {
                Logger.Error("provider is null");
                return false;
            }
            if (bandwidthProviders == null)
            {
                Logger.Error("BandwidthProviders is null");
                return false;
            }
            if (!bandwidthProviders.Remove(provider))
            {
                Logger.Error("Provider already removed");
                return false;
            }
            Logger.Message($"Unregistered {provider.parent.Label}");
            return true;
        }

        public bool TryRegisterRelay(CompBandwidthRelay relay)
        {
            if (relay == null)
            {
                Logger.Error("relay is null");
                return false;
            }
            if (relays == null)
            {
                Logger.Error("relays are null");
                return false;
            }
            if (!relays.Add(relay))
            {
                Logger.Error("relay already present");
                return false;
            }
            Logger.Message($"Registered relay");
            return true;
        }
        public bool TryUnregisterRelay(CompBandwidthRelay relay)
        {
            if (relay == null)
            {
                Logger.Error("relay is null");
                return false;
            }
            if (relays == null)
            {
                Logger.Error("relays are null");
                return false;
            }
            if (!relays.Remove(relay))
            {
                Logger.Error("relay already removed");
                return false;
            }
            Logger.Message($"Unregistered relay");
            return true;
        }


    }
}
