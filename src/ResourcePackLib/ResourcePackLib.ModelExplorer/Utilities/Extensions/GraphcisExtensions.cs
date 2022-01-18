using Microsoft.Xna.Framework.Graphics;

namespace ResourcePackLib.ModelExplorer.Utilities.Extensions;

public static class GraphcisExtensions
{
    
    public static RasterizerState Copy(this RasterizerState state)
    {
        return new RasterizerState
        {
            CullMode = state.CullMode,
            DepthBias = state.DepthBias,
            FillMode = state.FillMode,
            MultiSampleAntiAlias = state.MultiSampleAntiAlias,
            Name = state.Name,
            ScissorTestEnable = state.ScissorTestEnable,
            SlopeScaleDepthBias = state.SlopeScaleDepthBias,
            Tag = state.Tag,
        };
    }
}