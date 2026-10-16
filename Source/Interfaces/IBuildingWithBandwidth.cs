using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrimsonGridFramework
{
    public interface IBuildingWithBandwidth
    {
        public bool Enabled { get; }
        public bool TryBoostBandwidth(IBandwidthBooster booster);
        public bool TryUnboostBandwidth(IBandwidthBooster booster);
    }
}
