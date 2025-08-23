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
            if (pawn.GetBandwidthComp() != null && pawn.Faction == Faction.OfPlayer && pawn.drafter == null)
            {
                pawn.drafter = new Pawn_DraftController(pawn);

            }

        }
    }
}
