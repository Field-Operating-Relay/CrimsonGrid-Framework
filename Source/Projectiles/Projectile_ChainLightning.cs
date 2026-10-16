using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    [StaticConstructorOnStartup]
    public class Projectile_ChainLightning : Projectile
    {
        private static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt");
        private static readonly Mesh UnitBoltMesh = CreateUnitLineMesh();
        private List<Pawn> chainPawns = new List<Pawn>();
        private Vector3 startPosition;
        private Vector3 intendedTargetPosition;
        private int expirationTicks;
        private bool impactProcessed;
        private bool shouldDestroy;

        private static Mesh CreateUnitLineMesh()
        {
            var mesh = new Mesh();
            float halfWidth = 0.15f;
            mesh.vertices = new Vector3[] { new(-halfWidth,0,0), new(halfWidth,0,0), new(-halfWidth,0,1), new(halfWidth,0,1) };
            mesh.uv = new Vector2[] { new(0,0), new(1,0), new(0,1), new(1,1) };
            mesh.triangles = new int[] { 0,2,1, 2,3,1 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventSpecialEffects = false, Thing equipment = null, ThingDef targetWeaponDef = null)
        {
            intendedTargetPosition = intendedTarget.Cell.ToVector3Shifted();
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventSpecialEffects, equipment, targetWeaponDef);
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (impactProcessed)
            {
                return;
            }

            impactProcessed = true;
            startPosition = DrawPos;
            expirationTicks = Find.TickManager.TicksGame + 60;
            shouldDestroy = true;

            if (blockedByShield || !(hitThing is Pawn pawn))
            {
                return;
            }

            var extension = def.GetModExtension<ChainLightningExtension>();
            FindChainPawns(pawn, extension.maxChains, chainPawns, extension);
            for (int i = 0; i < chainPawns.Count; i++)
            {
                ApplyDamageAndStun(chainPawns[i], extension);
            }
        }

        private void FindChainPawns(Pawn currentPawn, int remainingChains, List<Pawn> hitPawns, ChainLightningExtension extension)
        {
            if (remainingChains <= 0 || currentPawn.Dead || currentPawn.Destroyed)
            {
                return;
            }

            if (!hitPawns.Contains(currentPawn))
            {
                hitPawns.Add(currentPawn);
            }
            bool canHitLauncher = false;

            if (launcher is Pawn launcherPawn && currentPawn.Position.DistanceTo(launcher.Position) <= extension.chainRange)
            {
                if (Rand.Value <= extension.chanceToHitLauncher)
                {
                    canHitLauncher = true;
                }
            }
            var nextPawn = (Pawn)GenClosest.ClosestThingReachable(
                currentPawn.Position,
                currentPawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.Pawn),
                PathEndMode.OnCell,
                TraverseParms.For(TraverseMode.NoPassClosedDoors),
                extension.chainRange,
                t => {
                    bool isPawn = t is Pawn;
                    if (!isPawn) return false;
                    Pawn p = (Pawn)t;
                    bool result = !hitPawns.Contains(p) && !p.Dead && !p.Downed && (canHitLauncher || t != launcher);
                    return result;
                }
            );

            if (nextPawn != null)
            {
                FindChainPawns(nextPawn, remainingChains - 1, hitPawns, extension);
            }
        }

        private void ApplyDamageAndStun(Pawn pawn, ChainLightningExtension extension)
        {
            var damageAmount = def.projectile.GetDamageAmount(null);

            var damageInfo = new DamageInfo(
                def.projectile.damageDef,
                damageAmount,
                0f,
                -1f,
                launcher,
                null,
                def
            );

            pawn.TakeDamage(damageInfo);

            if (pawn.stances?.stunner != null)
            {
                pawn.stances.stunner.StunFor(extension.stunTicks, launcher, addBattleLog: true, showMote: true);
            }
        }

        protected override void Tick()
        {
            base.Tick();
            if (shouldDestroy)
            {
                if (Find.TickManager.TicksGame >= expirationTicks)
                {
                    shouldDestroy = false;
                    Destroy();
                }
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (!impactProcessed)
            {
                base.DrawAt(drawLoc, flip);
                return;
            }

            var points = new List<Vector3> { launcher.DrawPos };
            foreach (var p in chainPawns)
                points.Add(p.DrawPos);

            if (chainPawns.Count == 0)
                points.Add(DrawPos);

            var material = FadedMaterialPool.FadedVersionOf(LightningMat, 1f);

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 fromPos = points[i];
                Vector3 toPos = points[i + 1];
                Vector3 direction = (toPos - fromPos).normalized;
                var distance = Vector3.Distance(fromPos, toPos);

                var matrix = Matrix4x4.TRS(fromPos, Quaternion.FromToRotation(Vector3.forward, direction),
                                          new Vector3(1f, 1f, distance));
                Graphics.DrawMesh(UnitBoltMesh, matrix, material, 0);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref chainPawns, "chainPawns", LookMode.Reference);
            Scribe_Values.Look(ref startPosition, "startPosition");
            Scribe_Values.Look(ref intendedTargetPosition, "intendedTargetPosition");
            Scribe_Values.Look(ref expirationTicks, "expirationTicks");
            Scribe_Values.Look(ref impactProcessed, "impactProcessed");
            Scribe_Values.Look(ref shouldDestroy, "shouldDestroy");
        }
    }
}
