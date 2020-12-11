using System;

namespace Alex.API
{
    public enum RenderStage : int
    {
        OpaqueFullCube = 0,
        Opaque = 1,
       // LiquidOld = 16,
        Transparent = 2,
        Translucent = 3,
        Animated = 4,
        //AnimatedTranslucent = 4096,
        Liquid = 5
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