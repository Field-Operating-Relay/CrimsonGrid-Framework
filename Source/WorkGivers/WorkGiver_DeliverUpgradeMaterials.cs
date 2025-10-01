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
    public class WorkGiver_DeliverUpgradeMaterials : WorkGiver_Scanner
    {
        private static HashSet<ThingDef> neededThingDefs = new HashSet<ThingDef>();
        public override PathEndMode PathEndMode => PathEndMode.Touch;
        public JobDef JobDef => CrimsonGridFramework_DefOfs.CG_DeliverUpgradeMaterials;
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return CompUpgradeableBuilding.buildingsWithUpgradeInProgress.Where(c => c.Map == pawn.Map && !c.TryGetComp<CompUpgradeableBuilding>().StoredCostSatisfied);
        }
        public ThingOwner<Thing> ThingOwner(Thing t)
        {
            return t.TryGetComp<CompUpgradeableBuilding>().upgradeContainer;
        }
        public IEnumerable<ThingDefCount> ThingDefs(Thing t)
        {
            CompUpgradeableBuilding comp = t.TryGetComp<CompUpgradeableBuilding>();
            if (!comp.isUpgrading || comp.TargetUpgrade.ingredients.NullOrEmpty())
            {
                yield break;
            }
            foreach (ThingDefCountClass thingDefCountClass in comp.TargetUpgrade.ingredients)
            {
                yield return new ThingDefCount(thingDefCountClass.thingDef, thingDefCountClass.count);
            }
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompUpgradeableBuilding comp = t.TryGetComp<CompUpgradeableBuilding>();
            if (comp == null)
            {
                return null;
            }
            if (pawn.Faction != t.Faction)
            {
                return null;
            }
            var defs = ThingDefs(t);
            if (defs != null && defs.Any() && pawn.CanReach(new LocalTargetInfo(t.Position), PathEndMode.Touch, Danger.Deadly))
            {
                Thing thing = FindThingToPack(t, pawn);
                if (thing != null && thing != pawn && thing != t)
                {
                    int countLeft = CountLeftToPack(t, pawn, new ThingDefCount(thing.def, comp.TargetUpgrade.ingredients.FirstOrDefault((ThingDefCountClass thingDefCountClass) => thingDefCountClass.thingDef == thing.def).count));
                    int jobCount = Mathf.Min(thing.stackCount, countLeft);
                    if (jobCount > 0)
                    {
                        Job job = JobMaker.MakeJob(JobDef, thing, t);
                        job.count = jobCount;
                        return job;
                    }
                }
            }
            return null;
        }

        public Thing FindThingToPack(Thing t, Pawn pawn)
        {
            CompUpgradeableBuilding comp = t.TryGetComp<CompUpgradeableBuilding>();
            Thing result = null;
            IEnumerable<ThingDefCountClass> thingDefs = comp.TargetUpgrade.ingredients;
            if (thingDefs != null && thingDefs.Any())
            {
                foreach (ThingDefCountClass item in thingDefs)
                {
                    ThingDefCount thingDefCount = item;
                    int countLeftToTransfer = CountLeftToPack(t, pawn, thingDefCount);
                    if (countLeftToTransfer > 0)
                    {
                        neededThingDefs.Add(thingDefCount.ThingDef);
                    }
                }
                if (!neededThingDefs.Any())
                {
                    return null;
                }
                result = ClosestHaulable(pawn, ThingRequestGroup.Pawn, ValidThingDef);
                if (result == null)
                {
                    result = ClosestHaulable(pawn, ThingRequestGroup.HaulableEver, ValidThingDef);
                }
                neededThingDefs.Clear();
            }
            return result;
            bool ValidThingDef(Thing thing)
            {
                if (neededThingDefs.Contains(thing.def) && pawn.CanReserve(thing))
                {
                    return !thing.IsForbidden(pawn.Faction);
                }
                return false;
            }
        }

        protected Thing ClosestHaulable(Pawn pawn, ThingRequestGroup thingRequestGroup, Predicate<Thing> validator = null)
        {
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(thingRequestGroup), PathEndMode.Touch, TraverseParms.For(pawn), 9999f, validator);
        }

        private int CountLeftToPack(Thing t, Pawn pawn, ThingDefCount thingDefCount)
        {
            if (thingDefCount.Count <= 0 || thingDefCount.ThingDef == null)
            {
                return 0;
            }
            return Mathf.Max(thingDefCount.Count - TransferableCountHauledByOthersForPacking(t, pawn, thingDefCount), 0);
        }
        private int TransferableCountHauledByOthersForPacking(Thing container, Pawn pawn, ThingDefCount thingDefCount)
        {
            int mechCount = 0;
            if (ModsConfig.BiotechActive)
            {
                mechCount = HauledByOthers(pawn, thingDefCount, container.Map.mapPawns.SpawnedColonyMechs);
            }
            int slaveCount = 0;
            if (ModsConfig.IdeologyActive)
            {
                slaveCount = HauledByOthers(pawn, thingDefCount, container.Map.mapPawns.SlavesOfColonySpawned);
            }
            return mechCount + slaveCount + HauledByOthers(pawn, thingDefCount, container.Map.mapPawns.FreeColonistsSpawned);
        }

        private int HauledByOthers(Pawn pawn, ThingDefCount thingDefCount, List<Pawn> pawns)
        {
            int count = 0;
            foreach (Pawn target in pawns)
            {
                count += CountFromJob(pawn, target, thingDefCount, pawns);
            }
            return count;
        }

        protected virtual int CountFromJob(Pawn pawn, Pawn target, ThingDefCount thingDefCount, List<Pawn> pawns)
        {
            if (target != pawn && target.CurJob != null && (target.CurJob.def == JobDef || target.CurJob.def == CrimsonGridFramework_DefOfs.CG_DeliverUpgradeMaterials) && target.jobs.curDriver is JobDriver_DeliverUpgradeMaterials { Item: { } toHaul } && thingDefCount.ThingDef == toHaul.def)
            {
                return toHaul.stackCount;
            }
            return 0;
        }
    }
}

