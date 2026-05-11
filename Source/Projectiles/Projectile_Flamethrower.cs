using RimWorld;
using UnityEngine;
using Verse;

namespace CrimsonGridFramework
{
    [HotSwappable]
    public class Projectile_Flamethrower : Projectile
    {
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            var comp = equipment.TryGetComp<CompFlamethrower>();
            var map = Map;
            var cell = Position;
            if (Rand.Chance(comp.Props.napalmSpawnChance))
            {
                if (map.thingGrid.ThingAt(cell, comp.Props.filthDef) != null)
                    SpreadToNearestUnnapalmed(cell, map, comp.Props.filthDef, comp.Props.fireStartChance);
                else
                    FilthMaker.TryMakeFilth(cell, map, comp.Props.filthDef);
            }
            base.Impact(hitThing, blockedByShield);
        }

        private const float FastRadius = 2.5f;

        private static void SpreadToNearestUnnapalmed(IntVec3 origin, Map map, ThingDef filthDef, float fireStartChance)
        {
            var maxRadius = Mathf.Min(Mathf.Max(map.Size.x, map.Size.z), 79);
            foreach (var cur in GenRadial.RadialCellsAround(origin, maxRadius, true))
            {
                if (cur == origin) continue;
                if (!cur.InBounds(map)) continue;
                if (cur.GetTerrain(map).IsWater) continue;
                if (map.thingGrid.ThingAt(cur, filthDef) != null) continue;

                var dist = cur.DistanceTo(origin);
                var chance = dist <= FastRadius ? 1f : Mathf.Pow(FastRadius / dist, 4f);
                if (!Rand.Chance(chance))
                    return;

                FilthMaker.TryMakeFilth(cur, map, filthDef);
                if (Rand.Chance(fireStartChance))
                    FireUtility.TryStartFireIn(cur, map, 0.1f, null, null);
                return;
            }
        }
    }
}
