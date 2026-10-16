using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }

    [HotSwappable]
    public class CompDeployableItem : ThingComp
    {
        public CompProperties_DeployableItem Props => (CompProperties_DeployableItem)props;
        public Pawn Pawn => (parent.ParentHolder as Pawn_InventoryTracker).pawn;

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (!selPawn.CanReach(parent, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                yield return new FloatMenuOption("CannotPickUp".Translate(parent.Label, parent) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                yield break;
            }
            if (MassUtility.WillBeOverEncumberedAfterPickingUp(selPawn, parent, 1))
            {
                yield return new FloatMenuOption("CannotPickUp".Translate(parent.Label, parent) + ": " + "TooHeavy".Translate(), null);
                yield break;
            }
            var maxCapacity = GetMaxCapacity();
            var used = 0f;
            foreach (var t in selPawn.inventory.innerContainer)
            {
                if (t.def == parent.def) used += t.stackCount;
            }
            int maxAllowedToPickUp = (int)Mathf.Max(0, maxCapacity - used);
            if (maxAllowedToPickUp == 0)
            {
                yield return new FloatMenuOption("CannotPickUp".Translate(parent.Label, parent) + ": " + "MaxPickUpAllowed".Translate(maxCapacity, parent.Label), null);
                yield break;
            }
            if (parent.stackCount == 1 || maxAllowedToPickUp == 1)
            {
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUpOne".Translate(parent.LabelNoCount, parent), delegate
                {
                    parent.SetForbidden(value: false, warnOnFail: false);
                    Job job3 = JobMaker.MakeJob(JobDefOf.TakeInventory, parent);
                    job3.count = 1;
                    job3.checkEncumbrance = true;
                    job3.takeInventoryDelay = 120;
                    selPawn.jobs.TryTakeOrderedJob(job3, JobTag.Misc);
                }, MenuOptionPriority.High), selPawn, parent);
                yield break;
            }
            if (maxAllowedToPickUp < parent.stackCount)
            {
                yield return new FloatMenuOption("CannotPickUpAll".Translate(parent.Label, parent) + ": " + "MaxPickUpAllowed".Translate(maxCapacity, parent.Label), null);
            }
            else if (MassUtility.WillBeOverEncumberedAfterPickingUp(selPawn, parent, parent.stackCount))
            {
                yield return new FloatMenuOption("CannotPickUpAll".Translate(parent.Label, parent) + ": " + "TooHeavy".Translate(), null);
            }
            else
            {
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUpAll".Translate(parent.Label, parent), delegate
                {
                    parent.SetForbidden(value: false, warnOnFail: false);
                    Job job2 = JobMaker.MakeJob(JobDefOf.TakeInventory, parent);
                    job2.count = parent.stackCount;
                    job2.checkEncumbrance = true;
                    job2.takeInventoryDelay = 120;
                    selPawn.jobs.TryTakeOrderedJob(job2, JobTag.Misc);
                }, MenuOptionPriority.High), selPawn, parent);
            }
            yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUpSome".Translate(parent.LabelNoCount, parent), delegate
            {
                int b = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(selPawn, parent), parent.stackCount);
                int to = Mathf.Min(maxAllowedToPickUp, b);
                Dialog_Slider window = new Dialog_Slider("PickUpCount".Translate(parent.LabelNoCount, parent), 1, to, delegate (int count)
                {
                    parent.SetForbidden(value: false, warnOnFail: false);
                    Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, parent);
                    job.count = count;
                    job.checkEncumbrance = true;
                    job.takeInventoryDelay = 120;
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                });
                Find.WindowStack.Add(window);
            }, MenuOptionPriority.High), selPawn, parent);
        }
        public IEnumerable<Gizmo> DeployGizmos()
        {
            yield return new Command_Action
            {
                icon = parent.def.uiIcon,
                defaultLabel = "CG_DeployItem".Translate(parent.def.label),
                action = () =>
                {
                    var pawn = Pawn;
                    var targetParams = new TargetingParameters
                    {
                        canTargetLocations = true,
                        canTargetPawns = false,
                        canTargetBuildings = false,
                        canTargetItems = false,
                        validator = (TargetInfo t) => t.Cell.InBounds(pawn.Map) && GenConstruct.CanPlaceBlueprintAt(Props.buildingDef, t.Cell, Rot4.North, pawn.Map).Accepted
                    };
                    Find.Targeter.BeginTargeting(targetParams, DeployAt);
                }
            };
        }

        private void DeployAt(LocalTargetInfo target)
        {
            var job = JobMaker.MakeJob(CrimsonGridFramework_DefOfs.CG_DeployItem, target.Cell, parent);
            job.count = 1;
            Pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        public bool HasCapacityForThisItem(Pawn pawn, int countToPick = 1)
        {
            var capacity = GetMaxCapacity();
            var used = 0f;
            foreach (var t in pawn.inventory.innerContainer)
            {
                if (t.def == this.parent.def)
                {
                    used += t.stackCount;
                }
            }
            return (used + countToPick) <= capacity;
        }

        private float GetMaxCapacity()
        {
            return parent.GetStatValue(CrimsonGridFramework_DefOfs.CG_DeployableCapacity);
        }
    }
}
