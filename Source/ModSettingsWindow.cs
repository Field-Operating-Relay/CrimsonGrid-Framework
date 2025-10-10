using UnityEngine;
using Verse;

namespace CrimsonGridFramework
{
    public class ModSettingsWindow
    {
        public static void Draw(Rect parent)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(parent);

            // Title
            Text.Font = GameFont.Medium;
            listing.Label("CGF_Settings_Title".Translate());
            Text.Font = GameFont.Small;
            listing.Gap();

            // Description
            listing.Label("CGF_Settings_Description".Translate());
            listing.Gap();

            // Light Weapons Threshold
            listing.Label("CGF_Settings_LightThreshold".Translate(ModSettings.lightWeaponsMassThreshold.ToString("F1")));
            float newLight = listing.Slider(ModSettings.lightWeaponsMassThreshold, 0f, 100f);
            listing.Gap();

            // Medium Weapons Threshold
            listing.Label("CGF_Settings_MediumThreshold".Translate(ModSettings.mediumWeaponsMassThreshold.ToString("F1")));
            float newMedium = listing.Slider(ModSettings.mediumWeaponsMassThreshold, 0f, 100f);
            listing.Gap();

            // Heavy Weapons Threshold
            listing.Label("CGF_Settings_HeavyThreshold".Translate(ModSettings.heavyWeaponsMassThreshold.ToString("F1")));
            float newHeavy = listing.Slider(ModSettings.heavyWeaponsMassThreshold, 0f, 100f);
            listing.Gap();

            ModSettings.lightWeaponsMassThreshold = newLight;
            ModSettings.mediumWeaponsMassThreshold = newMedium;
            ModSettings.heavyWeaponsMassThreshold = newHeavy;

            // Validation and application
            bool isValid = ValidateThresholds(newLight, newMedium, newHeavy);

            if (!isValid)
            {
                GUI.color = Color.red;
                listing.Label("CGF_Settings_ValidationWarning".Translate());
                listing.Label("CGF_Settings_ValidationRequired".Translate());
                GUI.color = Color.white;
            }

            listing.Gap();

            // Reset to defaults button
            if (listing.ButtonText("CGF_Settings_ResetButton".Translate()))
            {
                ModSettings.lightWeaponsMassThreshold = 2.0f;
                ModSettings.mediumWeaponsMassThreshold = 5.0f;
                ModSettings.heavyWeaponsMassThreshold = 10.0f;
            }

            listing.Gap();
            listing.Label("CGF_Settings_Note".Translate());

            listing.End();
        }

        private static bool ValidateThresholds(float light, float medium, float heavy)
        {
            return light < medium && medium < heavy;
        }
    }
}