using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework
{
    public class CompProperties_FuelStation : CompProperties
    {
        public int fuelTransferRate = 1;
        public CompProperties_FuelStation()
        {
            this.compClass = typeof(CompFuelStation);
        }
    }
    public class CompFuelStation : ThingComp
    {
        public CompProperties_FuelStation Props => (CompProperties_FuelStation)this.props;
        public CompRefuelable RefuelableComp => this.parent.GetComp<CompRefuelable>();
        public bool HasFuel => RefuelableComp != null && RefuelableComp.HasFuel;

    }
}
