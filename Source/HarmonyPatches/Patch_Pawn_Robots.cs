using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn), "get_IsColonyMech")]
    public static class Patch_IsColonyMech_Robots
    {
        public static void Postfix(ref bool __result, Pawn __instance)
        {
            if (__instance.IsCrimsonGridRobot() && __instance.Faction == Faction.OfPlayer && __instance.MentalStateDef == null)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "get_IsColonyMechPlayerControlled")]
    public static class Patch_IsColonyMechPlayerControlled_Robots
    {
        public static void Postfix(ref bool __result, Pawn __instance)
        {
            if (__instance.IsCrimsonGridRobot() && __instance.Faction == Faction.OfPlayer && __instance.MentalStateDef == null)
            {
                __result = true;
            }
        }
    }
}
