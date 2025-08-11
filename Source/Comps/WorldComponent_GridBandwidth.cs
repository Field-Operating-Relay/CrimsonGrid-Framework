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

        public HashSet<CompBandwidthProvider> bandwidthProviders;
        //public Dictionary<>
        public int TotalBandwidth
        {
            get
            {
                int val = 0;
                foreach(CompBandwidthProvider provider in bandwidthProviders)
                {
                    val += provider.bandwidthAmount;
                }
                return val;
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
            if (provider.parent.Faction == Find.FactionManager.OfPlayer)
            {
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



    }
}
