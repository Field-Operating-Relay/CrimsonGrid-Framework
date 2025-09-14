using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
    public class CompBandwidthProvider : ThingComp, IBuildingWithBandwidth
    {
        public CompProperties_BandwidthProvider Props => (CompProperties_BandwidthProvider)props;
        WorldComponent_GridBandwidth gridBandwidth => WorldComponent_GridBandwidth.Instance;
        public List<IBandwidthBooster> additionalBandwidthBoosts = [];
        public int BandwidthAmount => Props.bandwidthAmount + GetBoostedBandwidth();
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

        public bool Enabled => CanProvideBandwidth();
        private bool isRegistered = false;

        private int GetBoostedBandwidth()
        {
            int totalBoost = 0;
            foreach (IBandwidthBooster booster in additionalBandwidthBoosts)
            {
                if (booster != null && booster.Enabled)
                {
                    totalBoost += booster.BandwidthBoostAmount;
                }
            }
            return totalBoost;
        }
        public override void CompTick()
        {
            base.CompTick();
            if (parent.IsHashIntervalTick(60))
            {
                bool shouldBeRegistered = CanProvideBandwidth();
                if (shouldBeRegistered && !isRegistered)
                {
                    isRegistered = gridBandwidth.TryRegisterProvider(this);
                }
                if (!shouldBeRegistered && isRegistered)
                {
                    isRegistered = !gridBandwidth.TryUnregisterProvider(this);
                }

            }


        }
        private bool CanProvideBandwidth()
        {
            return parent.Faction == Find.FactionManager.OfPlayer && Power != null && Power.PowerOn;
        }
        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map, mode);
            if (isRegistered)
            {
                isRegistered = !gridBandwidth.TryUnregisterProvider(this);

            }
        }
        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);
            if (isRegistered && gridBandwidth.TryUnregisterProvider(this))
            {
                isRegistered = false;

            }
        }
        public override string CompInspectStringExtra()
        {
            string result = base.CompInspectStringExtra();
            if (isRegistered)
            {
                result += $"Providing {BandwidthAmount} bandwidth";
            }
            return result;
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach(Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            yield return new Command_Action()
            {
                defaultLabel = "Test",
                icon = null,
                action = () =>
                {
                    Logger.Message($"Total Bandwidth: {gridBandwidth.TotalBandwidth}");
                    Logger.Message($"Boosters: {additionalBandwidthBoosts.Count()}, {additionalBandwidthBoosts.First().BandwidthBoostAmount}");
                }
            };
        }

        public bool TryBoostBandwidth(IBandwidthBooster booster)
        {
            if (booster == null || additionalBandwidthBoosts.Contains(booster))
            {
                return false;
            }
            additionalBandwidthBoosts.Add(booster);
            return true;
        }

        public bool TryUnboostBandwidth(IBandwidthBooster booster)
        {
            if (booster == null || !additionalBandwidthBoosts.Contains(booster))
            {
                return false;
            }
            additionalBandwidthBoosts.Remove(booster);
            return true;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref additionalBandwidthBoosts, "additionalBandwidthBoosts", LookMode.Deep);
        }
    }
}
