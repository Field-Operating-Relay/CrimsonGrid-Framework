using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CrimsonGridFramework
{
    public class Building_TurretGunTopless : Building_TurretGun
    {
        public override LocalTargetInfo TryFindNewTarget()
        {
            return base.TryFindNewTarget();
        }
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            return; // Intentionally left blank to prevent drawing the turret top
        }
    }
}
