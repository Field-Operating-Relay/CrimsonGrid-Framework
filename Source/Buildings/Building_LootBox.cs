using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace CrimsonGridFramework
{
    public class Building_LootBox : Building, IThingHolder, IOpenable, ISearchableContents
    {
        protected ThingOwner<Thing> innerContainer;
        protected bool contentsKnown;
        private bool isOpened;
        private Graphic closedGraphic;
        private Graphic openGraphic;

        public int OpenTicks => 60;

        public bool CanOpen => !isOpened && HasAnyContents;

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public ThingOwner SearchableContents => innerContainer;

        public bool HasAnyContents => innerContainer.Count > 0;
        private CompProperties_LootBox _props;
        private CompProperties_LootBox Props => _props ??= def.GetCompProperties<CompProperties_LootBox>();
        public Building_LootBox()
        {
            innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad && Props != null)
            {
                GenerateLoot();
            }
        }

        private void GenerateLoot()
        {
            if (Props.lootType == LootType.FixedSet && Props.fixedItemSets != null)
            {
                GenerateFixedSetLoot();
            }
            else if (Props.lootType == LootType.RandomResources && Props.resourcePool != null)
            {
                GenerateRandomResourceLoot();
            }
        }

        private void GenerateFixedSetLoot()
        {
            var chosenSet = Props.fixedItemSets.RandomElement();
            if (chosenSet != null)
            {
                foreach (var item in chosenSet)
                {
                    var count = item.countRange.RandomInRange;
                    Thing thing;
                    if (item.thingDef.MadeFromStuff)
                    {
                        thing = ThingMaker.MakeThing(item.thingDef, GenStuff.DefaultStuffFor(item.thingDef));
                    }
                    else
                    {
                        thing = ThingMaker.MakeThing(item.thingDef);
                    }
                    thing.stackCount = count;
                    innerContainer.TryAdd(thing);
                }
            }
        }

        private void GenerateRandomResourceLoot()
        {
            if (Props.resourcePool.Count == 0)
            {
                return;
            }

            var variance = Props.variance.RandomInRange;
            for (int i = 0; i < variance; i++)
            {
                var chosenResource = Props.resourcePool.RandomElement();
                if (chosenResource != null)
                {
                    var count = chosenResource.countRange.RandomInRange;
                    Thing thing;
                    if (chosenResource.thingDef.MadeFromStuff)
                    {
                        thing = ThingMaker.MakeThing(chosenResource.thingDef, GenStuff.DefaultStuffFor(chosenResource.thingDef));
                    }
                    else
                    {
                        thing = ThingMaker.MakeThing(chosenResource.thingDef);
                    }
                    thing.stackCount = count;
                    innerContainer.TryAdd(thing);
                }
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (openGraphic != null || Props.openGraphicData != null)
            {
                if (closedGraphic is null)
                {
                    closedGraphic = Graphic;
                }
                if (openGraphic is null)
                {
                    openGraphic = Props.openGraphicData.GraphicColoredFor(this);
                }
                if (!isOpened)
                {
                    closedGraphic.Draw(drawLoc, Rotation, this);
                }
                else
                {
                    openGraphic.Draw(drawLoc, Rotation, this);
                }
            }
            else
            {
                base.DrawAt(drawLoc, flip);
            }
        }

        public void Open()
        {
            if (HasAnyContents && !isOpened)
            {
                EjectContents();
                isOpened = true;
                if (def.building.openingEffect != null)
                {
                    var effecter = def.building.openingEffect.Spawn();
                    effecter.Trigger(new TargetInfo(Position, Map), null);
                    effecter.Cleanup();
                }
                DirtyMapMesh(Map);
            }
        }

        public void EjectContents()
        {
            innerContainer.TryDropAll(Position, Map, ThingPlaceMode.Near);
            contentsKnown = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_Values.Look(ref contentsKnown, "contentsKnown", defaultValue: false);
            Scribe_Values.Look(ref isOpened, "isOpened", defaultValue: false);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            var map = Map;
            base.Destroy(mode);
            if (innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
            {
                innerContainer.TryDropAll(Position, map, ThingPlaceMode.Near);
            }
            innerContainer.ClearAndDestroyContents();
        }

        public override string GetInspectString()
        {
            var text = base.GetInspectString();
            var str = contentsKnown ? innerContainer.ContentsString : ((string)"UnknownLower".Translate());
            if (!text.NullOrEmpty())
            {
                text += "\n";
            }
            return text + ("Contains".Translate() + ": " + str.CapitalizeFirst());
        }
    }
}
