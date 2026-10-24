using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CrimsonGridFramework
{
    public class Mod: Verse.Mod
    {
        public Mod(ModContentPack content)
        : base(content)
        {
            LongEventHandler.QueueLongEvent(Init, "CrimsonGridFramework.Init", doAsynchronously: true, null);
        }
        public void Init()
        {
            GetSettings<ModSettings>();
            PatchWeapons();
        }

        public override string SettingsCategory()
        {
            return "Crimson Grid Framework";
        }

        public override void DoSettingsWindowContents(Rect rect)
        {
            ModSettingsWindow.Draw(rect);
            base.DoSettingsWindowContents(rect);
        }

        private static MechWeightClassDef DetermineWeightClassBasedOnMass(float mass)
        {
            if (mass <= ModSettings.lightWeaponsMassThreshold)
            {
                return MechWeightClassDefOf.Light;
            }
            else if (mass <= ModSettings.mediumWeaponsMassThreshold)
            {
                return MechWeightClassDefOf.Medium;
            }
            else if (mass <= ModSettings.heavyWeaponsMassThreshold)
            {
                return MechWeightClassDefOf.Heavy;
            }

            return MechWeightClassDefOf.UltraHeavy;
        }

        private static List<WeaponStatModifier> CreateDefaultDebuffStats()
        {
            WeaponStatModifier moveSpeed = new WeaponStatModifier
            {
                stat = StatDefOf.MoveSpeed,
                factor = 0.8f
            };

            WeaponStatModifier shootingAccuracy = new WeaponStatModifier
            {
                stat = StatDefOf.ShootingAccuracyPawn,
                offset = -0.3f
            };

            WeaponStatModifier aimingDelay = new WeaponStatModifier
            {
                stat = StatDefOf.AimingDelayFactor,
                offset = -0.3f
            };

            return new List<WeaponStatModifier> { moveSpeed, shootingAccuracy, aimingDelay };
        }

        private static void PatchWeapons()
        {
            List<ThingDef> weapons = DefDatabase<ThingDef>.AllDefs.Where(thingDef => thingDef.IsWeapon && thingDef.HasComp(typeof(CompQuality))).ToList();
            foreach (ThingDef weapon in weapons)
            {
                CompProperties_MechWeaponRestrictionGun props = new CompProperties_MechWeaponRestrictionGun();
                WeaponWeightClassExtension ext = new WeaponWeightClassExtension();

                ext.targetMechWeightClass = DetermineWeightClassBasedOnMass(weapon.BaseMass);
                ext.debuffStats = CreateDefaultDebuffStats();

                if (!weapon.HasModExtension<WeaponWeightClassExtension>())
                {
                    if (weapon.modExtensions == null)
                    {
                        weapon.modExtensions = new List<DefModExtension>();
                    }
                    weapon.modExtensions.Add(ext);
                }

                if (!weapon.HasComp<Comp_MechWeaponRestrictionGun>())
                {
                    weapon.comps.Add(props);
                }
            }
        }
    }
}
