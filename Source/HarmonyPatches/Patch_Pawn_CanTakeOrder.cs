using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn), "get_CanTakeOrder")]
    public static class Patch_Pawn_CanTakeOrder
    {
        public static void Postfix(Pawn __instance, ref bool __result)
        {
            if (__result is false && __instance.IsCrimsonGridRobot() && __instance.IsConnected())
            {
                __result = true;

            }
        }
    }
}
