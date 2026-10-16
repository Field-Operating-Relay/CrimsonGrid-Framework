using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    public static class RobotHelperMethods
    {
        public static CompBandwidthConsumer GetBandwidthComp(this Pawn pawn)
        {
            return pawn.TryGetComp<CompBandwidthConsumer>();
        }
        public static bool IsConnected(this Pawn pawn)
        {
            var bandwidthConnected = pawn.GetBandwidthComp()?.IsConnected ?? false;

            if (!bandwidthConnected)
                return false;

            if (HasPendingSurgery(pawn))
                return false;

            return true;
        }
        public static void AddFuel(this Pawn pawn, float transferAmount, CompFuelStation station = null)
        {
            pawn.needs.TryGetNeed<Need_Fuel>(out var fuelNeed);
            fuelNeed.CurLevel += transferAmount * pawn.GetStatValue(CrimsonGridFramework_DefOfs.CG_RefuelEfficiencyFactor);
            station?.RefuelableComp.ConsumeFuel(transferAmount);
        }
        public static bool IsFueled(this Pawn pawn)
        {
            pawn.needs.TryGetNeed<Need_Fuel>(out var fuelNeed);
            return fuelNeed != null && fuelNeed.CurLevelPercentage > 0.01f;
        }
        public static int NeededFuelAmount(this Pawn pawn)
        {
            pawn.needs.TryGetNeed<Need_Fuel>(out var fuelNeed);
            return (int)((fuelNeed.MaxLevel - fuelNeed.CurLevel) / pawn.GetStatValue(CrimsonGridFramework_DefOfs.CG_RefuelEfficiencyFactor));
        }
        public static bool FullFuel(this Pawn pawn)
        {
            pawn.needs.TryGetNeed<Need_Fuel>(out var fuelNeed);
            return fuelNeed != null && fuelNeed.CurLevelPercentage > 0.98f;
        }
        public static bool NeedsFuel(this Pawn pawn)
        {
            pawn.needs.TryGetNeed<Need_Fuel>(out var fuelNeed);
            Log.Message(fuelNeed.CurLevelPercentage);
            return fuelNeed != null && fuelNeed.CurLevelPercentage < 0.2f;
        }
        public static Job GetFuelJob(this Pawn pawn)
        {
            return JobMaker.MakeJob(CrimsonGridFramework_DefOfs.CG_GetFuelJob, GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.listerBuildings.allBuildingsColonist.Where(b => b.TryGetComp<CompFuelStation>() is CompFuelStation comp && comp.HasFuel), PathEndMode.InteractionCell, TraverseParms.For(pawn)));
        }
        public static bool IsCrimsonGridRobot(this Pawn pawn)
        {
            return pawn.GetBandwidthComp() != null;
        }
        public static void ApplyGlobalBottleneck(CompBandwidthConsumer consumer)
        {

        }
        private static bool HasPendingSurgery(Pawn pawn)
        {
            // Check if the robot has any surgery bills that should be done now
            if (pawn.health?.surgeryBills?.AnyShouldDoNow == true)
            {
                return true;
            }

            // Check if robot is currently undergoing surgery
            if (pawn.CurJob?.def?.defName == "DoBill" && pawn.CurJob.bill?.recipe?.IsSurgery == true)
            {
                return true;
            }

            return false;
        }
    }
}
