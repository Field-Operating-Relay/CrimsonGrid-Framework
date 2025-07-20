using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework.Comps
{
    public class WorldComponent_GridBandwidth : WorldComponent
    {
        private static WorldComponent_GridBandwidth _instance;
        public static WorldComponent_GridBandwidth Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WorldComponent_GridBandwidth(Find.World);
                }
                return _instance;
            }
        }
        public WorldComponent_GridBandwidth(World world) : base(world)
        {
        }
    }
}
