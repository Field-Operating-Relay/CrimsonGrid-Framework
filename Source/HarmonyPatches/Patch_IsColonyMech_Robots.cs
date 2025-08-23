using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn), "get_IsColonyMech")]
    public static class Patch_IsColonyMech_Robots
    {
        public static void Postfix(ref bool __result, Pawn __instance)
        {
            if (__instance.GetBandwidthComp() != null && __instance.Faction == Faction.OfPlayer && __instance.MentalStateDef == null)
            {
                __result = true;
            }
        }
    }
}
