using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    public class WorkGiver_WorkUpgradeBuilding : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public JobDef JobDef => CrimsonGridFramework_DefOfs.CG_UpgradeBuilding;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return CompUpgradeableBuilding.buildingsWithUpgradeInProgress.Where(c => c.Map == pawn.Map && c.TryGetComp<CompUpgradeableBuilding>().StoredCostSatisfied);
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.Faction != pawn.Faction)
            {
                return null;
            }
            if (pawn.CanReach(t.Position, PathEndMode.Touch, Danger.Deadly) && pawn.Map.reservationManager.CanReserve(pawn, t))
            {
                return JobMaker.MakeJob(JobDef, t);
            }
            return null;
        }
    }
}
