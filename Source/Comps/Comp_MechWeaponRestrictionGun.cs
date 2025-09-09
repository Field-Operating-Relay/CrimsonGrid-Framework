using System.Text;
using Verse;
using RimWorld;

namespace CrimsonGridFramework
{
    public class Comp_MechWeaponRestrictionGun : ThingComp
    {
        private WeaponWeightClassExtension GetEquippedWeaponExtension()
        {
            return parent.def.GetModExtension<WeaponWeightClassExtension>();
        }

        private bool ShouldApplyDebuff(Pawn mech, MechWeightClassDef def)
        {
            if (mech == null || def == null)
                return false;

            WeaponWeightClassExtension weaponExt = GetEquippedWeaponExtension();
            if (weaponExt?.targetMechWeightClass == null)
                return false;

            return MechWeightClassHelper.ShouldApplyDebuffs(def, weaponExt.targetMechWeightClass);
        }
        public override float GetStatOffset(StatDef stat)
        {
            if (parent.ParentHolder is not Pawn_EquipmentTracker tracker || !tracker.pawn.IsCrimsonGridRobot())
            {
                return 0f;
            }

            if (!ShouldApplyDebuff(tracker.pawn, tracker.pawn.def.race.mechWeightClass))
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
            if (parent.ParentHolder is not Pawn_EquipmentTracker tracker || !tracker.pawn.IsCrimsonGridRobot())
            {
                return;
            }

            if (!ShouldApplyDebuff(tracker.pawn, tracker.pawn.def.race.mechWeightClass))
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
    }
}
