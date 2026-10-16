using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    public class JobDriver_DeliverUpgradeMaterials : JobDriver
    {
        public Thing Item => job.GetTarget(TargetIndex.A).Thing;

        protected CompUpgradeableBuilding Comp => job.GetTarget(TargetIndex.B).Thing.TryGetComp<CompUpgradeableBuilding>();

        protected bool FailJob()
        {
            return !CompUpgradeableBuilding.buildingsWithUpgradeInProgress.Contains(job.GetTarget(TargetIndex.B).Thing);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!pawn.Reserve(Item, job))
            {
                return false;
            }
            pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnForbidden(TargetIndex.A);
            this.FailOn(FailJob);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.B);
            yield return Toils_General.Wait(25).WithProgressBarToilDelay(TargetIndex.B);
            yield return GiveAsMuchToBuildingAsPossible();
        }

        protected Toil GiveAsMuchToBuildingAsPossible()
        {
            return new Toil
            {
                initAction = delegate
                {
                    if (Item == null || Item.stackCount <= 0)
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                    }
                    else
                    {
                        ThingDefCountClass thingDefCountClass = Comp.TargetUpgrade.ingredients.FirstOrDefault((ThingDefCountClass x) => x.thingDef == Item.def);
                        if (thingDefCountClass == null || thingDefCountClass.count <= 0)
                        {
                            pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                        }
                        else
                        {
                            int count = Mathf.Min(thingDefCountClass.count, Item.stackCount);
                            Comp.AddToContainer(pawn.carryTracker.innerContainer, Item, count);
                        }
                    }
                }
            };
        }
    }
}
