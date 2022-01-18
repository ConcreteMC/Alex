using Microsoft.Xna.Framework;

namespace ResourcePackLib.ModelExplorer.Utilities.Extensions;

public static class MathExtensions
{
    public static float ToRadians(this float degrees)
    {
        return MathHelper.ToRadians(degrees);
    }
        
    public static float ToDegrees(this float radians)
    {
        return MathHelper.ToDegrees(radians);
    }
}