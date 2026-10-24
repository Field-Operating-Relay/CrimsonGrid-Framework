using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CrimsonGridFramework
{
    public class Command_TurretTarget : Command_VerbTarget
    {
        public CompTurretGunExtended turretComp;

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();

            if (turretComp != null)
            {
                // Start targeting with a custom action that sets the forced target
                Find.Targeter.BeginTargeting(
                    verb.verbProps.targetParams,
                    (LocalTargetInfo target) => {
                        if (target.IsValid)
                        {
                            turretComp.SetForcedTarget(target);
                        }
                    },
                    verb.CasterPawn,
                    null,
                    verb.UIIcon
                );
            }
            else
            {
                // Fallback to normal verb targeting if no extended turret comp
                Find.Targeter.BeginTargeting(verb, null, allowNonSelectedTargetingSource: false, null, null, requiresAvailableVerb);
            }
        }
    }
}