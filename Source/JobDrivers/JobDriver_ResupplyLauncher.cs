using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CrimsonGridFramework
{
    public class JobDriver_ResupplyLauncher : JobDriver
    {
        private Building_TurretGunTopless Turret => (Building_TurretGunTopless)job.GetTarget(TargetIndex.A).Thing;
        private CompTopDownArtillery MagazineComp => Turret.TryGetComp<CompTopDownArtillery>();
        private Thing Hauling => job.GetTarget(TargetIndex.B).Thing;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed) && pawn.Reserve(job.targetB, job, 10, 1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.WaitWith(TargetIndex.A, 60).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            Toil loadRocket = ToilMaker.MakeToil("MakeNewToils");
            loadRocket.initAction = delegate
            {
                Pawn actor = loadRocket.actor;
                SoundDefOf.Artillery_ShellLoaded.PlayOneShot(new TargetInfo(Turret.Position, Turret.Map));
                MagazineComp.LoadShells(Hauling.def, Hauling.stackCount);
                actor.carryTracker.innerContainer.ClearAndDestroyContents();
            };
            yield return loadRocket;
        }
    }
}
