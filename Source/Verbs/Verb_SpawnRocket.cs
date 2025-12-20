using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework
{
    public class Verb_SpawnRocket : Verb_LaunchProjectile
    {
        protected override bool TryCastShot()
        {
            if (base.Caster.TryGetComp<CompTopDownArtillery>(out var s))
            {
                return s.SpawnMissile();
            }
            return false;
            
        }
    }
}
