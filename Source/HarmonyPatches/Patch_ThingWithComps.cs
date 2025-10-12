using HarmonyLib;
using Verse;

namespace CrimsonGridFramework.HarmonyPatches
{
    [HarmonyPatch(typeof(ThingWithComps), "Destroy")]
    public static class Patch_ThingWithComps_Destroy
    {
        public static void Prefix(ThingWithComps __instance)
        {
            if (__instance?.Map != null && __instance is Projectile projectile)
            {
                var tracker = __instance.Map.GetComponent<MapComponent_ProjectileTracker>();
                tracker.UnregisterProjectile(projectile);
            }
        }
    }
}
