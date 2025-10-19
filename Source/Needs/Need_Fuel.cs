using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    public class Need_Fuel : Need
    {
        private float BaseFallPerDay
        {
            get
            {
                if (pawn.mindState != null && !pawn.mindState.IsIdle)
                {
                    return 10f;
                }
                return 3f;
            }
        }
        public float FallPerDay
        {
            get
            {
                if (pawn.Downed)
                {
                    return 0f;
                }
                if (!pawn.Awake())
                {
                    return 0f;
                }
                if (pawn.IsCaravanMember())
                {
                    return 0f;
                }
                return BaseFallPerDay * pawn.GetStatValue(CrimsonGridFramework_DefOfs.CG_FuelEnergyUsageFactor);
            }
        }

        public bool IsDisconnected => pawn.CurJobDef == CrimsonGridFramework_DefOfs.Disconnected;
        public bool needToShutdown = false;
        public Need_Fuel(Pawn newPawn) : base(newPawn){}
        public override void NeedInterval()
        {
            float num = 400f;
            if (!IsDisconnected)
            {
                CurLevel -= FallPerDay / num;
            }
            if (CurLevel <= 0f)
            {
                needToShutdown = true;
            }
            else if (CurLevel >= 15f || pawn.CurJobDef == JobDefOf.MechCharge)
            {
                needToShutdown = false;
            }
        }
    }
}
