using RimWorld;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;

namespace CrimsonGridFramework
{
    [StaticConstructorOnStartup]
    public class Command_APSToggle : Command
    {
        public HediffComp_APS comp;
        public Action toggleAction;

        private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

        public override string TopRightLabel => comp.LabelRemaining;

        public Command_APSToggle()
        {
            Order = 5f;
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            toggleAction?.Invoke();
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);

            disabled = comp.Disabled;

            GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);

            if (comp.CooldownTicksRemaining > 0 && comp.Enabled)
            {
                float fillPercent = 1f - Mathf.InverseLerp(0f, comp.Props.cooldownTicks, comp.CooldownTicksRemaining);
                Widgets.FillableBar(rect, Mathf.Clamp01(fillPercent), cooldownBarTex, null, doBorder: false);

                Text.Font = GameFont.Tiny;
                string cooldownText = "CGF_APS_CooldownSeconds".Translate(comp.CooldownTicksRemaining / 60);
                Vector2 textSize = Text.CalcSize(cooldownText);
                textSize.x += 2f;

                Rect textRect = rect;
                textRect.x = rect.x + rect.width / 2f - textSize.x / 2f;
                textRect.width = textSize.x;
                textRect.height = textSize.y;
                textRect.y = rect.y + rect.height - textSize.y - 2f;

                Rect bgRect = textRect.ExpandedBy(8f, 0f);
                Text.Anchor = TextAnchor.LowerCenter;
                GUI.DrawTexture(bgRect, TexUI.GrayTextBG);
                Widgets.Label(textRect, cooldownText);
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
            }

            if (!comp.Enabled)
            {
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Rect offRect = new Rect(rect.x, rect.y + rect.height / 2f - 10f, rect.width, 20f);
                GUI.color = new Color(1f, 0.3f, 0.3f, 0.8f);
                Widgets.Label(offRect, "CGF_APS_OffLabel".Translate());
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }

            return result;
        }

        protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
        {
            if (Mouse.IsOver(butRect))
            {
                defaultDesc = GetTooltip();
            }

            return base.GizmoOnGUIInt(butRect, parms);
        }

        //private string GetTooltip()
        //{
        //    string tooltip = defaultLabel + "\n\n";

        //    if (!comp.Enabled)
        //    {
        //        tooltip += "<color=#FF6666>" + "CGF_APS_TooltipSystemDisabled".Translate().ToString() + "</color>\n";
        //        tooltip += "CGF_APS_TooltipClickEnable".Translate().ToString() + "\n\n";
        //    }
        //    else
        //    {
        //        tooltip += "<color=#66FF66>" + "CGF_APS_TooltipSystemActive".Translate().ToString() + "</color>\n";
        //        tooltip += "CGF_APS_TooltipClickDisable".Translate().ToString() + "\n\n";
        //    }

        //    tooltip += "<b>" + "CGF_APS_TooltipCharges".Translate().ToString() + "</b> " + $"{comp.RemainingCharges} / {comp.MaxCharges}\n";

        //    if (comp.CooldownTicksRemaining > 0)
        //    {
        //        tooltip += "<b>" + "CGF_APS_TooltipCooldown".Translate().ToString() + "</b> " + $"{comp.CooldownTicksRemaining.ToStringTicksToPeriod()}\n";
        //    }
        //    else if (comp.RemainingCharges <= 0)
        //    {
        //        tooltip += "<color=#FFAA00>" + "CGF_APS_TooltipOutOfCharges".Translate().ToString() + "</color>\n";
        //    }
        //    else
        //    {
        //        tooltip += "<color=#66FF66>" + "CGF_APS_TooltipReady".Translate().ToString() + "</color>\n";
        //    }

        //    if (comp.Props.ammoDef != null)
        //    {
        //        tooltip += "\n<b>" + "CGF_APS_TooltipAmmo".Translate().ToString() + "</b> " + $"{comp.Props.ammoDef.LabelCap}";
        //        tooltip += "\n<b>" + "CGF_APS_TooltipPerCharge".Translate().ToString() + "</b> " + $"{comp.Props.ammoCountPerCharge}";
        //    }

        //    tooltip += "\n\n<b>" + "CGF_APS_TooltipInterceptRadius".Translate().ToString() + "</b> " +
        //               "CGF_APS_TooltipTiles".Translate(comp.Props.interceptRadius.ToString("F1"));

        //    return tooltip;
        //}

        private string GetTooltip()
        {
            string tooltip = defaultLabel + "\n\n";

            if (!comp.Enabled)
            {
                tooltip += $"<color=#FF6666>{"CGF_APS_TooltipSystemDisabled".Translate()}</color>\n";
                tooltip += $"{"CGF_APS_TooltipClickEnable".Translate()}\n\n";
            }
            else
            {
                tooltip += $"<color=#66FF66>{"CGF_APS_TooltipSystemActive".Translate()}</color>\n";
                tooltip += $"{"CGF_APS_TooltipClickDisable".Translate()}\n\n";
            }

            tooltip += $"<b>{"CGF_APS_TooltipCharges".Translate()}</b> {comp.RemainingCharges} / {comp.MaxCharges}\n";

            if (comp.CooldownTicksRemaining > 0)
            {
                tooltip += $"<b>{"CGF_APS_TooltipCooldown".Translate()}</b> {comp.CooldownTicksRemaining.ToStringTicksToPeriod()}\n";
            }
            else if (comp.RemainingCharges <= 0)
            {
                tooltip += $"<color=#FFAA00>{"Out of charges - needs reload".Translate()}</color>\n";
            }
            else
            {
                tooltip += $"<color=#66FF66>{"CGF_APS_TooltipReady".Translate()}</color>\n";
            }

            if (comp.Props.ammoDef != null)
            {
                tooltip += $"\n<b>{"CGF_APS_TooltipAmmo".Translate()}</b> {comp.Props.ammoDef.LabelCap}";
                tooltip += $"\n<b>{"CGF_APS_TooltipPerCharge".Translate()}</b> {comp.Props.ammoCountPerCharge}";
            }

            tooltip += $"\n\n<b>{"CGF_APS_TooltipInterceptRadius".Translate()}</b> {comp.Props.interceptRadius:F1} tiles";

            return tooltip;
        }

        public override bool InheritInteractionsFrom(Gizmo other)
        {
            return false;
        }
    }
}