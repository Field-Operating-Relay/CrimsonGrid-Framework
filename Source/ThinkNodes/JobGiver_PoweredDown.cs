using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    public class JobGiver_PoweredDown : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Job job = JobMaker.MakeJob(CrimsonGridFramework_DefOfs.CG_PoweredDown);
            job.forceSleep = true;
            return job;
        }
    }
}
