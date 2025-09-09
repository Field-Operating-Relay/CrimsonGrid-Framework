using HarmonyLib;
using RimWorld;
using Verse;

namespace CrimsonGridFramework.HarmonyPatches
{
    [HarmonyPatch(typeof(MechanitorUtility), "CanDraftMech")]
    public class Patch_CanDraftMech_Robots
    {
        public static void Postfix(Pawn mech, ref AcceptanceReport __result)
        {
            // TODO: Add is connected to a provider check
            if (!__result.Accepted && mech.IsCrimsonGridRobot() && mech.Faction == Faction.OfPlayer)
            {
                __result = AcceptanceReport.WasAccepted;
            }
        }
    }
}
