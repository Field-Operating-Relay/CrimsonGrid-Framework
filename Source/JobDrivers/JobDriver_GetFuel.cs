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
    public class JobDriver_GetFuel : JobDriver
    {
        CompFuelStation fuelStationComp => job.targetA.Thing.TryGetComp<CompFuelStation>();
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => !fuelStationComp.HasFuel);
            this.FailOnDestroyedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil getFuel = ToilMaker.MakeToil("GetFuel");
            getFuel.endConditions.Add(delegate
            {
                if (!fuelStationComp.HasFuel)
                {
                    return JobCondition.Incompletable;
                }
                if (pawn.FullFuel())
                {
                    return JobCondition.Succeeded;
                }
                return JobCondition.Ongoing;
            });
            getFuel.defaultCompleteMode = ToilCompleteMode.Never;
            getFuel.tickAction = delegate
            {
                pawn.AddFuel(fuelStationComp.Props.fuelTransferRate/60f, fuelStationComp);
            };
            yield return getFuel;
        }
    }
}
