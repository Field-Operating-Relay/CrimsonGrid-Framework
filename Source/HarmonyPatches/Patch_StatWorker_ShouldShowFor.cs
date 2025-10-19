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
    [HarmonyPatch(typeof(StatWorker), "ShouldShowFor")]
    public static class Patch_StatWorker_ShouldShowFor
    {
        public static bool Prefix(ref bool __result, StatRequest req, StatDef ___stat)
        {
            if(___stat.category == CrimsonGridFramework_DefOfs.CG_Ironhides)
            {
                if(req.Thing is Pawn pawn && pawn.IsCrimsonGridRobot())
                {
                    __result = true;
                    return false;
                }
            }
            return true;
        }
    }
}
