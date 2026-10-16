using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    public class WorkGiver_ResupplyLauncher : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;
        public JobDef JobDef => CrimsonGridFramework_DefOfs.CG_ResupplyLauncher;
        private Thing ammo;
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerBuildings.AllColonistBuildingsOfType<Building_TurretGunTopless>().Where(b =>
            {
                var comp = b.TryGetComp<CompTopDownArtillery>();
                return comp != null && comp.CurrentAmmo < comp.MaxAmmo;
            });
        }
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_TurretGunTopless launcher))
            {
                return false;
            }
            if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger()))
            {
                return false;
            }
            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
            {
                return false;
            }
            if (t.IsBurning())
            {
                return false;
            }
            CompTopDownArtillery comp = launcher.TryGetComp<CompTopDownArtillery>();
            if(comp == null)
            {
                return false;
            }
            if (comp.CurrentAmmo >= comp.MaxAmmo)
            {
                JobFailReason.Is("CG_MagazineFull".Translate());
                return false;
            }
            ammo = FindAmmo(pawn, t as Building_TurretGunTopless);
            if (ammo == null)
            {
                JobFailReason.Is("CG_NoAmmoFound".Translate());
                return false;
            }
            return true;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var job = JobMaker.MakeJob(JobDef, t, ammo);
            job.count = 1;
            return job;
        }
        public static Thing FindAmmo(Pawn pawn, Building_TurretGunTopless gun)
        {
            StorageSettings allowedShellsSettings = ((pawn.IsColonist || pawn.IsColonyMech) ? gun.gun.TryGetComp<CompChangeableProjectile>().allowedShellsSettings : null);
            return GenClosest.ClosestThingReachable(gun.Position, gun.Map, ThingRequest.ForGroup(ThingRequestGroup.Shell), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, ShellValidator);
            bool ShellValidator(Thing t)
            {
                if (t.IsForbidden(pawn))
                {
                    return false;
                }
                if (!pawn.CanReserve(t, 1, 1))
                {
                    return false;
                }
                if (allowedShellsSettings != null && !allowedShellsSettings.AllowedToAccept(t))
                {
                    return false;
                }
                if (pawn.Faction != Faction.OfPlayer && t.def.projectileWhenLoaded?.projectile != null && !t.def.projectileWhenLoaded.projectile.damageDef.harmsHealth)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
