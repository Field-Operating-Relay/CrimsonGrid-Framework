using HarmonyLib;
using Verse;

namespace CrimsonGridFramework
{
    [HarmonyPatch(typeof(Thing), "DefaultGraphic", MethodType.Getter)]
    public static class Thing_DefaultGraphic_Patch
    {
        public static bool Prefix(Thing __instance, ref Graphic __result)
        {
            if (ReflectionCache.weaponGraphic(__instance) is null && __instance is ThingWithComps thingWithComps)
            {
                var comp = thingWithComps.GetComp<CompBiomeGraphicsWeapon>();
                if (comp != null)
                {
                    var graphic = comp.BiomeGraphic();
                    __result = ReflectionCache.weaponGraphic(__instance) = graphic;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn), "SpawnSetup")]
    public static class Pawn_SpawnSetup_Patch
    {
        public static void Postfix(Pawn __instance)
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                var weapon = __instance.equipment?.Primary;
                if (weapon != null)
                {
                    var comp = weapon.GetComp<CompBiomeGraphicsWeapon>();
                    if (comp != null)
                    {
                        comp.UpdateGraphic(__instance.Map);
                    }
                }
            });
        }
    }
}
