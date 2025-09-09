using RimWorld;
using System.Collections.Generic;
using Verse;

namespace CrimsonGridFramework
{
    public enum WeightClassComparison
    {
        Lighter = -1,  // First is lighter than second
        Equal = 0,     // Same weight class
        Heavier = 1    // First is heavier than second
    }

    public static class MechWeightClassHelper
    {
        private static readonly Dictionary<string, int> weightClassOrder = new Dictionary<string, int>
        {
            { "Light", 0 },
            { "Medium", 1 },
            { "Heavy", 2 },
            { "UltraHeavy", 3 }
        };

        /// <summary>
        /// Compares two MechWeightClassDef objects to determine their relative weight
        /// </summary>
        /// <param name="first">First weight class to compare</param>
        /// <param name="second">Second weight class to compare</param>
        /// <returns>WeightClassComparison indicating relationship</returns>
        public static WeightClassComparison CompareWeightClasses(MechWeightClassDef first, MechWeightClassDef second)
        {
            if (first == null || second == null)
                return WeightClassComparison.Equal;

            int firstOrder = GetWeightClassOrder(first);
            int secondOrder = GetWeightClassOrder(second);

            if (firstOrder < secondOrder)
                return WeightClassComparison.Lighter;
            else if (firstOrder > secondOrder)
                return WeightClassComparison.Heavier;
            else
                return WeightClassComparison.Equal;
        }

        /// <summary>
        /// Gets the numerical difference between two weight classes
        /// </summary>
        /// <param name="first">First weight class</param>
        /// <param name="second">Second weight class</param>
        /// <returns>Absolute difference in weight class levels</returns>
        public static int GetWeightClassDifference(MechWeightClassDef first, MechWeightClassDef second)
        {
            if (first == null || second == null)
                return 0;

            int firstOrder = GetWeightClassOrder(first);
            int secondOrder = GetWeightClassOrder(second);

            return System.Math.Abs(firstOrder - secondOrder);
        }

        /// <summary>
        /// Checks if a mech can equip a weapon based on weight class restrictions
        /// </summary>
        /// <param name="mechWeightClass">Mech's weight class</param>
        /// <param name="weaponWeightClass">Weapon's weight class</param>
        /// <returns>True if weapon can be equipped</returns>
        public static bool CanEquipWeapon(MechWeightClassDef mechWeightClass, MechWeightClassDef weaponWeightClass)
        {
            if (mechWeightClass == null || weaponWeightClass == null)
                return true;

            WeightClassComparison comparison = CompareWeightClasses(mechWeightClass, weaponWeightClass);
            int difference = GetWeightClassDifference(mechWeightClass, weaponWeightClass);

            // Can equip if weapon is same weight class or exactly 1 heavier
            return comparison == WeightClassComparison.Equal ||
                   (comparison == WeightClassComparison.Lighter && difference == 1);
        }

        /// <summary>
        /// Checks if equipping a weapon would result in stat debuffs
        /// </summary>
        /// <param name="mechWeightClass">Mech's weight class</param>
        /// <param name="weaponWeightClass">Weapon's weight class</param>
        /// <returns>True if stat debuffs should be applied</returns>
        public static bool ShouldApplyDebuffs(MechWeightClassDef mechWeightClass, MechWeightClassDef weaponWeightClass)
        {
            if (mechWeightClass == null || weaponWeightClass == null)
                return false;

            WeightClassComparison comparison = CompareWeightClasses(mechWeightClass, weaponWeightClass);
            int difference = GetWeightClassDifference(mechWeightClass, weaponWeightClass);

            // Apply debuffs if weapon is exactly 1 weight class heavier
            return comparison == WeightClassComparison.Lighter && difference == 1;
        }

        /// <summary>
        /// Gets the numerical order of a weight class for comparison purposes
        /// </summary>
        /// <param name="weightClass">Weight class to get order for</param>
        /// <returns>Numerical order (0 = lightest, higher = heavier)</returns>
        private static int GetWeightClassOrder(MechWeightClassDef weightClass)
        {
            if (weightClass == null)
                return 0;

            if (weightClassOrder.TryGetValue(weightClass.defName, out int order))
                return order;

            // If we don't recognize the weight class, try to infer from its defName
            string defName = weightClass.defName.ToLower();
            if (defName.Contains("light"))
                return 0;
            else if (defName.Contains("medium"))
                return 1;
            else if (defName.Contains("heavy") && defName.Contains("ultra"))
                return 3;
            else if (defName.Contains("heavy"))
                return 2;

            // Default to medium if we can't determine
            return 1;
        }

        /// <summary>
        /// Gets a human-readable description of why a weapon cannot be equipped
        /// </summary>
        /// <param name="mechWeightClass">Mech's weight class</param>
        /// <param name="weaponWeightClass">Weapon's weight class</param>
        /// <returns>Localized failure reason or null if weapon can be equipped</returns>
        public static string GetEquipFailureReason(MechWeightClassDef mechWeightClass, MechWeightClassDef weaponWeightClass)
        {
            if (CanEquipWeapon(mechWeightClass, weaponWeightClass))
                return null;

            if (mechWeightClass == null || weaponWeightClass == null)
                return null;

            WeightClassComparison comparison = CompareWeightClasses(mechWeightClass, weaponWeightClass);
            int difference = GetWeightClassDifference(mechWeightClass, weaponWeightClass);

            if (comparison == WeightClassComparison.Heavier)
            {
                return "CGF_WeaponTooLight".Translate(weaponWeightClass.label, mechWeightClass.label);
            }
            else if (comparison == WeightClassComparison.Lighter && difference >= 2)
            {
                return "CGF_WeaponTooHeavy".Translate(weaponWeightClass.label, mechWeightClass.label);
            }

            return "CGF_WeaponIncompatible".Translate();
        }

        /// <summary>
        /// Gets the mech weight class from a pawn's race definition
        /// </summary>
        /// <param name="pawn">The pawn to check</param>
        /// <returns>The mech's weight class or null if not defined</returns>
        public static MechWeightClassDef GetMechWeightClass(Pawn pawn)
        {
            return pawn?.def?.race?.mechWeightClass;
        }

        /// <summary>
        /// Gets the weapon weight class from a weapon's mod extension
        /// </summary>
        /// <param name="weapon">The weapon to check</param>
        /// <returns>The weapon's target weight class or null if not defined</returns>
        public static MechWeightClassDef GetWeaponWeightClass(Thing weapon)
        {
            if (weapon == null)
                return null;

            var extension = weapon.def.GetModExtension<WeaponWeightClassExtension>();
            return extension?.targetMechWeightClass;
        }

        /// <summary>
        /// Gets the weapon weight class from a weapon def's mod extension
        /// </summary>
        /// <param name="weaponDef">The weapon def to check</param>
        /// <returns>The weapon's target weight class or null if not defined</returns>
        public static MechWeightClassDef GetWeaponWeightClass(ThingDef weaponDef)
        {
            if (weaponDef == null)
                return null;

            var extension = weaponDef.GetModExtension<WeaponWeightClassExtension>();
            return extension?.targetMechWeightClass;
        }
    }
}