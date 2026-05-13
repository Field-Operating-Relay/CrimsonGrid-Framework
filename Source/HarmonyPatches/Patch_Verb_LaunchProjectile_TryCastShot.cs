using HarmonyLib;
using RimWorld;
using Verse;

namespace CrimsonGridFramework
{
	[HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
	public static class Patch_Verb_LaunchProjectile_TryCastShot
	{
		public static void Postfix(Verb_LaunchProjectile __instance, bool __result)
		{
			if (__result && __instance.caster is Building_TurretGun turret)
			{
				var deployableComp = turret.TryGetComp<CompDeployableBuilding>();
				deployableComp?.Notify_ShotFired();
			}
		}
	}
}