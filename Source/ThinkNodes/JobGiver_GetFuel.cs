using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    public class JobGiver_GetFuel : ThinkNode_JobGiver
    { 
        protected override Job TryGiveJob(Pawn pawn)
        {
            return pawn.GetFuelJob();
        }
    }
}
