using HarmonyLib;
using Verse;

namespace CrimsonGridFramework;

[HarmonyPatch(typeof(MechanitorUtility), "EverControllable")]
public static class MechanitorUtility_EverControllable_Patch
{
    public static void Postfix(Pawn mech, ref bool __result)
    {
        if (__result is false && mech.IsMechanoidHacked())
        {
            __result = true;
        }
    }
}
