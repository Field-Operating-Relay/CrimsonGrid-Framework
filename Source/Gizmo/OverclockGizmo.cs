using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static Mono.Security.X509.X520;

namespace CrimsonGridFramework
{
    [StaticConstructorOnStartup]
    public class OverclockGizmo : Gizmo
    {

        private CompBandwidthOverclock tracker;

        private static bool draggingBar;

        public const float CostPreviewFadeIn = 0.1f;

        public const float CostPreviewSolid = 0.15f;

        public const float CostPreviewFadeInSolid = 0.25f;

        public const float CostPreviewFadeOut = 0.6f;

        private static readonly Texture2D OverclockBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.34f, 0.42f, 0.43f));

        private static readonly Texture2D OverclockBarHighlightTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.43f, 0.54f, 0.55f));

        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));

        private static readonly Texture2D OverclockTargetTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.74f, 0.97f, 0.8f));
        private float stagePercentage => (float)tracker.Overclock / (float)(tracker.overclockStages - 1);
        private float percentagePerStage;
        private List<float> stagePercentages = [];

        public OverclockGizmo(CompBandwidthOverclock tracker)
        {
            this.tracker = tracker;
            Order = -100f;
            percentagePerStage = 1f / (float)(tracker.overclockStages - 1);
            for(int i = 0; i < tracker.overclockStages; i++)
            {
                stagePercentages.Add(i * percentagePerStage);
            }
        }
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect rect2 = rect.ContractedBy(6f);
            float num = Mathf.Repeat(Time.time, 0.85f);
            float num2 = 1f;
            if (num < 0.1f)
            {
                num2 = num / 0.1f;
            }
            else if (num >= 0.25f)
            {
                num2 = 1f - (num - 0.25f) / 0.6f;
            }
            Widgets.DrawWindowBackground(rect);
            Text.Font = GameFont.Small;
            Rect headerRect = rect2;
            headerRect.height = Text.LineHeight;
            string title = "CGF_OverclockStage".Translate();
            title = title.Truncate(headerRect.width);
            Widgets.Label(headerRect, title);
            Rect barRect = rect2;
            barRect.yMin = headerRect.yMax + 8f;
            Widgets.DraggableBar(barRect, OverclockBarTex, OverclockBarHighlightTex, EmptyBarTex, OverclockTargetTex, ref draggingBar, stagePercentage, ref tracker.TargetStage, stagePercentages, (int)(percentagePerStage * 10));
            return new GizmoResult(GizmoState.Clear);
        }

        public override float GetWidth(float maxWidth)
        {
            return 212f;
        }
    }
}
