using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    public class JobDriver_Disconnected : JobDriver
    {
        public const TargetIndex RestSpotIndex = TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            Toil layDown = ToilMaker.MakeToil("Disconnected");
            layDown.initAction = delegate
            {
                Pawn actor = layDown.actor;
                actor.pather?.StopDead();
                JobDriver curDriver = actor.jobs.curDriver;
                actor.jobs.posture = PawnPosture.LayingOnGroundNormal;
                actor.mindState.lastBedDefSleptIn = null;
                curDriver.asleep = true;
                if (actor.Drafted)
                {
                    actor.drafter.Drafted = false;
                }
            };
            layDown.defaultCompleteMode = ToilCompleteMode.Never;
            layDown.tickIntervalAction = delegate
            {
                if(pawn.IsConnected())
                {
                    layDown.actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                }
            };
            layDown.AddFinishAction(delegate
            {
                layDown.actor.jobs.curDriver.asleep = false;
            });
            yield return layDown;
        }
    }
}
