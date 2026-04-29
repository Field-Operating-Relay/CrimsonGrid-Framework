using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace CrimsonGridFramework
{
    [HarmonyPatch(typeof(MassUtility), nameof(MassUtility.WillBeOverEncumberedAfterPickingUp))]
    public static class Patch_MassUtility_WillBeOverEncumberedAfterPickingUp
    {
        static bool Prefix(Pawn pawn, Thing thing, int count, ref bool __result)
        {
            var comp = thing.TryGetComp<CompDeployableItem>();
            if (comp != null && !comp.HasCapacityForThisItem(pawn, count))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MassUtility), nameof(MassUtility.CountToPickUpUntilOverEncumbered))]
    public static class Patch_MassUtility_CountToPickUpUntilOverEncumbered
    {
        static void Postfix(Pawn pawn, Thing thing, ref int __result)
        {
            var comp = thing.TryGetComp<CompDeployableItem>();
            if (comp != null)
            {
                var capacity = comp.parent.GetStatValue(CrimsonGridFramework_DefOfs.CG_DeployableCapacity);
                var used = 0f;
                foreach (var t in pawn.inventory.innerContainer)
                {
                    if (t.def == thing.def) used += t.stackCount;
                }
                var remaining = Mathf.Max(0, capacity - used);
                __result = Mathf.Min(__result, (int)remaining);
            }
        }
    }
}
