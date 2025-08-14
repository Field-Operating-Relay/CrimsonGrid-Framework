using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace CrimsonGridFramework;

[HarmonyPatch(typeof(MechanitorUtility), "GetMechGizmos")]
public static class MechanitorUtility_GetMechGizmos_Patch
{
    public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn mech)
    {
        if (mech.IsMechanoidHacked())
        {
            yield break;
        }

        foreach (Gizmo g in __result)
        {
            yield return g;
        }
    }
}
