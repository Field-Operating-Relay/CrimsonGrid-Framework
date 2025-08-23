using HarmonyLib;
using Verse;

namespace CrimsonGridFramework;

[HarmonyPatch(typeof(MechanitorUtility), "InMechanitorCommandRange")]
public static class MechanitorUtility_InMechanitorCommandRange_Patch
{
    public static void Postfix(Pawn mech, LocalTargetInfo target, ref bool __result)
    {
        if (__result is false && mech.IsMechanoidHacked())
        {
            __result = true;
        }
    }
}
