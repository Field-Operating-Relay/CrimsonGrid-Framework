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
            return pawn.GetBandwidthComp()?.IsConnected ?? false;
        }
    }
}
