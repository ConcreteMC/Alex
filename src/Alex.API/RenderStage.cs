using System;

namespace Alex.API
{
    [Flags]
    public enum RenderStage
    {
        OpaqueFullCube = 1,
     //   LitOpaqueFullCube = 2,
        Opaque = 4,
     //   LitOpaque = 8,
        Liquid = 16,
     //   LitLiquid = 32,
        Transparent = 64,
      //  LitTransparent = 128,
        Translucent = 256,
      //  LitTranslucent = 512,
        Animated = 1024,
   //     LitAnimated = 2048,
        AnimatedTranslucent = 4096,
      //  LitAnimatedTranslucent = 8192
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