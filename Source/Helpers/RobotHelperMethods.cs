using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

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
