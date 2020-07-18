using System;

namespace Alex.API
{
    [Flags]
    public enum RenderStage
    {
        OpaqueFullCube = 1,
        Opaque = 4,
       // LiquidOld = 16,
        Transparent = 64,
        Translucent = 256,
        Animated = 1024,
        AnimatedTranslucent = 4096,
        Liquid = 8192
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