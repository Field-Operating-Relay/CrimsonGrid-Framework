using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework
{
    public class Verb_SpawnRocket : Verb
    {
        protected override bool TryCastShot()
        {
            if (base.Caster.TryGetComp<CompArtilleryMagazine>(out var s))
            {
                return s.SpawnMissile(CurrentTarget.ToGlobalTargetInfo(caster.Map));
            }
            return false;
        }
    }
}
