using System.Collections.Generic;
using Verse;

namespace CrimsonGridFramework
{
    public class WeaponWeightClassExtension : DefModExtension
    {
        public MechWeightClassDef targetMechWeightClass;
        public List<WeaponStatModifier> debuffStats;

        public bool applyEmpStunOnShoot = false;
        public bool applyFireDamageOnShoot = false;
        public bool crimsonRobotsOnly = false;
        public float fireDamageAmount = 5f;

        public WeaponWeightClassExtension()
        {
            targetMechWeightClass = null;
            debuffStats = null;
            applyEmpStunOnShoot = false;
            applyFireDamageOnShoot = false;
            crimsonRobotsOnly = false;
            fireDamageAmount = 5f;
        }
    }
}