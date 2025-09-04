using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework
{
    [DefOf]
    [SuppressMessage("ReSharper", "UnassignedField.Global")]
    public static class CrimsonGridFramework_DefOfs
    {
        public static JobDef Disconnected;
        public static HediffDef CG_Hediff_Draftable;
        public static HediffDef CG_GlobalBottleneck;
        public static HediffDef CG_RelayBottleneck;
    }
}
