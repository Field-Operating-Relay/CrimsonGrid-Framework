using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    public class CompProperties_BandwidthRelay : CompProperties
    {
        public int bandwidthRelayAmount = 5;
    }
    public abstract class CompBandwidthRelay : ThingComp
    {
        WorldComponent_GridBandwidth gridBandwidth => WorldComponent_GridBandwidth.Instance;
        CompProperties_BandwidthRelay Props => (CompProperties_BandwidthRelay)props;
        public HashSet<CompBandwidthConsumer> consumers = [];
        public bool AnyGridBandwidth => gridBandwidth.TotalBandwidth > 0;
        public int RelayBandwidthAmount => Props.bandwidthRelayAmount;
        public int RelayBandwidthInUse
        {
            get
            {
                int val = 0;
                foreach (var consumer in consumers)
                {
                    if (!consumer.DeadOrDowned)
                    {
                        val += consumer.bandwidthAmount;
                    }
                }
                return val;
            }
        }
        public int FreeBandwidthLeft => RelayBandwidthAmount - RelayBandwidthInUse;
        private bool isRegistered = false;
        public bool IsOverdraw => RelayBandwidthInUse > RelayBandwidthAmount;
        public float OverDrawPercentage => (float)RelayBandwidthInUse / RelayBandwidthAmount;
        public virtual bool IsEnabled => AnyGridBandwidth;

        public bool TryConnectConsumer(CompBandwidthConsumer consumer)
        {
            if (consumer == null)
            {
                Logger.Error("Consumer is null");
                return false;
            }
            if (consumers == null)
            {
                Logger.Error("Consumers are null");
                return false;
            }
            if (consumer.bandwidthAmount > FreeBandwidthLeft)
            {
                //Needs a warning screen
                Logger.Warning($"Consumer needs more bandwidth {consumer.bandwidthAmount} than the relay can provide without overdraw {FreeBandwidthLeft}");
            }
            if (!consumers.Add(consumer))
            {
                Logger.Error("Consumer already connected");
                return false;
            }
            consumer.connectedRelay = this;
            return true;

        }
        public bool TryDisconnectConsumer(CompBandwidthConsumer consumer)
        {
            if (consumer == null)
            {
                Logger.Error("Consumer is null");
                return false;
            }
            if (consumers == null)
            {
                Logger.Error("Consumers are null");
                return false;
            }
            if (!consumers.Remove(consumer))
            {
                Logger.Error("Consumer already disconnected");
                return false;
            }
            consumer.connectedRelay = null;
            return true;

        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!isRegistered)
            {
                isRegistered = gridBandwidth.TryRegisterRelay(this);
            }
        }
        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map, mode);
            if (isRegistered)
            {
                isRegistered = !gridBandwidth.TryUnregisterRelay(this);
            }
        }
        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);
            if (isRegistered)
            {
                isRegistered = !gridBandwidth.TryUnregisterRelay(this);
            }
        }
        public override void Notify_AbandonedAtTile(PlanetTile tile)
        {
            base.Notify_AbandonedAtTile(tile);
            if (isRegistered)
            {
                isRegistered = !gridBandwidth.TryUnregisterRelay(this);
            }
        }
        public override void Notify_MapRemoved()
        {
            base.Notify_MapRemoved();
            if (isRegistered)
            {
                isRegistered = !gridBandwidth.TryUnregisterRelay(this);
            }
        }
        public override void Notify_PassedToWorld()
        {
            base.Notify_PassedToWorld();
            if (isRegistered)
            {
                isRegistered = !gridBandwidth.TryUnregisterRelay(this);
            }
        }
        public override void PostDrawExtraSelectionOverlays()
        {
            foreach (var consumer in consumers)
            {
                GenDraw.DrawLineBetween(parent.TrueCenter(), consumer.parent.TrueCenter());
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            foreach (var consumer in consumers)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "Select Connected Consumer";
                command_Action.defaultDesc = "A";
                command_Action.icon = null;
                command_Action.Order = -87f;
                command_Action.action = delegate
                {
                    Find.Selector.ClearSelection();
                    Find.Selector.Select(consumer.parent);
                };
                yield return command_Action;
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref consumers, "consumers", LookMode.Deep);
        }
    }

    public class CompBandwidthRelayBuilding : CompBandwidthRelay
    {
        private CompPowerTrader powerTrader;
        public CompPowerTrader Power
        {
            get
            {
                if (powerTrader == null)
                {
                    powerTrader = parent.GetComp<CompPowerTrader>();
                }
                return powerTrader;
            }
        }
        public override bool IsEnabled => base.IsEnabled && parent.Faction == Find.FactionManager.OfPlayer && Power != null && Power.PowerOn;
        public override string CompInspectStringExtra()
        {
            string res = base.CompInspectStringExtra();
            if(IsEnabled)
            {
                res += $"Bandwidth: {FreeBandwidthLeft}/{RelayBandwidthAmount}";
            }

            return res;
        }
    }

}
