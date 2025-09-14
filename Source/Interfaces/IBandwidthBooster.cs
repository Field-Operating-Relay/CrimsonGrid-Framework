using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrimsonGridFramework
{
    public interface IBandwidthBooster
    {
        public int BandwidthBoostAmount { get; }
        public bool Enabled { get; }
/*        public bool Enable();
        public bool Disable();*/
    }
}
