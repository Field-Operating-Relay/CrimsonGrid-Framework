using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace CrimsonGridFramework
{
    public class Recipe_InstallCrimsonGridModification : RecipeWorker
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            // Only apply to Crimson Grid robots that are player-controlled
            if (!pawn.IsCrimsonGridRobot() || pawn.Faction != Faction.OfPlayer)
            {
                yield break;
            }

            for (int i = 0; i < recipe.appliedOnFixedBodyParts.Count; i++)
            {
                BodyPartDef part = recipe.appliedOnFixedBodyParts[i];
                List<BodyPartRecord> bpList = pawn.RaceProps.body.AllParts;

                for (int j = 0; j < bpList.Count; j++)
                {
                    BodyPartRecord record = bpList[j];
                    if (record.def == part)
                    {
                        // Check if the part exists and isn't missing
                        if (pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).Contains(record))
                        {
                            // Check if this modification isn't already installed
                            if (!pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == record && x.def == recipe.addsHediff))
                            {
                                yield return record;
                            }
                        }
                    }
                }
            }
            yield break;
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (!pawn.IsCrimsonGridRobot())
            {
                Log.Error($"Attempted to apply Crimson Grid modification to non-robot: {pawn.LabelShort}");
                return;
            }

            if (billDoer != null)
            {
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
            }

            pawn.health.AddHediff(this.recipe.addsHediff, part, null);
        }
    }
}