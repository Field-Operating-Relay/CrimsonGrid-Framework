using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    public class ThinkNode_ConditionalFuelAvailable : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn.Map.listerBuildings.allBuildingsColonist.Any(b => b.TryGetComp<CompFuelStation>() is CompFuelStation comp && comp.HasFuel);
            
        }
    }
}