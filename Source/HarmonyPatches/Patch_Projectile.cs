using HarmonyLib;
using Verse;
using UnityEngine;

namespace CrimsonGridFramework.HarmonyPatches
{
    [HarmonyPatch(typeof(Projectile), "Launch", new[] { typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags), typeof(bool), typeof(Thing), typeof(ThingDef) })]
    public static class Patch_Projectile_Launch
    {
        public static void Postfix(Projectile __instance)
        {
            if (__instance?.Map != null)
            {
                var tracker = __instance.Map.GetComponent<MapComponent_ProjectileTracker>();
                tracker.RegisterProjectile(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(Projectile), "Impact")]
    public static class Patch_Projectile_Impact
    {
        public static void Prefix(Projectile __instance)
        {
            if (__instance?.Map != null)
            {
                var tracker = __instance.Map.GetComponent<MapComponent_ProjectileTracker>();
                tracker.UnregisterProjectile(__instance);
            }
        }
    }
}