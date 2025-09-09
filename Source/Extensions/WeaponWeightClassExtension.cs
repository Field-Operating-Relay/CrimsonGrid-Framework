using System.Collections.Generic;
using Verse;
using RimWorld;

namespace CrimsonGridFramework
{
    public class WeaponWeightClassExtension : DefModExtension
    {
        public MechWeightClassDef targetMechWeightClass;
        public List<StatModifier> debuffStats;

        public bool applyEmpStunOnFire = false;
        public bool applyFireDamageOnFire = false;
        public float fireDamageAmount = 5f;

        public WeaponWeightClassExtension()
        {
            targetMechWeightClass = null;
            debuffStats = null;
            applyEmpStunOnFire = false;
            applyFireDamageOnFire = false;
            fireDamageAmount = 5f;
        }
    }
}