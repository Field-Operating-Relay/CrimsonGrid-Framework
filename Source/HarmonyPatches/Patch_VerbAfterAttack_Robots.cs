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


            // Must have a pawn caster
            if (!(__instance.caster is Pawn casterPawn))
            {
                return;
            }

            // Must be a Crimson Grid robot
            if (!casterPawn.IsCrimsonGridRobot())
            {
                return;
            }

            // Must have the weapon restriction component
            var weaponRestrictionComp = casterPawn.TryGetComp<Comp_MechWeaponRestriction>();
            if (weaponRestrictionComp == null)
            {
                return;
            }

            // Must have an equipped weapon
            var equippedWeapon = casterPawn.equipment?.Primary;
            if (equippedWeapon == null)
            {
                return;
            }

            // Must have the weapon weight class extension
            var weaponExtension = equippedWeapon.def.GetModExtension<WeaponWeightClassExtension>();
            if (weaponExtension == null)
            {
                return;
            }

            // Check if debuffs should be applied based on weight class mismatch
            var mechWeightClass = casterPawn.def?.race?.mechWeightClass;
            if (mechWeightClass == null || weaponExtension.targetMechWeightClass == null)
            {
                return;
            }

            if (!MechWeightClassHelper.ShouldApplyDebuffs(mechWeightClass, weaponExtension.targetMechWeightClass))
            {
                return;
            }

            // Apply EMP stun if enabled
            if (weaponExtension.applyEmpStunOnFire)
            {
                ApplyEmpStun(casterPawn);
            }

            // Apply fire damage if enabled
            if (weaponExtension.applyFireDamageOnFire)
            {
                ApplyFireDamage(casterPawn, weaponExtension.fireDamageAmount);
            }
        }

        private static void ApplyEmpStun(Pawn pawn)
        {
            // Apply EMP damage which will stun mechs
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
            // Apply burn damage
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