using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
                    val += consumer.BandwidthAmount;
                }
                return val;
            }
        }
        public int FreeBandwidthLeft => RelayBandwidthAmount - RelayBandwidthInUse;
        private bool isRegistered = false;
        public float DrawPercentage => (float)RelayBandwidthInUse / (float)RelayBandwidthAmount;
        public float OverDrawPercentage => DrawPercentage - 1f;
        public bool IsOverdraw => DrawPercentage > 1f;
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
            if (consumer.BandwidthAmount > FreeBandwidthLeft)
            {
                //Needs a warning screen
                Logger.Warning($"Consumer needs more bandwidth {consumer.BandwidthAmount} than the relay can provide without overdraw {FreeBandwidthLeft}");
            }
            if (!consumers.Add(consumer))
            {
                Logger.Error("Consumer already connected");
                return false;
            }
            consumer.relay = parent;
            if(consumer.pawn.jobs.curJob != null)
            {
                consumer.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
            }
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
            consumer.relay = null;
            if (consumer.pawn.jobs.curJob != null)
            {
                consumer.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
            }
            if(consumer.pawn.health.hediffSet.HasHediff(CrimsonGridFramework_DefOfs.CG_GlobalBottleneck))
            {
                consumer.pawn.health.RemoveHediff(consumer.pawn.health.hediffSet.GetFirstHediffOfDef(CrimsonGridFramework_DefOfs.CG_GlobalBottleneck));
            }
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
        public override void PostPostMake()
        {
            base.PostPostMake();
            if (!isRegistered)
            {
                isRegistered = gridBandwidth.TryRegisterRelay(this);
            }
        }
        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);
            Logger.Message("Relay killed, unregistering");
            Unregister();
        }

        protected void Unregister()
        {
            foreach (var consumer in consumers.ToList())
            {
                TryDisconnectConsumer(consumer);
            }
            if (isRegistered)
            {
                isRegistered = !gridBandwidth.TryUnregisterRelay(this);
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
                if (consumer.IsSelfRelay)
                {
                    continue;
                }
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
        public override void Notify_MapRemoved()
        {
            base.Notify_MapRemoved();
            Unregister();
        }
        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map, mode);
            Logger.Message($"Despawning relay, unregistering, {mode.ToString()}");
            Unregister();
        }
        public override string CompInspectStringExtra()
        {
            string res = base.CompInspectStringExtra();
            if (IsEnabled)
            {
                res += $"Bandwidth: {FreeBandwidthLeft}/{RelayBandwidthAmount}";
            }

            return res;
        }
        public override void PostDrawExtraSelectionOverlays()
        {
            foreach (var consumer in consumers)
            {
                GenDraw.DrawLineBetween(parent.TrueCenter(), consumer.parent.TrueCenter());
            }
        }
    }
    public class CompBandwidthRelayEquipment : CompBandwidthRelay
    {
        public Pawn Pawn
        {
            get
            {
                Logger.Message($"{parent.ParentHolder == null}");
                Logger.Message(parent.ParentHolder.ParentHolder.GetType().ToString());
                if (parent.ParentHolder == null || parent.ParentHolder.ParentHolder is not Pawn p || p.Faction != Faction.OfPlayer)
                {
                    return null;
                }
                return (Pawn)parent.ParentHolder.ParentHolder;
            }
        }
        public override bool IsEnabled => true && Pawn != null && !Pawn.DeadOrDowned;
        public override string CompInspectStringExtra()
        {
            string res = base.CompInspectStringExtra();
            if (IsEnabled)
            {
                res += $"Bandwidth: {FreeBandwidthLeft}/{RelayBandwidthAmount}";
            }
            return res;
        }


        public override void Notify_AbandonedAtTile(PlanetTile tile)
        {
            base.Notify_AbandonedAtTile(tile);
            Logger.Message("Relay abandoned, unregistering");
            Unregister();
        }
    }

    public class CompBandwidthRelayPawn : CompBandwidthRelay
    {
        public Pawn Pawn => (Pawn)parent;
        public override bool IsEnabled => true && Pawn != null && !Pawn.Dead && Pawn.Faction == Faction.OfPlayer;
        public override string CompInspectStringExtra()
        {
            string res = base.CompInspectStringExtra();
            if (IsEnabled)
            {
                res += $"Bandwidth: {FreeBandwidthLeft}/{RelayBandwidthAmount}";
                if (IsOverdraw)
                {
                    res += $"\n<color=red>Overdrawn!</color>";
                }

            }
            return res;
        }
        public override void Notify_AbandonedAtTile(PlanetTile tile)
        {
            base.Notify_AbandonedAtTile(tile);
            Logger.Message("Relay abandoned, unregistering");
            Unregister();
        }
        public override void PostDrawExtraSelectionOverlays()
        {
            foreach (var consumer in consumers)
            {
                GenDraw.DrawLineBetween(parent.TrueCenter(), consumer.parent.TrueCenter());
            }
        }
    }

}
