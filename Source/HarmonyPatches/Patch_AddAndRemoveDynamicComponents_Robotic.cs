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
        public static void Postfix(Pawn pawn)
        {
            if (pawn.IsCrimsonGridRobot() && pawn.Faction == Faction.OfPlayer && pawn.drafter == null)
            {
                pawn.drafter = new Pawn_DraftController(pawn);

                pawn.abilities = new Pawn_AbilityTracker(pawn);
            }

        }
    }
}
