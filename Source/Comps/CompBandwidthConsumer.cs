using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CrimsonGridFramework
{
    public class CompProperties_BandwidthConsumer : CompProperties
    {
        public int bandwidthAmount = 5;
        public CompProperties_BandwidthConsumer()
        {
            compClass = typeof(CompBandwidthConsumer);
        }
    }
    public class CompBandwidthConsumer : ThingComp
    {
        public Pawn pawn => (Pawn)parent;

        public CompProperties_BandwidthConsumer Props => (CompProperties_BandwidthConsumer)props;
        WorldComponent_GridBandwidth gridBandwidth => WorldComponent_GridBandwidth.Instance;
        public int bandwidthAmount => Props.bandwidthAmount;

        public CompBandwidthRelay connectedRelay;
        public bool DeadOrDowned => pawn.DeadOrDowned;
        public bool inCaravan => pawn.IsCaravanMember();
        public bool IsConnected => connectedRelay != null && connectedRelay.IsEnabled;
        public bool IsPlayerControlled => pawn.Faction == Find.FactionManager.OfPlayer;
        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);
            if (IsConnected)
            {
                connectedRelay.TryDisconnectConsumer(this);
            }
        }
        public override void CompTick()
        {
            base.CompTick();
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref connectedRelay, "relay");
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }

            if (IsPlayerControlled)
            {
                if (connectedRelay == null)
                {
                    Command_Action command_ConnectToRelay = new Command_Action();
                    command_ConnectToRelay.defaultLabel = "CFG_ConnectToRelay".Translate();
                    command_ConnectToRelay.defaultDesc = "CFG_ConnectToRelayDesc".Translate();
                    command_ConnectToRelay.icon = null;
                    command_ConnectToRelay.action = delegate
                    {
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        IEnumerable<CompBandwidthRelay> availableRelays = gridBandwidth.relaysInMap(parent.Map);
                        if (!availableRelays.Any())
                        {
                            list.Add(new FloatMenuOption("CFG_NoAvailableRelays".Translate(), null));
                        }
                        foreach (var relay in availableRelays)
                        {
                            list.Add(new FloatMenuOption("CFG_ConnectTo".Translate(relay.parent.LabelCap), delegate
                            {
                                relay.TryConnectConsumer(this);
                            }, (Thing)null, Color.white));
                        }
                        Find.WindowStack.Add(new FloatMenu(list));
                    };
                    yield return command_ConnectToRelay;
                }
                else
                {
                    Command_Action command_DisconnectFromRelay = new Command_Action();
                    command_DisconnectFromRelay.defaultLabel = "CFG_DisconnectFromRelay".Translate();
                    command_DisconnectFromRelay.defaultDesc = "CFG_DisconnectFromRelayDesc".Translate();
                    command_DisconnectFromRelay.icon = null;
                    command_DisconnectFromRelay.action = delegate
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                        connectedRelay.TryDisconnectConsumer(this);
                    };
                    yield return command_DisconnectFromRelay;
                }
                Command_Action test = new Command_Action();
                test.defaultLabel = "CFG_DisconnectFromRelay".Translate();
                test.defaultDesc = "CFG_DisconnectFromRelayDesc".Translate();
                test.icon = null;
                test.action = delegate
                {
                    Logger.Message($"{pawn.IsColonyMech}");

                    Logger.Message($"{pawn.drafter.ShowDraftGizmo}");

                };
                yield return test;
            }
        }
    }
}
