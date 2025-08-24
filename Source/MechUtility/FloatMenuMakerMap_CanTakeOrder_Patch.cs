/*using HarmonyLib;
using RimWorld;
using Verse;

namespace CrimsonGridFramework;

[HarmonyPatch(typeof(FloatMenuMakerMap), "CanTakeOrder")]
public static class FloatMenuMakerMap_CanTakeOrder_Patch
{
    [HarmonyPriority(int.MinValue)]
    public static void Postfix(Pawn pawn, ref bool __result)
    {
        if (__result is false && pawn.IsMechanoidHacked())
        {
            __result = true;
        }
    }
}
*/