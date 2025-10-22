using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    public class JobDriver_RefuelRobot : JobDriver
    {
        public Pawn robot => job.targetB.Pawn;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            this.FailOnDowned(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
            Toil toil = Toils_General.WaitWith(TargetIndex.B, 500, true, true, false, TargetIndex.B);
            toil.AddFinishAction(() =>
            {
                robot.AddFuel(job.count);
                GetActor().carryTracker.CarriedThing.Destroy();
            });
            yield return toil;
        }
    }
}
