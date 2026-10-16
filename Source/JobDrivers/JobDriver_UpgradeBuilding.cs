using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    public class JobDriver_UpgradeBuilding : JobDriver
    {
        private const int JobEndInterval = 5000;

        private const TargetIndex BuildingInd = TargetIndex.A;

        private CompUpgradeableBuilding Comp => job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompUpgradeableBuilding>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            Toil upgrade = ToilMaker.MakeToil("MakeNewToils");
            upgrade.initAction = delegate
            {
                GenClamor.DoClamor(upgrade.actor, 15f, ClamorDefOf.Construction);
            };
            upgrade.tickIntervalAction = delegate (int delta)
            {
                Pawn actor = upgrade.actor;
                if (actor.skills != null)
                {
                    actor.skills.Learn(SkillDefOf.Construction, 0.25f * (float)delta);
                }
                actor.rotationTracker.FaceTarget(job.GetTarget(TargetIndex.A));
                float num = actor.GetStatValue(StatDefOf.ConstructionSpeed) * 1.7f * (float)delta;
                Comp.upgradeWorkLeft -= num;
                if (Comp.upgradeWorkLeft <= 0)
                {
                    Comp.FinishUpgrade();
                    ReadyForNextToil();
                }
            };
            upgrade.WithEffect(EffecterDefOf.ConstructMetal, TargetIndex.A);
            upgrade.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            upgrade.FailOn(FailJob);
            upgrade.defaultCompleteMode = ToilCompleteMode.Delay;
            upgrade.defaultDuration = JobEndInterval;
            upgrade.activeSkill = () => SkillDefOf.Construction;
            upgrade.handlingFacing = true;
            yield return upgrade;
        }
        protected bool FailJob()
        {
            return !CompUpgradeableBuilding.buildingsWithUpgradeInProgress.Contains(job.GetTarget(TargetIndex.A).Thing);
        }
    }
}
