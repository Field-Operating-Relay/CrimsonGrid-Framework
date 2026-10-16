using UnityEngine;
using Verse;

namespace CrimsonGridFramework
{
    public class PawnRenderNode_HediffAttachment : PawnRenderNode
    {
        private Graphic cachedGraphic;

        public PawnRenderNode_HediffAttachment(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            if (cachedGraphic == null && !Props.texPath.NullOrEmpty())
            {
                cachedGraphic = GraphicDatabase.Get<Graphic_Multi>(
                    Props.texPath,
                    Props.shaderTypeDef.Shader,
                    Props.drawSize,
                    Color.white
                );
            }
            return cachedGraphic;
        }

        public override GraphicMeshSet MeshSetFor(Pawn pawn)
        {
            Graphic graphic = GraphicFor(pawn);
            if (graphic != null)
            {
                return MeshPool.GetMeshSetForSize(graphic.drawSize.x, graphic.drawSize.y);
            }
            return null;
        }
    }
}