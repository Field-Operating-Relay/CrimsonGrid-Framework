using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CrimsonGridFramework
{
    [StaticConstructorOnStartup]
    public class Dialog_NetworkManager : Window
    {
        private Vector2 scrollPositionLeft;
        private Vector2 scrollPositionMiddle;
        private Vector2 scrollPositionRight;

        private const float ColumnMargins = 8f;
        private const float EntryRowHeight = 70f;

        protected float OffsetHeaderY = 24f;

        List<CompBandwidthProvider> providers => WorldComponent_GridBandwidth.Instance.bandwidthProviders.ToList();
        CompBandwidthRelay selectedRelay = null;
        string ProviderFilter = "";
        string RelayFilter = "";
        string ConsumerFilter = "";
        Map map;
        List<CompBandwidthRelay> relays = [];
        List<CompBandwidthConsumer> consumers = [];
        public override Vector2 InitialSize => new Vector2(Mathf.Min(Screen.width - 50, 1300), 720f);
        public override void PostClose()
        {
            base.PostClose();
        }
        public Dialog_NetworkManager()
        {
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
            map = Find.CurrentMap;
            relays = WorldComponent_GridBandwidth.Instance.relaysInMap(map).ToList();
            FillConsumers();
            optionalTitle = "CGF_NetworkManager_Title".Translate();
        }
        public override void PostOpen()
        {
            base.PostOpen();
        }
        public override void DoWindowContents(Rect inRect)
        {

            float width = (inRect.width / 3) - (ColumnMargins * 0.75f);
            Rect providerLabelRect = inRect;
            providerLabelRect.height = OffsetHeaderY;
            providerLabelRect.width = width * 0.8f;
            // Left window (providers)
            Widgets.Label(providerLabelRect, "CGF_NetworkManager_Providers".Translate());
            Rect providerSearchRect = new Rect(ColumnMargins, providerLabelRect.yMax, width * 0.78f, EntryRowHeight / 2);
            ProviderFilter = Widgets.TextArea(providerSearchRect, ProviderFilter);
            Regex rgx = new Regex(ProviderFilter, RegexOptions.IgnoreCase);
            List<CompBandwidthProvider> providersFiltered = providers.Where(c => rgx.IsMatch(c.parent.Label)).ToList();
            Rect rect3 = inRect;
            rect3.yMin = providerSearchRect.yMax;
            rect3.width = width * 0.8f;
            rect3.xMin = ColumnMargins;
            Widgets.DrawMenuSection(rect3);
            rect3 = rect3.ContractedBy(1f);
            float height = providersFiltered.Count * EntryRowHeight;
            if (providersFiltered.Count == 0)
            {
                using (new TextBlock(GameFont.Small))
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(rect3, "CGF_NetworkManager_NoProviders".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                }
            }
            else
            {
                Rect viewRect = new Rect(0f, 0f, rect3.width, height);
                Widgets.AdjustRectsForScrollView(inRect, ref rect3, ref viewRect);
                providerSearchRect.width = rect3.width;
                Widgets.BeginScrollView(rect3, ref scrollPositionLeft, viewRect);
                for (int i = 0; i < providersFiltered.Count; i++)
                {
                    Rect rect4 = new Rect(0f, (float)i * EntryRowHeight, viewRect.width, EntryRowHeight);
                    DoEntryProviderRow(rect4, providersFiltered[i], i);
                }
                Widgets.EndScrollView();
            }
            // Middle window (Relays)
            Rect relayLabelRect = inRect;
            relayLabelRect.xMin = providerLabelRect.xMax + ColumnMargins;
            relayLabelRect.height = OffsetHeaderY;
            relayLabelRect.width = width * 0.8f;
            Widgets.Label(relayLabelRect, "CGF_NetworkManager_Relays".Translate());
            Rect relaySearchRect = new Rect(rect3.xMax + ColumnMargins, relayLabelRect.yMax, width * 0.8f, EntryRowHeight / 2);
            Rect rect5 = inRect;
            rect5.yMin = relaySearchRect.yMax;
            rect5.xMin = rect3.xMax + ColumnMargins;
            rect5.width = width * 0.8f;
            Widgets.DrawMenuSection(rect5);
            rect5 = rect5.ContractedBy(1f);
            RelayFilter = Widgets.TextArea(relaySearchRect, RelayFilter);
            Regex rgx1 = new Regex(RelayFilter, RegexOptions.IgnoreCase);
            List<CompBandwidthRelay> relaysFiltered = relays.Where(c => rgx1.IsMatch(c.parent.Label)).ToList();
            if (relaysFiltered.Count == 0)
            {
                using (new TextBlock(GameFont.Small))
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(rect5, "CGF_NetworkManager_NoRelays".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                }
            }
            else
            {
                float height2 = relays.Count * EntryRowHeight;
                Rect viewRect1 = new Rect(0f, 0f, rect5.width, height2);
                Widgets.AdjustRectsForScrollView(inRect, ref rect5, ref viewRect1);
                Widgets.BeginScrollView(rect5, ref scrollPositionMiddle, viewRect1);
                for (int i = 0; i < relaysFiltered.Count; i++)
                {
                    Rect rect6 = new Rect(0f, (float)i * EntryRowHeight, viewRect1.width, EntryRowHeight);
                    DoEntryRelayRow(rect6, relaysFiltered[i], i);
                }
                Widgets.EndScrollView();
            }

            //Right window (consumers)
            Rect consumerLabelRect = inRect;
            consumerLabelRect.xMin = relayLabelRect.xMax + ColumnMargins;
            consumerLabelRect.height = OffsetHeaderY;
            consumerLabelRect.width = width * 0.8f;
            Widgets.Label(consumerLabelRect, "CGF_NetworkManager_Consumers".Translate());
            Rect consumerSearchRect = new Rect(relaySearchRect.xMax + ColumnMargins, consumerLabelRect.yMax, width * 0.8f, EntryRowHeight / 2);
            ConsumerFilter = Widgets.TextArea(consumerSearchRect, ConsumerFilter);
            Regex rgx2 = new Regex(ConsumerFilter, RegexOptions.IgnoreCase);
            List<CompBandwidthConsumer> consumersFiltered = consumers.Where(c => rgx2.IsMatch(c.parent.Label)).ToList();
            Rect consumerRectangle = inRect;
            consumerRectangle.yMin = consumerSearchRect.yMax;
            consumerRectangle.xMin = rect5.xMax + ColumnMargins;
            consumerRectangle.width = width * 1.4f;
            consumerRectangle.yMax = rect5.yMax * 0.95f;
            Widgets.DrawMenuSection(consumerRectangle);
            consumerRectangle = consumerRectangle.ContractedBy(1f);
            float height1 = relays.Count * EntryRowHeight;
            if (consumersFiltered.Count == 0)
            {
                using (new TextBlock(GameFont.Small))
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(consumerRectangle, "CGF_NetworkManager_NoConsumers".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                }
            }
            else
            {

                float height3 = consumersFiltered.Count * EntryRowHeight;
                Rect viewRect2 = new Rect(0f, 0f, consumerRectangle.width, height3);
                Widgets.AdjustRectsForScrollView(inRect, ref consumerRectangle, ref viewRect2);
                Widgets.BeginScrollView(consumerRectangle, ref scrollPositionRight, viewRect2);
                for (int i = 0; i < consumersFiltered.Count; i++)
                {
                    Rect rect8 = new Rect(0f, (float)i * EntryRowHeight, viewRect2.width, EntryRowHeight);
                    DoEntryConsumerRow(rect8, consumersFiltered[i], i);
                }
                Widgets.EndScrollView();
            }
            //Bottom right Window (display local bandwidth)
            Rect rect9 = inRect;
            rect9.yMin = consumerRectangle.yMax + ColumnMargins;
            rect9.xMin = rect5.xMax + ColumnMargins;
            rect9.width = width * 1.4f;
            Widgets.DrawMenuSection(rect9);
            rect9 = rect9.ContractedBy(1f);
            using (new TextBlock(GameFont.Small))
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                Rect rect10 = rect9;
                rect10.xMin += 15f;
                rect10.width -= 15f;
                Widgets.Label(rect10, TranslatorFormattedStringExtensions.Translate("CGF_NetworkManager_LocalBandwidth", selectedRelay == null ? [0,0] :[selectedRelay.RelayBandwidthInUse, selectedRelay.RelayBandwidthAmount]));
            }
        }
        private void DoEntryRelayRow(Rect rect, CompBandwidthRelay relay, int index)
        {
            Text.Anchor = TextAnchor.MiddleLeft;
            float x = rect.x;
            float num = (rect.height - 64) / 2f;
            using (new TextBlock(GameFont.Small))
            {
                Rect rowRect = new Rect(x + 5f, rect.y + num, rect.width, 64);
                DrawIconWithLabel(rowRect, relay.parent.LabelCap, Widgets.GetIconFor(relay.parent.def));
                if(relay == selectedRelay)
                {
                    Widgets.DrawHighlightSelected(rect);
                }
                else if (index % 2 == 1)
                {
                    Widgets.DrawLightHighlight(rect);
                }
                if (Mouse.IsOver(rect))
                {
                    Widgets.DrawHighlight(rect);
                }
                if (Widgets.ButtonInvisible(rect))
                {
                    if(selectedRelay == relay)
                    {
                        selectedRelay = null;
                    }
                    else
                    {
                        selectedRelay = relay;
                    }
                    FillConsumers();
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
        }
        private void DoEntryConsumerRow(Rect rect, CompBandwidthConsumer consumer, int index)
        {
            Text.Anchor = TextAnchor.MiddleLeft;
            float x = rect.x;
            float num = (rect.height - 64) / 2f;
            using (new TextBlock(GameFont.Small))
            {
                Rect rowRect = new Rect(x + 5f, rect.y + num, rect.width, 64);
                DrawIconWithLabel(rowRect, consumer.parent.LabelCap, Widgets.GetIconFor(consumer.parent.def));
                if (Mouse.IsOver(rect))
                {
                    Widgets.DrawHighlight(rect);
                }
                else if (index % 2 == 1)
                {
                    Widgets.DrawLightHighlight(rect);
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
        }
        private void DoEntryProviderRow(Rect rect, CompBandwidthProvider provider, int index)
        {

            Text.Anchor = TextAnchor.MiddleLeft;
            float x = rect.x;
            float num = (rect.height - 64) / 2f;
            using (new TextBlock(GameFont.Small))
            {
                Rect rowRect = new Rect(x + 5f, rect.y + num, rect.width, 64);
                DrawIconWithLabel(rowRect, provider.parent.LabelCap, Widgets.GetIconFor(provider.parent.def));
                if (Mouse.IsOver(rect))
                {
                    Widgets.DrawHighlight(rect);
                }
                else if (index % 2 == 1)
                {
                    Widgets.DrawLightHighlight(rect);
                }

            }
            Text.Anchor = TextAnchor.UpperLeft;
        }
        private void FillConsumers()
        {
            consumers = [];
            if (selectedRelay != null)
            {
                consumers = selectedRelay.consumers.ToList();
            }
            else
            {
                foreach (var item in relays)
                {
                    consumers.AddRange(item.consumers.ToList());
                }
            }
        }
        private void DrawIconWithLabel(Rect rect, string label, Texture2D icon)
        {
            Rect iconRect = rect;
            iconRect.width = rect.height;
            Rect labelRect = rect;
            labelRect.xMin = iconRect.xMax + 15;
            labelRect.width = rect.width - rect.height;
            Widgets.DrawTextureFitted(iconRect, icon, 1f);
            Widgets.Label(labelRect, label);
        }
    }
}
