using System.Collections.Generic;
using RimWorld;
using RimWorld.Utility;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    public class JobDriver_ReloadAPS : JobDriver
    {
        private const TargetIndex PawnInd = TargetIndex.A;
        private const TargetIndex AmmoInd = TargetIndex.B;

        private Pawn TargetPawn => job.GetTarget(TargetIndex.A).Pawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            IReloadableComp reloadable = FindReloadableHediffComponent(TargetPawn);

            this.FailOn(() => !reloadable.NeedsReload(allowForceReload: true));
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);

            Toil getNextIngredient = Toils_General.Label();
            yield return getNextIngredient;

            foreach (Toil item in ReloadAsMuchAsPossible(reloadable))
            {
                yield return item;
            }

            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.B)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true)
                .FailOnDestroyedNullOrForbidden(TargetIndex.B);

            yield return Toils_Jump.JumpIf(getNextIngredient, () => !job.GetTargetQueue(TargetIndex.B).NullOrEmpty());

            foreach (Toil item2 in ReloadAsMuchAsPossible(reloadable))
            {
                yield return item2;
            }

            Toil dropRemaining = ToilMaker.MakeToil("MakeNewToils");
            dropRemaining.initAction = delegate
            {
                Thing carriedThing = pawn.carryTracker.CarriedThing;
                if (carriedThing != null && !carriedThing.Destroyed)
                {
                    pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var _);
                }
            };
            dropRemaining.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return dropRemaining;
        }

        private IEnumerable<Toil> ReloadAsMuchAsPossible(IReloadableComp reloadable)
        {
            Toil done = Toils_General.Label();

            yield return Toils_Jump.JumpIf(done, () =>
                pawn.carryTracker.CarriedThing == null ||
                pawn.carryTracker.CarriedThing.stackCount < reloadable.MinAmmoNeeded(allowForcedReload: true));

            yield return Toils_General.Wait(reloadable.BaseReloadTicks).WithProgressBarToilDelay(TargetIndex.A);

            Toil doReload = ToilMaker.MakeToil("ReloadAsMuchAsPossible");
            doReload.initAction = delegate
            {
                Thing carriedThing = pawn.carryTracker.CarriedThing;
                reloadable.ReloadFrom(carriedThing);
            };
            doReload.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return doReload;

            yield return done;
        }

        private IReloadableComp FindReloadableHediffComponent(Pawn targetPawn)
        {
            if (targetPawn?.health?.hediffSet?.hediffs == null)
            {
                return null;
            }

            foreach (Hediff hediff in targetPawn.health.hediffSet.hediffs)
            {
                if (hediff.TryGetComp<HediffComp_APS>() is HediffComp_APS apsComp)
                {
                    return apsComp;
                }
            }

            return null;
        }
    }
}