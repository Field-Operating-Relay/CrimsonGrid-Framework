using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using Verse.Sound;
using static UnityEngine.GraphicsBuffer;

namespace CrimsonGridFramework
{
    public class CompProperties_TopDownArtillery : CompProperties
    {
        public int maxAmmo = 1;
        public int fireMissionRange = 10;
        public CompProperties_TopDownArtillery()
        {
            this.compClass = typeof(CompTopDownArtillery);
        }
    }
    public class CompTopDownArtillery : ThingComp
    {
        public Building_TurretGun Parent => parent as Building_TurretGun;
        public CompChangeableProjectile ProjectileComp => Parent.gun.TryGetComp<CompChangeableProjectile>();
        public CompProperties_TopDownArtillery Props => (CompProperties_TopDownArtillery)props;
        public GlobalTargetInfo target;
        public int MaxAmmo => Props.maxAmmo;
        public int FireMissionRange => Props.fireMissionRange;
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

        public bool SpawnMissile()
        {
            if (ProjectileComp.Loaded)
            {
                GlobalTargetInfo globalTargetInfo;
                if (target.IsValid)
                {
                    globalTargetInfo = target;
                    target = GlobalTargetInfo.Invalid;
                }
                else
                {
                    globalTargetInfo = Parent.CurrentTarget.ToGlobalTargetInfo(Parent.Map);
                }
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
        public void StartFireMission()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(parent));
            Find.WorldSelector.ClearSelection();
            PlanetTile tile = parent.Map.Tile;
            Find.WorldTargeter.BeginTargeting(TargetChosen, canTargetTiles: false, null, closeWorldTabWhenFinished: true, delegate
            {
                GenDraw.DrawWorldRadiusRing(tile, FireMissionRange);
            });

        }

        private bool TargetChosen(GlobalTargetInfo target)
        {
            if (!target.IsValid)
            {
                Messages.Message("CG_FireMissionTargetInvalid".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }
            int distance = Find.WorldGrid.TraversalDistanceBetween(parent.Map.Tile, target.Tile);
            if (distance > FireMissionRange)
            {
                Messages.Message("CG_FireMissionTargetBeyondMaximumRange".Translate(), parent, MessageTypeDefOf.RejectInput);
                return false;
            }
            Map artilleryMap;
            if (target.WorldObject is MapParent { HasMap: not false } mapParent)
            {
                artilleryMap = parent.Map;
                Map map = mapParent.Map;
                Current.Game.CurrentMap = map;
                Targeter targeter = Find.Targeter;
                targeter.BeginTargeting(new TargetingParameters
                {
                    canTargetPawns = true,
                    canTargetBuildings = true,
                    canTargetLocations = true
                }, (LocalTargetInfo x) => { FireMission(map.Tile, x, map.uniqueID); });
                return true;
            }
            Messages.Message("CG_FireMissionNeedMap".Translate(), MessageTypeDefOf.RejectInput);
            return false;
        }
        public void FireMission(int tile, LocalTargetInfo targ, int map)
        {
            if (!targ.IsValid)
            {
                return;
            }
            GlobalTargetInfo newtarget = targ.ToGlobalTargetInfo(Find.Maps.FirstOrDefault((Map x) => x.uniqueID == map));
            int num = Find.WorldGrid.TraversalDistanceBetween(parent.Map.Tile, tile);
            if (num > FireMissionRange)
            {
                Messages.Message("CG_FireMissionTargetBeyondMaximumRange".Translate(), parent, MessageTypeDefOf.RejectInput);
                return;
            }
            if (Parent.burstCooldownTicksLeft <= 0)
            {
                target = newtarget;
                Parent.currentTargetInt = Parent;
                Parent.burstWarmupTicksLeft = Parent.gun.def.Verbs[0].warmupTime.SecondsToTicks();
            }
            SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            var crossFireMission = new Command_Action
            {
                action = StartFireMission,
                defaultLabel = "CG_CrossFireMission".Translate(),
                defaultDesc = "CG_CrossFireMissionDesc".Translate(),
                icon = null,
                Disabled = ProjectileComp.Loaded == false || Parent.burstCooldownTicksLeft > 0,
            };
            yield return crossFireMission;
            var unloadShells = new Command_Action
            {
                action = delegate
                {
                    UnloadAllShells();
                },
                defaultLabel = "CG_UnloadShells".Translate(),
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
