using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework.HarmonyPatches
{
    [HarmonyPatch(typeof(FloatMenuOptionProvider_DraftedMove), "PawnCanGoto")]
    public class Patch_PawnCanGoto_Robots
    {
        public static void Postfix(Pawn pawn, IntVec3 gotoLoc, ref AcceptanceReport __result)
        {
            if (__result.Accepted)
            {
                return;
            }
            // TODO: Add is connected to a provider check
            if (pawn.IsCrimsonGridRobot() && pawn.Faction == Faction.OfPlayer && pawn.CanReach(gotoLoc, PathEndMode.OnCell, Danger.Deadly))
            {
                __result = true;
            }
        }
    }
}
