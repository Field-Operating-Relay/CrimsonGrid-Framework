using HarmonyLib;
using RimWorld;
using Verse;

namespace CrimsonGridFramework;

[HarmonyPatch(typeof(PawnComponentsUtility), "AddAndRemoveDynamicComponents")]
public static class PawnComponentsUtility_AddAndRemoveDynamicComponents
{
    public static void Postfix(Pawn pawn)
    {
        if (pawn.IsMechanoidHacked())
        {
            pawn.drafter ??= new Pawn_DraftController(pawn);
        }
    }
}
