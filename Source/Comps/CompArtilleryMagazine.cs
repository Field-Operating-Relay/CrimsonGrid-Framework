using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;

namespace CrimsonGridFramework
{
    public class CompProperties_ArtilleryMagazine : CompProperties
    {
        public int maxAmmo = 1;
        public CompProperties_ArtilleryMagazine()
        {
            this.compClass = typeof(CompArtilleryMagazine);
        }
    }
    public class CompArtilleryMagazine : ThingComp
    {
        public Building_TurretGun Parent => parent as Building_TurretGun;
        public CompChangeableProjectile ProjectileComp => Parent.gun.TryGetComp<CompChangeableProjectile>();
        public CompProperties_ArtilleryMagazine Props => (CompProperties_ArtilleryMagazine)props;
        public int MaxAmmo => Props.maxAmmo;
        public int CurrentAmmo => currentAmmo.Sum(ac => ac.count);
        public class AmmoCount : IExposable
        {
            public ThingDef ammoDef;
            public int count;

            public void ExposeData()
            {
                Scribe_Values.Look(ref count, "count", 0);
                Scribe_Defs.Look(ref ammoDef, "ammoDef");
            }
        }
        public Queue<AmmoCount> currentAmmo = new Queue<AmmoCount>();
        public void ChamberShell()
        {
            ProjectileComp.LoadShell(currentAmmo.Peek().ammoDef, 1);
            currentAmmo.Peek().count = currentAmmo.Peek().count - 1;
            if (currentAmmo.Peek().count <= 0)
            {
                currentAmmo.Dequeue();
            }
        }
        public void LoadShells(ThingDef ammoDef, int count)
        {
            if (CurrentAmmo + count > MaxAmmo)
            {
                Thing shells = ThingMaker.MakeThing(ammoDef);
                shells.stackCount = count - MaxAmmo - CurrentAmmo;
                GenPlace.TryPlaceThing(shells, parent.Position, parent.Map, ThingPlaceMode.Near);
                count = MaxAmmo - CurrentAmmo;
            }
            var existingAmmo = currentAmmo.LastOrDefault();
            if (existingAmmo != null && existingAmmo.ammoDef == ammoDef)
            {
                existingAmmo.count += count;
            }
            else
            {
                currentAmmo.Enqueue(new AmmoCount { ammoDef = ammoDef, count = count });
            }
            if (!ProjectileComp.Loaded)
            {
                ChamberShell();
            }
        }
        public void UnloadAllShells()
        {
            while (currentAmmo.Any())
            {
                Thing shells = ThingMaker.MakeThing(currentAmmo.Peek().ammoDef);
                shells.stackCount = currentAmmo.Peek().count;
                GenPlace.TryPlaceThing(shells, parent.Position, parent.Map, ThingPlaceMode.Near);
                currentAmmo.Dequeue();
            }
            GenPlace.TryPlaceThing(ProjectileComp.RemoveShell(), parent.Position, parent.Map, ThingPlaceMode.Near);
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref currentAmmo, "currentAmmo", LookMode.Deep);
        }

        public bool SpawnMissile(GlobalTargetInfo globalTargetInfo)
        {
            if (ProjectileComp.Loaded)
            {
                ActiveTransporter activeDropPod = (ActiveTransporter)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod);
                activeDropPod.Contents = new ActiveTransporterInfo();
                FlyShipLeaving flyShipLeaving = (FlyShipLeaving)SkyfallerMaker.MakeSkyfaller(CrimsonGridFramework_DefOfs.CG_RocketLeaving, activeDropPod);
                flyShipLeaving.groupID = 0;
                flyShipLeaving.destinationTile = globalTargetInfo.Tile;
                flyShipLeaving.worldObjectDef = CrimsonGridFramework_DefOfs.CG_TravellingRocket;
                flyShipLeaving.arrivalAction = new TransportersArrivalAction_SpawnRocketProjectileOnTarget()
                {
                    GlobalTargetInfo = globalTargetInfo,
                    ProjectileToSpawn = ProjectileComp.Projectile,
                    launcher = parent
                };
                GenSpawn.Spawn(flyShipLeaving, parent.Position, parent.Map);
                ProjectileComp.RemoveShell();
                ChamberShell();
                return true;
            }
            return false;
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            var unloadShells = new Command_Action
            {
                action = delegate
                {
                    UnloadAllShells();
                },
                defaultLabel = "Unload Shells",
                defaultDesc = null,
                icon = null
            };
            if (CurrentAmmo <= 0)
            {
                unloadShells.Disabled = true;
                unloadShells.disabledReason = "CG_NoShellsToUnload".Translate();
            }
            else
            {
                unloadShells.Disabled = false;
                unloadShells.disabledReason = null;
            }
            yield return unloadShells;
            if (!DebugSettings.ShowDevGizmos)
            {
                yield break;
            }
            yield return new Command_Action
            {
                action = delegate
                {
                    LoadShells(ThingDefOf.Shell_HighExplosive, 5);
                    Log.Message(CurrentAmmo.ToString());
                    ChamberShell();
                    Log.Message(CurrentAmmo.ToString());
                },
                defaultLabel = "Fill Shells",
                defaultDesc = null,
                icon = null
            };
        }
        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new(base.CompInspectStringExtra());
            sb.AppendLine("CG_NextShell".Translate() + ": " + (CurrentAmmo > 0 ? currentAmmo.Peek().ammoDef.LabelCap : "CG_None".Translate()));
            sb.Append($"Ammo: {CurrentAmmo}/{MaxAmmo}");
            return sb.ToString();
        }
    }
}
