using HarmonyLib;
using Verse;

namespace CrimsonGridFramework;

[HarmonyPatch(typeof(Pawn), "IsColonyMech", MethodType.Getter)]
public static class Pawn_IsColonyMech_Patch
{
    public static void Postfix(ref bool __result, Pawn __instance)
    {
        if (__result is false && __instance.Spawned && __instance.IsMechanoidHacked())
        {
            __result = true;
        }
    }
}
