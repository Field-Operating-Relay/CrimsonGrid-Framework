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
        public bool IsDisconnected => pawn.CurJobDef == CrimsonGridFramework_DefOfs.Disconnected;
        public bool IsRefuelling => pawn.CurJobDef == CrimsonGridFramework_DefOfs.CG_GetFuelJob;
        public bool IsPoweredDown => pawn.CurJobDef == CrimsonGridFramework_DefOfs.CG_PoweredDown;
        public bool needToShutdown = false;
        private float BaseFallPerDay
        {
            get
            {
                if (pawn.mindState != null && !pawn.mindState.IsIdle)
                {
                    return 0.2f;
                }
                return 0.05f;
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
                if (IsRefuelling)
                {
                    return 0f;
                }
                if (IsDisconnected)
                {
                    return 0f;
                }
                if (IsPoweredDown)
                {
                    return 0f;
                }
                return BaseFallPerDay * pawn.GetStatValue(CrimsonGridFramework_DefOfs.CG_FuelEnergyUsageFactor);
            }
        }
        public Need_Fuel(Pawn newPawn) : base(newPawn){}
        public override void NeedInterval()
        {
            if (!IsDisconnected)
            {
                CurLevel -= FallPerDay / 400f;
            }
            if (CurLevel <= 0f)
            {
                needToShutdown = true;
            }
            else if (CurLevel >= 0.15f || IsRefuelling)
            {
                needToShutdown = false;
            }
        }
    }
}
