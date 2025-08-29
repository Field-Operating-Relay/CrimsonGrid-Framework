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
    public class CompBandwidthProvider : ThingComp
    {
        public CompProperties_BandwidthProvider Props => (CompProperties_BandwidthProvider)props;
        WorldComponent_GridBandwidth gridBandwidth => WorldComponent_GridBandwidth.Instance;
        public int bandwidthAmount => Props.bandwidthAmount;

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
        private bool isRegistered = false;

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
                result += $"Providing {bandwidthAmount} bandwidth";
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
                }
            };
        }
    }
}
