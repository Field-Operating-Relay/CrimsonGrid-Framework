using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework
{
    
    public static class Utils
    {
        [StaticConstructorOnStartup]
        public static class HarmonyStarter
        {
            static HarmonyStarter()
            {
                Harmony harmony = new Harmony("CrimsonGridFramework");
                harmony.PatchAll();
                Logger.Message("Harmony Patches Applied");
            }
        }    
    }
}
