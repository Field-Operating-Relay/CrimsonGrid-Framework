using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace CrimsonGridFramework.HarmonyPatches
{
    [HarmonyPatch(typeof(CaravanUIUtility), "AddPawnsSections")]
    public class Patch_AddPawnsSections_Prefix_Robots
    {
        public static List<TransferableOneWay> transferables;
        public static void Prefix(List<TransferableOneWay> transferables)
        {
            Patch_AddPawnsSections_Prefix_Robots.transferables = transferables;
        }
    }

    [HarmonyPatch(typeof(CaravanUIUtility), "AddPawnsSections")]
    public class Patch_AddPawnsSections_Postfix_Robots
    {
        public static bool Prepare(MethodBase original)
        {
            return !ModsConfig.BiotechActive;
        }
        public static void Postfix(TransferableOneWayWidget widget, List<TransferableOneWay> transferables)
        {
            IEnumerable<TransferableOneWay> source = transferables.Where((TransferableOneWay x) => x.ThingDef.category == ThingCategory.Pawn);
            widget.AddSection("CGF_Caravan_MechSection_Title".Translate(), source.Where((TransferableOneWay x) => ((Pawn)x.AnyThing).IsCrimsonGridRobot() && ((Pawn)x.AnyThing).Faction == Faction.OfPlayer));
        }
    }

    [HarmonyPatch(typeof(TransferableOneWayWidget), "AddSection")]
    public class Patch_AddSection_Robots
    {
        public static bool Prepare(MethodBase original)
        {
            return ModsConfig.BiotechActive;
        }
        public static void Prefix(string title, ref IEnumerable<TransferableOneWay> transferables)
        {
            if (title != "MechsSection".Translate())
            {
                return;
            }
            List<TransferableOneWay> modRobots = Patch_AddPawnsSections_Prefix_Robots.transferables.Where(transferable =>
            {
                if (transferable.ThingDef.category == ThingCategory.Pawn)
                {
                    var pawn = transferable.AnyThing as Pawn;
                    return pawn.IsCrimsonGridRobot() && pawn.IsConnected() && pawn.Faction == Faction.OfPlayer;
                }
                return false;
            }).ToList();
            modRobots.AddRange(transferables);
            transferables = modRobots;
        }
    }
}
