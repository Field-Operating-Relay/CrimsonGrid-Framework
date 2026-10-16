using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CrimsonGridFramework
{
    public class Verb_CastLaunchProjectileStatic : Verb_LaunchProjectileStatic
    {
        protected override bool TryCastShot()
        {
            if (base.TryCastShot())
            {
                if (DirectOwner is Ability ability)
                {
                    return ability.Activate(currentTarget, currentDestination);
                }
                return true;
            }
            return false;
        }
    }
}
