using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Utility;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    public class JobGiver_ReloadAPS : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            return 5.9f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return null;
            }

            IReloadableComp reloadableComp = FindReloadableHediffComponent(pawn, allowForcedReload: false);
            if (reloadableComp == null)
            {
                return null;
            }

            if (pawn.carryTracker.AvailableStackSpace(reloadableComp.AmmoDef) < reloadableComp.MinAmmoNeeded(allowForcedReload: true))
            {
                return null;
            }

            List<Thing> ammoList = ReloadableUtility.FindEnoughAmmo(pawn, pawn.Position, reloadableComp, forceReload: false);
            if (ammoList.NullOrEmpty())
            {
                return null;
            }

            return MakeReloadJob(reloadableComp, ammoList);
        }

        private IReloadableComp FindReloadableHediffComponent(Pawn pawn, bool allowForcedReload)
        {
            if (pawn.health?.hediffSet?.hediffs == null)
            {
                return null;
            }

            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff.TryGetComp<HediffComp_APS>() is HediffComp_APS apsComp)
                {
                    if (apsComp.NeedsReload(allowForcedReload))
                    {
                        return apsComp;
                    }
                }
            }

            return null;
        }

        public static Job MakeReloadJob(IReloadableComp reloadable, List<Thing> chosenAmmo)
        {
            Job job = JobMaker.MakeJob(CrimsonGridFramework_DefOfs.CG_ReloadAPS, reloadable.ReloadableThing);
            job.targetQueueB = chosenAmmo.Select((Thing t) => new LocalTargetInfo(t)).ToList();
            job.count = chosenAmmo.Sum((Thing t) => t.stackCount);
            job.count = Math.Min(job.count, reloadable.MaxAmmoNeeded(allowForcedReload: true));
            return job;
        }
    }
}