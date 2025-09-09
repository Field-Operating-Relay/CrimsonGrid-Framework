using HarmonyLib;
using RimWorld;
using Verse;

namespace CrimsonGridFramework.HarmonyPatches
{
    [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
    public static class Patch_VerbAfterAttack_Robots
    {
        public static void Postfix(Verb __instance)
        {
            // Only apply debuffs when the burst is complete
            if (__instance.Bursting)
            {
                return;
            }


            if (!(__instance.caster is Pawn casterPawn))
            {
                return;
            }

            if (!casterPawn.IsCrimsonGridRobot())
            {
                return;
            }

            var weaponRestrictionComp = casterPawn.TryGetComp<Comp_MechWeaponRestrictionPawn>();
            if (weaponRestrictionComp == null)
            {
                return;
            }

            var equippedWeapon = casterPawn.equipment?.Primary;
            if (equippedWeapon == null)
            {
                return;
            }

            var weaponExtension = equippedWeapon.def.GetModExtension<WeaponWeightClassExtension>();
            if (weaponExtension == null)
            {
                return;
            }

            var mechWeightClass = casterPawn.def?.race?.mechWeightClass;
            if (mechWeightClass == null || weaponExtension.targetMechWeightClass == null)
            {
                return;
            }

            if (!MechWeightClassHelper.ShouldApplyDebuffs(mechWeightClass, weaponExtension.targetMechWeightClass))
            {
                return;
            }

            if (weaponExtension.applyEmpStunOnFire)
            {
                ApplyEmpStun(casterPawn);
            }

            if (weaponExtension.applyFireDamageOnFire)
            {
                ApplyFireDamage(casterPawn, weaponExtension.fireDamageAmount);
            }
        }

        private static void ApplyEmpStun(Pawn pawn)
        {
            var empDamage = new DamageInfo(
                DamageDefOf.EMP,
                5f, // Small amount of EMP damage
                0f, // No armor penetration needed
                -1f, // No angle
                pawn, // Self-inflicted
                null // No body part
            );

            pawn.TakeDamage(empDamage);
        }

        private static void ApplyFireDamage(Pawn pawn, float damageAmount)
        {
            var fireDamage = new DamageInfo(
                CrimsonGridFramework_DefOfs.CG_Burn,
                Rand.Range(damageAmount * 0.8f, damageAmount * 1.2f), // Add some randomness
                0f, // No armor penetration
                -1f, // No angle
                pawn, // Self-inflicted
                null // No specific body part
            );

            pawn.TakeDamage(fireDamage);
        }
    }
}