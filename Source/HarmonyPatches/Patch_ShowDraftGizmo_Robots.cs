using HarmonyLib;
using RimWorld;
using Verse;

namespace CrimsonGridFramework.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn_DraftController), "ShowDraftGizmo", MethodType.Getter)]
    public class Patch_ShowDraftGizmo_Robots
    {
        public static void Postfix(Pawn ___pawn, ref bool __result)
        {
            // TODO: Add is connected to a provider check
            if (__result == false && ___pawn.IsCrimsonGridRobot() && ___pawn.Faction == Faction.OfPlayer)
            {
                __result = true;
            }
        }
    }
}
