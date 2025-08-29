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
    [HarmonyPatch(typeof(PawnComponentsUtility), "AddAndRemoveDynamicComponents")]
    public static class Patch_AddAndRemoveDynamicComponents_Robotic
    {
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (pawn.IsCrimsonGridRobot() && pawn.Faction == Faction.OfPlayer && pawn.drafter == null)
            {
                pawn.drafter = new Pawn_DraftController(pawn);
            }

        }
    }
    public static class FloatMenuMakerMap_CanTakeOrder_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (__result is false && pawn.IsCrimsonGridRobot())
            {
                __result = true;

            }
        }
    }
}
