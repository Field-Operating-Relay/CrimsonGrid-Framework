using System.Collections.Generic;
using Verse;

namespace CrimsonGridFramework
{
    public enum LootType
    {
        FixedSet,
        RandomResources
    }

    public class CompProperties_LootBox : CompProperties
    {
        public LootType lootType;
        public List<List<ThingDefCountRangeClass>> fixedItemSets;
        public List<ThingDefCountRangeClass> resourcePool;
        public IntRange variance;
        public GraphicData openGraphicData;

        public CompProperties_LootBox()
        {
            compClass = typeof(CompLootBox);
        }
    }
}