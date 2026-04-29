using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    [HotSwappable]
    public class JobDriver_DeployItem : JobDriver
    {
        private const TargetIndex CellInd = TargetIndex.A;
        private const TargetIndex ItemInd = TargetIndex.B;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(CellInd), job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_General.Do(delegate
            {
                var item = job.GetTarget(ItemInd).Thing;
                var splitItem = item.SplitOff(1);
                job.SetTarget(ItemInd, splitItem);
                GenSpawn.Spawn(splitItem, pawn.Position, pawn.Map);
            });

            yield return Toils_Haul.StartCarryThing(ItemInd);
            yield return Toils_Haul.CarryHauledThingToCell(CellInd);

            var comp = job.GetTarget(ItemInd).Thing.TryGetComp<CompDeployableItem>();
            var duration = comp.Props.installDuration;
            yield return Toils_General.Wait(duration, CellInd).WithProgressBarToilDelay(ItemInd);

            yield return Toils_General.Do(delegate
            {
                var item = job.GetTarget(ItemInd).Thing;
                var deployComp = item.TryGetComp<CompDeployableItem>();
                var building = GenSpawn.Spawn(deployComp.Props.buildingDef, job.GetTarget(CellInd).Cell, pawn.Map);
                building.SetFaction(pawn.Faction);
                item.Destroy();
            });
        }
    }
}
