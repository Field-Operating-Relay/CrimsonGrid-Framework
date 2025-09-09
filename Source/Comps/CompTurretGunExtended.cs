using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace CrimsonGridFramework
{
    public class CompTurretGunExtended : CompTurretGun
    {
        private static readonly CachedTexture ToggleTurretIcon = new CachedTexture("UI/Gizmos/ToggleTurret");

        // Forced target functionality
        private LocalTargetInfo forcedTarget = LocalTargetInfo.Invalid;
        private bool hasForcedTarget = false;

        // Duplicate private members from base class that we need
        private LocalTargetInfo myLastAttackedTarget = LocalTargetInfo.Invalid;
        private int myLastAttackTargetTick;

        // Our own fireAtWill tracking
        private bool myFireAtWill = true;

        // Properties for forced targeting
        public LocalTargetInfo ForcedTarget
        {
            get => forcedTarget;
            set
            {
                forcedTarget = value;
                hasForcedTarget = value.IsValid;
            }
        }

        public bool HasForcedTarget => hasForcedTarget && forcedTarget.IsValid;

        // Expose our tracked values similar to base class
        public new LocalTargetInfo LastAttackedTarget => myLastAttackedTarget;
        public new int LastAttackTargetTick => myLastAttackTargetTick;

        // Duplicate private properties from base class
        private bool MyCanShoot
        {
            get
            {
                if (parent is Pawn pawn)
                {
                    if (!pawn.Spawned || pawn.Downed || pawn.Dead || !pawn.Awake())
                        return false;

                    if (pawn.stances.stunner.Stunned)
                        return false;

                    if (MyTurretDestroyed)
                        return false;

                    if (pawn.IsColonyMechPlayerControlled && !MyFireAtWill)
                        return false;
                }

                CompCanBeDormant compCanBeDormant = parent.TryGetComp<CompCanBeDormant>();
                if (compCanBeDormant != null && !compCanBeDormant.Awake)
                    return false;

                return true;
            }
        }

        private bool MyTurretDestroyed
        {
            get
            {
                if (parent is Pawn pawn && AttackVerb.verbProps.linkedBodyPartsGroup != null &&
                    AttackVerb.verbProps.ensureLinkedBodyPartsGroupAlwaysUsable &&
                    PawnCapacityUtility.CalculateNaturalPartsAverageEfficiency(pawn.health.hediffSet, AttackVerb.verbProps.linkedBodyPartsGroup) <= 0f)
                {
                    return true;
                }
                return false;
            }
        }

        private bool MyFireAtWill
        {
            get
            {
                // We track our own fireAtWill since the base class field is private
                return myFireAtWill;
            }
        }

        private bool MyWarmingUp => burstWarmupTicksLeft > 0;

        // Forced target methods
        public void ClearForcedTarget()
        {
            forcedTarget = LocalTargetInfo.Invalid;
            hasForcedTarget = false;
        }

        public void SetForcedTarget(LocalTargetInfo target)
        {
            if (target.IsValid)
            {
                ForcedTarget = target;
                // Immediately start targeting if we can shoot
                if (MyCanShoot && burstCooldownTicksLeft <= 0)
                {
                    currentTarget = forcedTarget;
                    burstWarmupTicksLeft = 1;
                }
            }
        }

        public override void CompTick()
        {
            if (!MyCanShoot)
            {
                return;
            }

            if (currentTarget.IsValid)
            {
                curRotation = (currentTarget.Cell.ToVector3Shifted() - parent.DrawPos).AngleFlat() + Props.angleOffset;
            }

            AttackVerb.VerbTick();

            if (AttackVerb.state == VerbState.Bursting)
            {
                return;
            }

            if (MyWarmingUp)
            {
                burstWarmupTicksLeft--;
                if (burstWarmupTicksLeft == 0)
                {
                    AttackVerb.TryStartCastOn(currentTarget, surpriseAttack: false, canHitNonTargetPawns: true, preventFriendlyFire: false, nonInterruptingSelfCast: true);
                    myLastAttackTargetTick = Find.TickManager.TicksGame;
                    myLastAttackedTarget = currentTarget;
                }
                return;
            }

            if (burstCooldownTicksLeft > 0)
            {
                burstCooldownTicksLeft--;
            }

            if (burstCooldownTicksLeft <= 0 && parent.IsHashIntervalTick(10))
            {
                // Check if we have a forced target and it's still valid
                if (HasForcedTarget)
                {
                    if (IsForcedTargetStillValid())
                    {
                        currentTarget = forcedTarget;
                        burstWarmupTicksLeft = 1;
                        return;
                    }
                    else
                    {
                        // Forced target is no longer valid, clear it
                        ClearForcedTarget();
                    }
                }

                // Fall back to automatic target finding
                currentTarget = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(this, TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable);
                if (currentTarget.IsValid)
                {
                    burstWarmupTicksLeft = 1;
                }
                else
                {
                    MyResetCurrentTarget();
                }
            }
        }

        private bool IsForcedTargetStillValid()
        {
            if (!forcedTarget.IsValid)
                return false;

            // Check if target still exists and is alive
            if (forcedTarget.Thing != null)
            {
                if (forcedTarget.Thing.Destroyed)
                    return false;

                if (forcedTarget.Thing is Pawn pawn && (pawn.Dead || pawn.Downed))
                    return false;
            }

            // Check if we can still shoot at the target (line of sight, range, etc.)
            if (!AttackVerb.CanHitTarget(forcedTarget))
                return false;

            return true;
        }

        private void MyResetCurrentTarget()
        {
            currentTarget = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Get base gizmos but replace the fire at will toggle with our own
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                // Skip the base fire at will toggle, we'll add our own
                if (gizmo is Command_Toggle toggle && toggle.defaultLabel == "CommandToggleTurret".Translate())
                    continue;

                yield return gizmo;
            }

            if (parent is Pawn pawn && pawn.IsColonyMechPlayerControlled)
            {
                // Our own fire at will toggle
                Command_Toggle command_Toggle = new Command_Toggle();
                command_Toggle.defaultLabel = "CommandToggleTurret".Translate();
                command_Toggle.defaultDesc = "CommandToggleTurretDesc".Translate();
                command_Toggle.isActive = () => myFireAtWill;
                command_Toggle.icon = ToggleTurretIcon.Texture;
                command_Toggle.toggleAction = delegate
                {
                    myFireAtWill = !myFireAtWill;
                };
                yield return command_Toggle;

                // Forced target gizmo for Crimson Grid robots
                if (pawn.IsCrimsonGridRobot() && pawn.Faction == Faction.OfPlayer && pawn.MentalStateDef == null)
                {
                    yield return CreateForcedTargetCommand();
                }
            }
        }

        private Command_VerbTarget CreateForcedTargetCommand()
        {
            Command_TurretTarget command = new Command_TurretTarget()
            {
                turretComp = this
            };

            command.defaultDesc = gun.LabelCap + ": " + gun.def.description.CapitalizeFirst();
            command.ownerThing = gun;
            command.tutorTag = "VerbTarget";
            command.verb = AttackVerb;
            //command.defaultLabel = "Force Target";
            command.icon = AttackVerb.UIIcon;

            // Handle disable conditions
            if (AttackVerb.caster.Faction != Faction.OfPlayer && !DebugSettings.ShowDevGizmos)
            {
                command.Disable("CannotOrderNonControlled".Translate());
            }
            else if (AttackVerb.CasterIsPawn && !AttackVerb.CasterPawn.Drafted && !DebugSettings.ShowDevGizmos)
            {
                command.Disable("IsNotDrafted".Translate(AttackVerb.CasterPawn.LabelShort, AttackVerb.CasterPawn));
            }

            // Add weather range cap warning if applicable
            if (AttackVerb.EquipmentSource != null &&
                AttackVerb.caster.Spawned &&
                AttackVerb.caster.Map.weatherManager.CurWeatherMaxRangeCap >= 0f)
            {
                command.defaultDescPostfix = "\n\n" + ("WeatherMaxRangeCap".Translate() + ": " + AttackVerb.caster.Map.weatherManager.curWeather.LabelCap).Colorize(ColoredText.WarningColor);
            }

            return command;
        }

        private Command_Action CreateClearTargetCommand()
        {
            return new Command_Action
            {
                defaultLabel = "Clear Target",
                defaultDesc = "Clear the forced target and return to automatic targeting.",
                icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),
                action = () => ClearForcedTarget()
            };
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_TargetInfo.Look(ref forcedTarget, "forcedTarget");
            Scribe_Values.Look(ref hasForcedTarget, "hasForcedTarget", false);
            Scribe_Values.Look(ref myFireAtWill, "myFireAtWill", true);
            Scribe_TargetInfo.Look(ref myLastAttackedTarget, "myLastAttackedTarget");
            Scribe_Values.Look(ref myLastAttackTargetTick, "myLastAttackTargetTick", 0);
        }
    }
}