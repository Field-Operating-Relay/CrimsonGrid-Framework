using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CrimsonGridFramework
{
    public class Upgrade
    {
        public int workAmount = 0;
        public int additionalBandwidth;
        public int additionalPowerDraw;
        public int AdditionalHeatPerSecond;
        public GraphicData graphicData;
        public List<ThingDefCountClass> ingredients = new List<ThingDefCountClass>();
        public Upgrade() { }
    }
    public class CompProperties_UpgradeableBuilding : CompProperties
    {
        public List<Upgrade> upgrades;

        public CompProperties_UpgradeableBuilding()
        {
            compClass = typeof(CompUpgradeableBuilding);
        }
    }
    [StaticConstructorOnStartup]
    public class CompUpgradeableBuilding : ThingComp, IBandwidthBooster
    {
        public static HashSet<Thing> buildingsWithUpgradeInProgress = [];
        private static readonly Material UnderfieldMat = MaterialPool.MatFrom("Things/Building/BuildingFrame/Underfield", ShaderDatabase.Transparent);

        private static readonly Texture2D CornerTex = ContentFinder<Texture2D>.Get("Things/Building/BuildingFrame/Corner");

        private static readonly Texture2D TileTex = ContentFinder<Texture2D>.Get("Things/Building/BuildingFrame/Tile");

        private static readonly Color FrameColor = new Color(0.6f, 0.6f, 0.6f);
        private Material cachedCornerMat;
        private Material cachedTileMat;
        private Graphic cachedGraphic;
        public CompProperties_UpgradeableBuilding Props => (CompProperties_UpgradeableBuilding)props;
        public CompHeatPusher heatPusher;
        public CompPowerTrader powerTrader;
        public IBuildingWithBandwidth buildingWithBandwidth;
        public int CurrentUpgradeLevel = 0;
        public int MinUpgradeLevel = 0;
        public int MaxUpgradeLevel => Props.upgrades.Count - 1;

        private int TargetUpgradeLevel;
        public bool isUpgrading = false;
        public float upgradeWorkLeft = 0;
        public ThingOwner<Thing> upgradeContainer = new ThingOwner<Thing>();
        public Upgrade CurrentUpgrade => Props.upgrades[CurrentUpgradeLevel];
        public Upgrade TargetUpgrade => Props.upgrades[TargetUpgradeLevel];
        private Graphic OverlayGraphic
        {
            get
            {
                if (cachedGraphic == null)
                {
                    cachedGraphic = CurrentUpgrade.graphicData.GraphicColoredFor(parent);
                }
                return cachedGraphic;
            }
        }
        private Material CornerMat
        {
            get
            {
                if (!cachedCornerMat)
                {
                    cachedCornerMat = MaterialPool.MatFrom(CornerTex, ShaderDatabase.MetaOverlay, FrameColor);
                }
                return cachedCornerMat;
            }
        }

        private Material TileMat
        {
            get
            {
                if (!cachedTileMat)
                {
                    cachedTileMat = MaterialPool.MatFrom(TileTex, ShaderDatabase.MetaOverlay, FrameColor);
                }
                return cachedTileMat;
            }
        }
        public int BandwidthBoostAmount => CurrentUpgrade.additionalBandwidth;
        public bool Enabled => true;
        public bool StoredCostSatisfied
        {
            get
            {
                if (!isUpgrading)
                {
                    return false;
                }
                foreach (ThingDefCountClass thingDefCount in TargetUpgrade.ingredients)
                {
                    if (upgradeContainer.TotalStackCountOfDef(thingDefCount.thingDef) < thingDefCount.count)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public float PercentComplete
        {
            get
            {
                if (!isUpgrading || !(TargetUpgrade.workAmount > 0f))
                {
                    return 0f;
                }
                return 1f - (float)upgradeWorkLeft / (float)TargetUpgrade.workAmount;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (heatPusher != null && heatPusher.enabled && parent.IsHashIntervalTick(60))
            {
                GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, CurrentUpgrade.AdditionalHeatPerSecond);
            }
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerTrader = parent.GetComp<CompPowerTrader>();
            heatPusher = parent.GetComp<CompHeatPusher>();
            buildingWithBandwidth = (IBuildingWithBandwidth)parent.AllComps.Where(c => c is IBuildingWithBandwidth).First();
            buildingWithBandwidth.TryBoostBandwidth(this);
        }
        public override void PostDraw()
        {
            base.PostDraw();
            if (CurrentUpgrade.graphicData != null)
            {
                Vector3 loc = parent.DrawPos + Altitudes.AltIncVect;
                loc.y += 5f;
                OverlayGraphic.Draw(loc, parent.Rotation, parent);
            }
            if (isUpgrading)
            {
                Vector3 drawPos = parent.DrawPos;
                Matrix4x4 matrix = Matrix4x4.TRS(s: new Vector3((float)parent.def.Size.x * 1.15f, 1f, (float)parent.def.Size.z * 1.15f), pos: drawPos, q: parent.Rotation.AsQuat);
                Graphics.DrawMesh(MeshPool.plane10, matrix, UnderfieldMat, 0);
                for (int i = 0; i < 4; i++)
                {
                    float num2 = (float)Mathf.Min(parent.RotatedSize.x, parent.RotatedSize.z) * 0.38f;
                    IntVec3 intVec = i switch
                    {
                        0 => new IntVec3(-1, 0, -1),
                        1 => new IntVec3(-1, 0, 1),
                        2 => new IntVec3(1, 0, 1),
                        3 => new IntVec3(1, 0, -1),
                        _ => throw new InvalidOperationException("Rot4"),
                    };
                    Vector3 b = new Vector3
                    {
                        x = (float)intVec.x * ((float)parent.RotatedSize.x / 2f - num2 / 2f),
                        z = (float)intVec.z * ((float)parent.RotatedSize.z / 2f - num2 / 2f)
                    };
                    Vector3 s2 = new Vector3(num2, 1f, num2);
                    Matrix4x4 matrix2 = default(Matrix4x4);
                    matrix2.SetTRS(drawPos + Vector3.up * 0.03f + b, new Rot4(i).AsQuat, s2);
                    Graphics.DrawMesh(MeshPool.plane10, matrix2, CornerMat, 0);
                }
                int tiles = Mathf.CeilToInt(PercentComplete * (float)parent.RotatedSize.x * (float)parent.RotatedSize.z * 4f);
                IntVec2 intVec2 = parent.RotatedSize * 2;
                for (int j = 0; j < tiles; j++)
                {
                    IntVec2 intVec3 = default(IntVec2);
                    intVec3.z = j / intVec2.x;
                    intVec3.x = j - intVec3.z * intVec2.x;
                    Vector3 a = new Vector3((float)intVec3.x * 0.5f, 0f, (float)intVec3.z * 0.5f) + drawPos;
                    a.x -= (float)parent.RotatedSize.x * 0.5f - 0.25f;
                    a.z -= (float)parent.RotatedSize.z * 0.5f - 0.25f;
                    Vector3 s3 = new Vector3(0.5f, 1f, 0.5f);
                    Matrix4x4 matrix3 = default(Matrix4x4);
                    matrix3.SetTRS(a + Vector3.up * 0.02f, Quaternion.identity, s3);
                    Graphics.DrawMesh(MeshPool.plane10, matrix3, TileMat, 0);
                }
            }
        }
        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("CGF_CurrentLevel".Translate(CurrentUpgradeLevel));
            if (isUpgrading)
            {
                if (!StoredCostSatisfied)
                {
                    stringBuilder.AppendLine("CGF_WaitingForIngredients");
                    foreach(ThingDefCountClass thingDefCountClass in TargetUpgrade.ingredients)
                    {
                        stringBuilder.AppendLine($"- {thingDefCountClass.thingDef.LabelCap}: {upgradeContainer.TotalStackCountOfDef(thingDefCountClass.thingDef)}/{thingDefCountClass.count}");
                    }
                }
                else
                {
                    stringBuilder.AppendLine("CGF_Upgrading".Translate(upgradeWorkLeft));
                }
                stringBuilder.AppendLine(PercentComplete.ToString());
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if(CurrentUpgradeLevel < MaxUpgradeLevel)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "CG_Upgrade".Translate(),
                    defaultDesc = "CG_UpgradeDesc".Translate(Props.upgrades[CurrentUpgradeLevel + 1].additionalBandwidth, Props.upgrades[CurrentUpgradeLevel + 1].additionalPowerDraw, Props.upgrades[CurrentUpgradeLevel + 1].AdditionalHeatPerSecond, Props.upgrades[CurrentUpgradeLevel + 1].ingredients.ToStringSafeEnumerable()),
                    icon = null,
                    activateSound = SoundDefOf.Designate_DragBuilding,
                    action = () =>
                    {
                        isUpgrading = true;
                        TargetUpgradeLevel = CurrentUpgradeLevel + 1;
                        buildingsWithUpgradeInProgress.Add(parent);
                        upgradeWorkLeft = TargetUpgrade.workAmount;
                    }
                };
            }
            if (isUpgrading)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "CG_CancelUpgrade".Translate(),
                    defaultDesc = "CG_CancelUpgradeDesc".Translate(),
                    icon = null,
                    activateSound = SoundDefOf.Designate_Cancel,
                    action = () =>
                    {
                        TargetUpgradeLevel = 0;
                        isUpgrading = false;
                        buildingsWithUpgradeInProgress.Remove(parent);
                        upgradeContainer.TryDropAll(parent.Position, parent.Map, ThingPlaceMode.Near, playDropSound: false);
                    }
                };
            }
            yield return new Command_Action()
            {
                defaultLabel = "Test Upgrade",
                icon = null,
                action = () =>
                {
                    isUpgrading = true;
                    TargetUpgradeLevel = 1;
                    buildingsWithUpgradeInProgress.Add(parent);
                }
            };
        }
        public void FinishUpgrade()
        {
            upgradeContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            isUpgrading = false;
            CurrentUpgradeLevel = CurrentUpgradeLevel + 1;
            buildingsWithUpgradeInProgress.Remove(parent);
            TargetUpgradeLevel = 0;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref CurrentUpgradeLevel, "CurrentUpgradeLevel", 0);
            Scribe_Deep.Look(ref upgradeContainer, "upgradeContainer");
            Scribe_Values.Look(ref isUpgrading, "isUpgrading", false);
            Scribe_Values.Look(ref upgradeWorkLeft, "upgradeWorkLeft", 0);
            Scribe_Values.Look(ref TargetUpgradeLevel, "TargetUpgradeLevel", 0);

            if (upgradeContainer == null)
            {
                upgradeContainer = new ThingOwner<Thing>();
            }
        }
        public void AddToContainer(ThingOwner<Thing> innerContainer, Thing thing, int count)
        {
            innerContainer.TryTransferToContainer(thing, upgradeContainer, count);
        }
    }
}
