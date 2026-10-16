using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework
{
    public class ModSettings : Verse.ModSettings
    {
        public static float lightWeaponsMassThreshold = 2.5f;
        public static float mediumWeaponsMassThreshold = 5.0f;
        public static float heavyWeaponsMassThreshold = 10.0f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lightWeaponsMassThreshold, "lightWeaponsMassThreshold", 2.5f);
            Scribe_Values.Look(ref mediumWeaponsMassThreshold, "mediumWeaponsMassThreshold", 5.0f);
            Scribe_Values.Look(ref heavyWeaponsMassThreshold, "heavyWeaponsMassThreshold", 10.0f);
        }
    }
}
