using HarmonyLib;
using RimWorld;
using Verse;

namespace CrimsonGridFramework.HarmonyPatches
{
    [HarmonyPatch(typeof(EquipmentUtility), "CanEquip")]
    [HarmonyPatch([typeof(Thing), typeof(Pawn), typeof(string), typeof(bool)], [ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal])] 
     public static class Patch_EquipmentUtility_CanEquip
    {
        public static void Postfix(Thing thing, Pawn pawn, ref string cantReason, bool checkBonded, ref bool __result)
        {
            if (!__result)
            {
                return;
            }

            var weaponExt = thing.def.GetModExtension<WeaponWeightClassExtension>();
            if (weaponExt?.targetMechWeightClass == null)
                return;

            if (pawn == null)
                return;

            if (!pawn.IsCrimsonGridRobot())
            {
                if (weaponExt.crimsonRobotsOnly)
                {
                    cantReason = "CGF_MechOnlyWeapon".Translate();
                    __result = false;
                }
                return;
            }

            var mechWeightClass = MechWeightClassHelper.GetMechWeightClass(pawn);
            if (mechWeightClass == null)
                return;

            if (!MechWeightClassHelper.CanEquipWeapon(mechWeightClass, weaponExt.targetMechWeightClass))
            {
                cantReason = MechWeightClassHelper.GetEquipFailureReason(mechWeightClass, weaponExt.targetMechWeightClass);
                __result = false;
            }
        }
    }
}