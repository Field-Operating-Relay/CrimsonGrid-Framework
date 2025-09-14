using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CrimsonGridFramework
{
    public struct BandwidthOverclockStage
    {
        public int BonusBandwidth;
        public float AdditionalHeatPerSecond;
        public int PowerConsumption;
        public BandwidthOverclockStage(int bonusBandwidth, float additionalHeatPerSecond, int powerConsumption)
        {
            BonusBandwidth = bonusBandwidth;
            AdditionalHeatPerSecond = additionalHeatPerSecond;
            PowerConsumption = powerConsumption;
        }
    }
    public class CompProperties_BandwidthOverclock : CompProperties
    {
        public List<BandwidthOverclockStage> stages = new List<BandwidthOverclockStage>();
        public CompProperties_BandwidthOverclock()
        {
            compClass = typeof(CompBandwidthOverclock);
        }
    }
    public class CompBandwidthOverclock : ThingComp, IBandwidthBooster
    {
        private bool enabled = false;
        public bool Enabled => enabled;
        private int overclock = 0;
        public int Overclock { get { return overclock; } set { overclock = value; } }
        public IBuildingWithBandwidth buildingWithBandwidth;
        public CompPowerTrader compPower;
        public CompHeatPusher heatPusher;
        public float TargetStage;
        private float defaultPowerOutput;
        public int overclockStages => Props.stages.Count;

        public CompProperties_BandwidthOverclock Props => (CompProperties_BandwidthOverclock)props;

        public int BandwidthBoostAmount => Props.stages[Overclock].BonusBandwidth;

        public CompBandwidthOverclock() { }
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            compPower = parent.GetComp<CompPowerTrader>();
            defaultPowerOutput = compPower.PowerOutput;
            heatPusher = parent.GetComp<CompHeatPusher>();
            buildingWithBandwidth = (IBuildingWithBandwidth)parent.AllComps.Where(c => c is IBuildingWithBandwidth).First();
            buildingWithBandwidth.TryBoostBandwidth(this);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref overclock, "overclock", 0);
            Scribe_Values.Look(ref enabled, "enabled", false);
        }
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            if (buildingWithBandwidth != null && enabled)
            {
                buildingWithBandwidth.TryUnboostBandwidth(this);
            }
        }

        public override string CompInspectStringExtra()
        {
            string text = base.CompInspectStringExtra();
            text = ((text != null) ? (text + "\n") : string.Empty);
            if (!enabled)
            {
                text += "CGF_OverclockingDisabled".Translate();
            }
            else
            {
                text += "CGF_Overclocking".Translate(Props.stages[Overclock].BonusBandwidth);
            }
            return text;
        }
        public override void CompTick()
        {
            base.CompTick();
            if (buildingWithBandwidth.Enabled && enabled)
            {
                if (heatPusher != null && heatPusher.enabled && parent.IsHashIntervalTick(60))
                {
                    GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, Props.stages[Overclock].AdditionalHeatPerSecond);
                }
                int targetStage = (int)Math.Round(TargetStage * (overclockStages - 1));
                targetStage = Math.Max(0, Math.Min(overclockStages - 1, targetStage));
                if (targetStage != Overclock && targetStage < overclockStages)
                {
                    Log.Message("Rah " + targetStage);
                    Overclock = targetStage;
                }
            }
            compPower.PowerOutput = defaultPowerOutput - Props.stages[Overclock].PowerConsumption;
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            if (parent.Faction == Faction.OfPlayer)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "CGF_EnableOverclocking".Translate(),
                    defaultDesc = "CGF_EnableOverclockingDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Overclock", true),
                    isActive = () => enabled,
                    toggleAction = delegate
                    {
                        enabled = !enabled;
                        if (enabled)
                        {
                            buildingWithBandwidth.TryBoostBandwidth(this);
                        }
                        else
                        {
                            buildingWithBandwidth.TryUnboostBandwidth(this);
                            Overclock = 0;
                            TargetStage = 0;
                        }
                    },
                    Order = -100f
                };
                if (enabled && Find.Selector.SelectedObjects.Count == 1)
                {
                    yield return new OverclockGizmo(this);
                }
            }
        }
    }
}
