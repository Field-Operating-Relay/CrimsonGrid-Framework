using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace CrimsonGridFramework
{
    [HarmonyPatch(typeof(Pawn_DraftController), "GetGizmos")]
    public static class Patch_Pawn_DraftController_GetGizmos
    {
        static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn_DraftController __instance)
        {
            foreach (var gizmo in __result)
            {
                yield return gizmo;
            }

            if (__instance.pawn.inventory == null) yield break;
            foreach (var item in __instance.pawn.inventory.innerContainer)
            {
                var comp = item.TryGetComp<CompDeployableItem>();
                if (comp != null)
                {
                    foreach (var gizmo in comp.DeployGizmos())
                    {
                        yield return gizmo;
                    }
                }
            }
        }
    }
}
