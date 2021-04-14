using System;

namespace Alex.API
{
    public enum RenderStage : int
    {
        Opaque = 0,
       // LiquidOld = 16,
        Transparent = 1,
        Translucent = 2,
        Animated = 3,
        //AnimatedTranslucent = 4096,
        Liquid = 4
    }

   /* public static class RenderStageExtensions
    {
        private static RenderStage _litStages = RenderStage.LitOpaqueFullCube | RenderStage.LitOpaque | RenderStage.LitLiquid | RenderStage.LitTransparent
                                               | RenderStage.LitTranslucent | RenderStage.LitAnimated | RenderStage.LitAnimatedTranslucent;
        public static bool IsLitStage(this RenderStage stage)
        {
            return (_litStages & stage) == stage;
        }
    }*/
}