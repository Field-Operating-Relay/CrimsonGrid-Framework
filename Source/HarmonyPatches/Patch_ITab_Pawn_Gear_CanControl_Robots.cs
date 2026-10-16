using HarmonyLib;
using RimWorld;
using Verse;

namespace CrimsonGridFramework.HarmonyPatches
{
    [HarmonyPatch(typeof(ITab_Pawn_Gear), "CanControl", MethodType.Getter)]
    public static class Patch_ITab_Pawn_Gear_CanControl_Robots
    {
        public static void Postfix(ITab_Pawn_Gear __instance, ref bool __result)
        {
            if (__result)
                return;

            if (Patch_ITab_Pawn_Gear_SelPawnForGear_Cache.cachedPawn != null && Patch_ITab_Pawn_Gear_SelPawnForGear_Cache.cachedPawn.IsCrimsonGridRobot() &&
                Patch_ITab_Pawn_Gear_SelPawnForGear_Cache.cachedPawn.Faction == Faction.OfPlayer &&
                Patch_ITab_Pawn_Gear_SelPawnForGear_Cache.cachedPawn.MentalStateDef == null)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(ITab_Pawn_Gear), "CanControlColonist", MethodType.Getter)]
    public static class Patch_ITab_Pawn_Gear_CanControlColonist_Robots
    {
        public static void Postfix(ITab_Pawn_Gear __instance, ref bool __result)
        {
            if (__result)
                return;

            if (Patch_ITab_Pawn_Gear_SelPawnForGear_Cache.cachedPawn != null && Patch_ITab_Pawn_Gear_SelPawnForGear_Cache.cachedPawn.IsCrimsonGridRobot() &&
                Patch_ITab_Pawn_Gear_SelPawnForGear_Cache.cachedPawn.Faction == Faction.OfPlayer &&
                Patch_ITab_Pawn_Gear_SelPawnForGear_Cache.cachedPawn.MentalStateDef == null)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(ITab_Pawn_Gear), "SelPawnForGear", MethodType.Getter)]
    public static class Patch_ITab_Pawn_Gear_SelPawnForGear_Cache
    {
        public static Pawn cachedPawn;
        public static void Postfix(Pawn __result)
        {
            cachedPawn = __result;
        }
    }
}