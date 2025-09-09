using HarmonyLib;
using RimWorld;
using Verse;

namespace CrimsonGridFramework.HarmonyPatches
{
    [HarmonyPatch(typeof(FloatMenuOptionProvider_Equip), "GetSingleOptionFor")]
    public class Patch_FloatMenuOptionProvider_Equip_Robots
    {
        public static void Postfix(Thing clickedThing, FloatMenuContext context, ref FloatMenuOption __result)
        {
            if (__result == null || context.FirstSelectedPawn == null || !context.FirstSelectedPawn.IsCrimsonGridRobot())
                return;

            if (__result.action == null)
                return;

            var weaponExt = clickedThing.def.GetModExtension<WeaponWeightClassExtension>();
            if (weaponExt?.targetMechWeightClass == null)
                return;

            var mechWeightClass = MechWeightClassHelper.GetMechWeightClass(context.FirstSelectedPawn);
            if (mechWeightClass == null)
                return;

            if (!MechWeightClassHelper.CanEquipWeapon(mechWeightClass, weaponExt.targetMechWeightClass))
            {
                string failureReason = MechWeightClassHelper.GetEquipFailureReason(mechWeightClass, weaponExt.targetMechWeightClass);
                if (!string.IsNullOrEmpty(failureReason))
                {
                    __result = new FloatMenuOption("CannotEquip".Translate(clickedThing.LabelShort) + ": " + failureReason, null);
                }
            }
        }
    }
}