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
    [HarmonyPatch(typeof(Pawn), "get_IsColonistPlayerControlled")]
    public static class Patch_IsColonistPlayerControlled
    {
        public static void Postfix(ref bool __result, Pawn __instance)
        {
            if (__instance.IsCrimsonGridRobot() && __instance.IsConnected() && __instance.HostFaction == null && __instance.Faction == Faction.OfPlayer && __instance.MentalStateDef == null)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "get_IsColonyMechPlayerControlled")]
    public static class Patch_IsColonyMechPlayerControlled
    {
        public static void Postfix(ref bool __result, Pawn __instance)
        {
            if (__instance.IsCrimsonGridRobot() && __instance.Faction == Faction.OfPlayer && __instance.MentalStateDef == null)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "CurrentlyUsableForBills")]
    public static class Patch_CurrentlyUsableForBills
    {
        public static void Postfix(ref bool __result, Pawn __instance)
        {
            if (__instance.IsCrimsonGridRobot())
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "get_CanTakeOrder")]
    public static class Patch_Pawn_CanTakeOrder
    {
        public static void Postfix(Pawn __instance, ref bool __result)
        {
            if (__result is false && __instance.IsCrimsonGridRobot() && __instance.IsConnected())
            {
                __result = true;

            }
        }
    }
}
