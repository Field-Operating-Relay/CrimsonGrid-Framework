using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework
{
    public static class Logger
    {
        static string CrimsonGridFrameworkStart = "[CrimsonGridFramework]";
        public static void Error(string message)
        {
            Log.Error(CrimsonGridFrameworkStart + message);
        }
        public static void Message(string message)
        {
            Log.Message(CrimsonGridFrameworkStart + message);
        }
        public static void Warning(string message)
        {
            Log.Warning(CrimsonGridFrameworkStart + message);
        }
    }
}
