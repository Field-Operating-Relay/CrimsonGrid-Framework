/*using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework.HarmonyPatches
{
    [HarmonyPatch]
    public static class Patch_Pawn_GetGizmos_AllowCrimsonGridRobots
    {
        public static bool IsConnectedRobot(Pawn pawn)
        {
            return pawn.IsCrimsonGridRobot() && pawn.IsConnected();
        }
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(AccessTools.Inner(typeof(Pawn), "<GetGizmos>d__344"), "MoveNext");
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var methodToFind = AccessTools.Method(typeof(Pawn), "get_IsColonyMech");
            bool found = false;
            bool line = false;
            CodeInstruction goInstr = null;
            foreach (CodeInstruction instruction in instructions)
            {
                if (line)
                {
                    line = false;
                    yield return new(OpCodes.Ldloc_2);
                    yield return new(OpCodes.Call, AccessTools.Method(typeof(Patch_Pawn_GetGizmos_AllowCrimsonGridRobots), nameof(IsConnectedRobot)));
                    yield return goInstr;
                }
                if (found)
                {
                    found = false;
                    line = true;
                    goInstr = instruction;
                }
                if (instruction.Calls(methodToFind))
                {
                    found = true;
                }
                yield return instruction;
            }
        }

    }
}*/