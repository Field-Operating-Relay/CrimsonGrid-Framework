using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework.HarmonyPatches
{
    [HarmonyPatch(typeof(VerbUtility), "GetProjectile")]
    public static class Patch_VerbUtility_GetProjectile
    {
        public static void Postfix(ThingDef __result, Verb verb)
        {
            if (__result == null && verb is Verb_SpawnRocket)
            {
                __result = verb.Caster.TryGetComp<CompArtilleryMagazine>(out var s) ? s.ProjectileComp.Projectile : null;
            }
        }
    }
}
