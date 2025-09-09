using RimWorld;
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
        public bool IsSelfRelay => pawn.HasComp<CompBandwidthRelayPawn>();
        public CompProperties_BandwidthConsumer Props => (CompProperties_BandwidthConsumer)props;
        WorldComponent_GridBandwidth gridBandwidth => WorldComponent_GridBandwidth.Instance;
        public int BandwidthAmount => Props.bandwidthAmount;
        public Thing relay;
        public CompBandwidthRelay ConnectedRelayComp => relay.TryGetComp<CompBandwidthRelay>();
        public bool DeadOrDowned => pawn.DeadOrDowned;
        public bool InCaravan => pawn.IsCaravanMember();
        public bool IsConnected => ConnectedRelayComp != null && ConnectedRelayComp.IsEnabled && ConnectedRelayComp.AnyGridBandwidth;
        public bool IsPlayerControlled => pawn.Faction == Find.FactionManager.OfPlayer;
        private Hediff GlobalBottleneck => pawn.health.GetOrAddHediff(CrimsonGridFramework_DefOfs.CG_GlobalBottleneck);
        private Hediff RelayBottleneck => pawn.health.GetOrAddHediff(CrimsonGridFramework_DefOfs.CG_RelayBottleneck);

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (IsSelfRelay)
            {
                pawn.GetComp<CompBandwidthRelayPawn>().TryConnectConsumer(this);
                return;
            }
            if (respawningAfterLoad && relay != null)
            {
                ConnectedRelayComp.TryConnectConsumer(this);
            }
        }
        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);
            if (IsConnected)
            {
                ConnectedRelayComp.TryDisconnectConsumer(this);
            }
        }
        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);
            if (IsConnected || IsSelfRelay)
            {
                Log.Message(gridBandwidth.IsOverdraw);
                Log.Message(gridBandwidth.OverDrawPercentage);
                if (ConnectedRelayComp.IsOverdraw)
                {
                    RelayBottleneck.Severity = ConnectedRelayComp.OverDrawPercentage;
                }
                else
                {
                    RelayBottleneck.Severity = 0;
                }
                if (gridBandwidth.IsOverdraw && gridBandwidth.AnyBandwidth)
                {
                    GlobalBottleneck.Severity = gridBandwidth.OverDrawPercentage;
                }
                else
                {
                    GlobalBottleneck.Severity = 0;
                }

            }
        }
        public override void CompTick()
        {
            base.CompTick();
            if (!IsConnected && pawn.Drafted)
            {
                pawn.drafter.Drafted = false;
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref relay, "relay");
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }
            if (!IsSelfRelay)
            {
                if (ConnectedRelayComp != null)
                {
                    Command_Action command_Action = new Command_Action();
                    command_Action.defaultLabel = "Select Parent Relay";
                    command_Action.defaultDesc = "A";
                    command_Action.icon = null;
                    command_Action.Order = -86f;
                    command_Action.action = delegate
                    {
                        Find.Selector.ClearSelection();
                        if (ConnectedRelayComp is CompBandwidthRelayBuilding)
                        {
                            Find.Selector.Select(ConnectedRelayComp.parent);
                            return;
                        }
                        if (ConnectedRelayComp is CompBandwidthRelayEquipment e)
                        {
                            Find.Selector.Select(e.Pawn);
                            return;
                        }
                        if (ConnectedRelayComp is CompBandwidthRelayPawn p)
                        {
                            Find.Selector.Select(p.Pawn);
                            return;
                        }
                    };
                    yield return command_Action;
                }
                if (IsPlayerControlled)
                {
                    if (ConnectedRelayComp == null)
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
                            ConnectedRelayComp.TryDisconnectConsumer(this);
                        };
                        yield return command_DisconnectFromRelay;
                    }
                }
            }

            Command_Action test = new Command_Action();
            test.defaultLabel = "Log Some Things";
            test.defaultDesc = null;
            test.icon = null;
            test.action = delegate
            {
                Logger.Message($"{pawn.IsColonyMech}");

                Logger.Message($"{pawn.drafter.ShowDraftGizmo}");

                Logger.Message($"{pawn.lord == null}");
                Logger.Message($"Rah {pawn.RaceProps.IsMechanoid}");

            };
            yield return test;
        }
    }
}
