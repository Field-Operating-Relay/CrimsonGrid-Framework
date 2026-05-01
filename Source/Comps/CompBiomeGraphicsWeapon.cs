using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CrimsonGridFramework
{
    public enum BiomeCategory
    {
        Forest,
        Desert,
        Snow,
        Other
    }

    public class BiomeSkinMapping
    {
        public BiomeCategory biomeCategory;
        public GraphicData graphicData;
    }

    public class CompProperties_BiomeGraphicsWeapon : CompProperties
    {
        public List<BiomeSkinMapping> skins;
        public CompProperties_BiomeGraphicsWeapon()
        {
            compClass = typeof(CompBiomeGraphicsWeapon);
        }
    }
    [HotSwappable]
    public class CompBiomeGraphicsWeapon : ThingComp
    {
        private Dictionary<BiomeCategory, Graphic> cachedGraphics;
        private BiomeCategory currentBiomeCategory;
        public BiomeDef currentBiome;
        public CompProperties_BiomeGraphicsWeapon Props => (CompProperties_BiomeGraphicsWeapon)props;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                UpdateGraphic(parent.Map);
            });
        }

        public void UpdateGraphic(Map map)
        {
            var newCategory = GetBiomeCategory(map);
            if (newCategory != currentBiomeCategory)
            {
                currentBiomeCategory = newCategory;
                ReflectionCache.weaponGraphic(parent) = BiomeGraphic();
            }
        }

        public override void CompTickLong()
        {
            base.CompTickLong();
            var map = parent.MapHeld;
            if (map != null)
            {
                UpdateGraphic(map);
            }
        }

        public BiomeCategory GetBiomeCategory(Map map)
        {
            var biome = map.Biome;
            if (biome.defName.ToLower().Contains("forest") || biome == BiomeDefOf.TropicalSwamp)
                return BiomeCategory.Forest;
            if (biome == BiomeDefOf.Desert)
                return BiomeCategory.Desert;
            if (biome == BiomeDefOf.IceSheet || biome == BiomeDefOf.SeaIce)
                return BiomeCategory.Snow;

            var topGrid = map.terrainGrid.topGrid;
            var terrainCounts = new Dictionary<TerrainDef, int>();

            for (int i = 0; i < topGrid.Length; i++)
            {
                var terrain = topGrid[i];
                if (!terrainCounts.ContainsKey(terrain))
                    terrainCounts[terrain] = 0;
                terrainCounts[terrain]++;
            }

            var mostCommon = terrainCounts.MaxBy(pair => pair.Value).Key;
            if (mostCommon.categoryType == TerrainDef.TerrainCategoryType.Sand)
                return BiomeCategory.Desert;
            if (mostCommon.categoryType == TerrainDef.TerrainCategoryType.Soil)
                return BiomeCategory.Other;
            if (mostCommon.HasTag("Ice") || map.snowGrid.TotalDepth >= 200)
                return BiomeCategory.Snow;

            return BiomeCategory.Other;
        }

        public Graphic BiomeGraphic()
        {
            if (currentBiomeCategory == BiomeCategory.Other)
                return parent.def.graphicData.GraphicColoredFor(parent);
            cachedGraphics ??= new Dictionary<BiomeCategory, Graphic>();
            if (!cachedGraphics.TryGetValue(currentBiomeCategory, out var graphic))
            {
                var mapping = Props.skins.FirstOrDefault(s => s.biomeCategory == currentBiomeCategory);
                if (mapping != null && mapping.graphicData != null)
                {
                    graphic = mapping.graphicData.GraphicColoredFor(parent);
                    cachedGraphics[currentBiomeCategory] = graphic;
                }
            }
            return graphic;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentBiomeCategory, "currentBiomeCategory", BiomeCategory.Other);
        }
    }

    public static class ReflectionCache
    {
        public static readonly AccessTools.FieldRef<Thing, Graphic> weaponGraphic =
            AccessTools.FieldRefAccess<Thing, Graphic>(AccessTools.Field(typeof(Thing), "graphicInt"));
    }
}
