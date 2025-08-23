using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CrimsonGridFramework;

public class Hediff_MechControllable : HediffWithComps
{
    public override IEnumerable<Gizmo> GetGizmos()
    {
        Command_Toggle command_Toggle = new()
        {
            hotKey = KeyBindingDefOf.Command_ColonistDraft,
            isActive = () => pawn.Drafted,
            toggleAction = delegate
            {
                pawn.drafter.Drafted = !pawn.drafter.Drafted;
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Drafting, KnowledgeAmount.SpecificInteraction);
                if (pawn.Drafted)
                {
                    LessonAutoActivator.TeachOpportunity(ConceptDefOf.QueueOrders, OpportunityType.GoodToKnow);
                }
            },
            defaultDesc = "CommandToggleDraftDesc".Translate(),
            icon = TexCommand.Draft,
            turnOnSound = SoundDefOf.DraftOn,
            turnOffSound = SoundDefOf.DraftOff,
            groupKey = 81729172,
            defaultLabel = (pawn.Drafted ? "CommandUndraftLabel" : "CommandDraftLabel").Translate()
        };

        if (pawn.Downed)
        {
            command_Toggle.Disable("IsIncapped".Translate(pawn.LabelShort, this));
        }

        command_Toggle.tutorTag = !pawn.Drafted ? "Draft" : "Undraft";

        yield return command_Toggle;

        if (pawn.Drafted && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsRangedWeapon)
        {
            yield return new Command_Toggle
            {
                hotKey = KeyBindingDefOf.Misc6,
                isActive = () => pawn.drafter.FireAtWill,
                toggleAction = delegate
                {
                    pawn.drafter.FireAtWill = !pawn.drafter.FireAtWill;
                },
                icon = TexCommand.FireAtWill,
                defaultLabel = "CommandFireAtWillLabel".Translate(),
                defaultDesc = "CommandFireAtWillDesc".Translate(),
                tutorTag = "FireAtWillToggle"
            };
        }
    }
}
