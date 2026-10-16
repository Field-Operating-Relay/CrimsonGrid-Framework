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
    public class FloatMenuOptionProvider_RefuelRobot : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;

        protected override bool Undrafted => true;

        protected override bool Multiselect => false;

        protected override bool RequiresManipulation => true;
        protected override bool MechanoidCanDo => true;

        public override bool TargetPawnValid(Pawn pawn, FloatMenuContext context)
        {
            return pawn.IsCrimsonGridRobot();
        }
        protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
        {
            Thing fuel = GenClosest.ClosestThingReachable(clickedPawn.Position, clickedPawn.Map, ThingRequest.ForDef(ThingDefOf.Chemfuel), PathEndMode.Touch, TraverseParms.For(clickedPawn), 9999f, (Thing t) => !t.IsForbidden(clickedPawn) && clickedPawn.CanReserve(t), null, 0, -1, false);
            bool anyFuel = fuel != null;
            if (!anyFuel)
            {
                return new FloatMenuOption("CGF_NoFuelAvailable".Translate(), null);
            }
            if (clickedPawn.FullFuel())
            {
                return new FloatMenuOption("CGF_FullyFueled".Translate(), null);
            }
            return new FloatMenuOption("CGF_RefuelRobot".Translate(), delegate
            {
                Job job = JobMaker.MakeJob(CrimsonGridFramework_DefOfs.CGF_RefuelRobot, fuel, clickedPawn);
                job.count = Mathf.Min(fuel.stackCount, clickedPawn.NeededFuelAmount());
                context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            });
        }

    }
}
