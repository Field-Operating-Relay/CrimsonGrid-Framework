using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace CrimsonGridFramework
{
    [StaticConstructorOnStartup]
    public class Projectile_TOW : Projectile_Explosive
    {
        private static readonly Material WireMat;
        private static readonly Mesh WireSegMesh;

        private static readonly AccessTools.FieldRef<Projectile, Vector3> OriginRef =
            AccessTools.FieldRefAccess<Projectile, Vector3>("origin");
        private static readonly AccessTools.FieldRef<Projectile, Vector3> DestinationRef =
            AccessTools.FieldRefAccess<Projectile, Vector3>("destination");
        private static readonly AccessTools.FieldRef<Projectile, int> TicksToImpactRef =
            AccessTools.FieldRefAccess<Projectile, int>("ticksToImpact");

        static Projectile_TOW()
        {
            var tex = SolidColorMaterials.NewSolidColorTexture(new Color(0.07f, 0.07f, 0.07f, 1f));
            WireMat = new Material(ShaderDatabase.Transparent);
            WireMat.mainTexture = tex;

            WireSegMesh = BuildWireSegMesh();
        }

        private static Mesh BuildWireSegMesh()
        {
            var mesh = new Mesh();
            const float halfWidth = 0.5f;
            mesh.vertices = new Vector3[]
            {
                new(-halfWidth, 0f, 0f),
                new( halfWidth, 0f, 0f),
                new(-halfWidth, 0f, 1f),
                new( halfWidth, 0f, 1f)
            };
            mesh.uv = new Vector2[] { new(0,0), new(1,0), new(0,1), new(1,1) };
            mesh.triangles = new int[] { 0,2,1, 2,3,1 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private Vector3 launchPos;
        private Vector3 originalOrigin;
        private float originalDistance;
        private int originalTotalTicks;
        private float baseArcHeight;
        private List<Vector3> wireHistory = new List<Vector3>();
        private Vector3 currentVelocity;
        private int ticksAlive;
        private LocalTargetInfo myIntendedTarget;
        private bool launched;

        private TOWMissileExtension Extension => def.GetModExtension<TOWMissileExtension>() ?? new TOWMissileExtension();

        private float ArcHeight
        {
            get
            {
                float num = def.projectile.arcHeightFactor;
                var num2 = (DestinationRef(this) - OriginRef(this)).MagnitudeHorizontalSquared();
                if (num * num > num2 * 0.2f * 0.2f)
                    num = Mathf.Sqrt(num2) * 0.2f;
                return num;
            }
        }

        private float GetArcFraction()
        {
            if (originalDistance <= 0.0001f) return 1f;
            var curPos = ExactPosition;
            var targetPos = myIntendedTarget.HasThing
                ? myIntendedTarget.Thing.DrawPos
                : myIntendedTarget.Cell.ToVector3Shifted();
            if (Extension.useArcTrajectory)
                targetPos.z += 1f;

            float dist = (targetPos.Yto0() - curPos.Yto0()).magnitude;
            return Mathf.Clamp01(1f - (dist / originalDistance));
        }

        public override void Launch(
            Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget,
            ProjectileHitFlags hitFlags, bool preventSpecialEffects = false,
            Thing equipment = null, ThingDef targetWeaponDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventSpecialEffects, equipment, targetWeaponDef);

            myIntendedTarget = intendedTarget;
            var targetPos = myIntendedTarget.HasThing ? myIntendedTarget.Thing.DrawPos : myIntendedTarget.Cell.ToVector3Shifted();
            if (Extension.useArcTrajectory)
                targetPos.z += 1f;

            originalOrigin = OriginRef(this);
            originalDistance = (targetPos.Yto0() - originalOrigin.Yto0()).magnitude;
            originalTotalTicks = Mathf.CeilToInt(originalDistance / def.projectile.SpeedTilesPerTick);
            float num = def.projectile.arcHeightFactor;
            float num2 = originalDistance * originalDistance;
            if (num * num > num2 * 0.2f * 0.2f)
                num = Mathf.Sqrt(num2) * 0.2f;
            baseArcHeight = num;

            launchPos = origin;
            launchPos.y = launcher.DrawPos.y - 0.01f;
            wireHistory.Add(launchPos);

            currentVelocity = (targetPos.Yto0() - origin.Yto0()).normalized;
            launched = true;
        }
        public override Quaternion ExactRotation
        {
            get
            {
                if (!Extension.useArcTrajectory || currentVelocity == default || originalDistance <= 0.1f)
                    return base.ExactRotation;

                var t = GetArcFraction();
                float speed = def.projectile.SpeedTilesPerTick;

                Vector3 visualVel = currentVelocity * speed;
                float verticalSpeed = baseArcHeight * (4f - 8f * t) / originalTotalTicks;
                visualVel.z += verticalSpeed;
                visualVel.y = 0f;

                if (visualVel.sqrMagnitude > 0.001f)
                    return Quaternion.LookRotation(visualVel);

                return Quaternion.LookRotation(currentVelocity);
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (hitThing != intendedTarget.Thing)
            {
                foreach (var thing in GenRadial.RadialDistinctThingsAround(Position, Map, 3f, true))
                {
                    if (thing == intendedTarget.Thing)
                    {
                        if (Vector3.Distance(thing.DrawPos.Yto0(), ExactPosition.Yto0()) <= 0.5f)
                        {
                            hitThing = thing;
                        }
                    }
                }
            }
            base.Impact(hitThing, blockedByShield);
        }

        protected override void Tick()
        {
            if (launched && myIntendedTarget.IsValid)
            {
                var curPos = ExactPosition;
                var targetPos = myIntendedTarget.HasThing
                    ? myIntendedTarget.Thing.DrawPos
                    : myIntendedTarget.Cell.ToVector3Shifted();
                if (Extension.useArcTrajectory)
                    targetPos.z += 1f;
                var toTarget = targetPos - curPos;
                toTarget.y = 0f;
                float dist = toTarget.magnitude;

                var dot = Vector3.Dot(currentVelocity, toTarget.normalized);
                const float guidanceLossThreshold = 0f;

                if (dist > 0.1f && dot > guidanceLossThreshold)
                {
                    float maxRad = Extension.maxTurnRateDegreesPerTick * Mathf.Deg2Rad;
                    currentVelocity = Vector3.RotateTowards(currentVelocity, toTarget / dist, maxRad, 0f);
                    currentVelocity.Normalize();
                }

                float speed = def.projectile.SpeedTilesPerTick;
                var newOrigin = new Vector3(curPos.x, OriginRef(this).y, curPos.z);
                OriginRef(this) = newOrigin;

                const float farDistance = 1000f;
                DestinationRef(this) = newOrigin + currentVelocity * (dist <= speed ? dist : farDistance);
                TicksToImpactRef(this) = Mathf.CeilToInt((dist <= speed ? dist : farDistance) / speed);
            }

            ticksAlive++;
            var wirePoint = ExactPosition;
            if (Extension.useArcTrajectory)
            {
                var t = GetArcFraction();
                float offset = baseArcHeight * GenMath.InverseParabola(t);
                wirePoint.z += offset;
            }
            wireHistory.Add(wirePoint);
            base.Tick();
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (wireHistory.Count < 2)
                return;

            var ext = Extension;
            float phase = ticksAlive * ext.waveAnimSpeed;
            int count = wireHistory.Count;
            var points = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                Vector3 segDir;
                if (i == 0)
                    segDir = wireHistory[1] - wireHistory[0];
                else if (i == count - 1)
                    segDir = wireHistory[count - 1] - wireHistory[count - 2];
                else
                    segDir = wireHistory[i + 1] - wireHistory[i - 1];

                segDir.y = 0f;
                if (segDir.sqrMagnitude > 0.001f) segDir.Normalize();
                var perp = new Vector3(-segDir.z, 0f, segDir.x);

                float t = i / (float)(count - 1);
                var wave = Mathf.Sin(t * ext.wireSCount * Mathf.PI * 2f - phase);
                points[i] = wireHistory[i] + perp * (ext.wireAmplitude * wave);
                points[i].y = wireHistory[i].y;
            }

            for (int i = 0; i < count - 1; i++)
                DrawWireSegment(points[i], points[i + 1], ext.wireWidth);

            if (Extension.useArcTrajectory)
            {
                var t = GetArcFraction();
                float num = baseArcHeight * GenMath.InverseParabola(t);
                Vector3 position = drawLoc + new Vector3(0f, 0f, 1f) * num;
                Graphics.DrawMesh(MeshPool.GridPlane(DrawSize), position, ExactRotation, DrawMat, 0);
                Comps_PostDraw();
            }
            else
            {
                base.DrawAt(drawLoc, flip);
            }
        }

        private void DrawWireSegment(Vector3 from, Vector3 to, float width)
        {
            Vector3 dir = to - from;
            float len = dir.magnitude;
            if (len < 0.001f)
                return;

            var mat = Matrix4x4.TRS(
                from,
                Quaternion.FromToRotation(Vector3.forward, dir / len),
                new Vector3(width, 1f, len)
            );
            Graphics.DrawMesh(WireSegMesh, mat, WireMat, 0);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref launchPos, "launchPos");
            Scribe_Collections.Look(ref wireHistory, "wireHistory", LookMode.Value);
            Scribe_Values.Look(ref currentVelocity, "currentVelocity");
            Scribe_Values.Look(ref ticksAlive, "ticksAlive");
            Scribe_Values.Look(ref launched, "launched");
            Scribe_TargetInfo.Look(ref myIntendedTarget, "myIntendedTarget");
            Scribe_Values.Look(ref originalOrigin, "originalOrigin");
            Scribe_Values.Look(ref originalDistance, "originalDistance");
            Scribe_Values.Look(ref originalTotalTicks, "originalTotalTicks");
            Scribe_Values.Look(ref baseArcHeight, "baseArcHeight");
        }
    }
}
