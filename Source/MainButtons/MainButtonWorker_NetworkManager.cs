using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrimsonGridFramework
{
    public class MainButtonWorker_NetworkManager : MainButtonWorker
    {
        public override void Activate()
        {
            Find.WindowStack.Add(new Dialog_NetworkManager());
        }
    }
}
