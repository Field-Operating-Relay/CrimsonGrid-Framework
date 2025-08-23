using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    public class JobGiver_Disconnected : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (RCellFinder.TryFindRandomMechSelfShutdownSpot(pawn.Position, pawn, pawn.Map, out var result))
            {
                Job job = JobMaker.MakeJob(DefOfs.Disconnected, result);
                job.forceSleep = true;
                return job;
            }
            return null;
        }
    }
}
