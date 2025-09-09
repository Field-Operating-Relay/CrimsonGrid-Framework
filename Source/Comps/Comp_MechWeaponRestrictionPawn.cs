using System.Text;
using Verse;
using RimWorld;

namespace CrimsonGridFramework
{
    public class Comp_MechWeaponRestrictionPawn : ThingComp
    {
        private Pawn Mech => parent as Pawn;

        private MechWeightClassDef MechWeightClass => Mech?.def?.race?.mechWeightClass;

        private WeaponWeightClassExtension GetEquippedWeaponExtension()
        {
            if (Mech?.equipment?.Primary == null)
                return null;

            return Mech.equipment.Primary.def.GetModExtension<WeaponWeightClassExtension>();
        }

        private bool ShouldApplyDebuff()
        {
            if (Mech == null || MechWeightClass == null)
                return false;

            WeaponWeightClassExtension weaponExt = GetEquippedWeaponExtension();
            if (weaponExt?.targetMechWeightClass == null)
                return false;

            return MechWeightClassHelper.ShouldApplyDebuffs(MechWeightClass, weaponExt.targetMechWeightClass);
        }

        public override float GetStatOffset(StatDef stat)
        {
            if (!ShouldApplyDebuff())
                return 0f;

            WeaponWeightClassExtension weaponExt = GetEquippedWeaponExtension();
            if (weaponExt?.debuffStats == null)
                return 0f;

            foreach (var statModifier in weaponExt.debuffStats)
            {
                if (statModifier.stat == stat)
                {
                    return statModifier.value;
                }
            }

            return 0f;
        }

        public override void GetStatsExplanation(StatDef stat, StringBuilder sb, string whitespace = "")
        {
            if (!ShouldApplyDebuff())
                return;

            WeaponWeightClassExtension weaponExt = GetEquippedWeaponExtension();
            if (weaponExt?.debuffStats == null)
                return;

            foreach (var statModifier in weaponExt.debuffStats)
            {
                if (statModifier.stat == stat)
                {
                    sb.AppendLine($"{whitespace}{"CGF_HeavyWeaponPenalty".Translate(weaponExt.targetMechWeightClass?.label ?? "unknown")}: {statModifier.value.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset)}");
                    break;
                }
            }
        }

        public bool CanEquipWeapon(ThingDef weaponDef)
        {
            if (MechWeightClass == null)
                return true;

            var weaponExt = weaponDef.GetModExtension<WeaponWeightClassExtension>();
            if (weaponExt?.targetMechWeightClass == null)
                return true;

            return MechWeightClassHelper.CanEquipWeapon(MechWeightClass, weaponExt.targetMechWeightClass);
        }

        public string GetEquipFailureReason(ThingDef weaponDef)
        {
            if (MechWeightClass == null)
                return null;

            var weaponExt = weaponDef.GetModExtension<WeaponWeightClassExtension>();
            if (weaponExt?.targetMechWeightClass == null)
                return null;

            return MechWeightClassHelper.GetEquipFailureReason(MechWeightClass, weaponExt.targetMechWeightClass);
        }
    }
}