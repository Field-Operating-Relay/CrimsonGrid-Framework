using CrimsonGridFramework.Comps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework
{
    public class MapComponent_LocalBandwidth : MapComponent
    {
        WorldComponent_GridBandwidth gridBandwidth => WorldComponent_GridBandwidth.Instance;
        public MapComponent_LocalBandwidth(Map map) : base(map)
        {
        }
    }
}
