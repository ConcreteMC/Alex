using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace Alex.Utils
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    // ReSharper disable once InconsistentNaming
    public struct UVMap
    {
        public readonly Color ColorLeft;
        public readonly Color ColorRight;

        public readonly Color ColorFront;
        public readonly Color ColorBack;

        public readonly Color ColorTop;
        public readonly Color ColorBottom;

        public readonly Vector2 TopLeft;
        public readonly Vector2 TopRight;

        public readonly Vector2 BottomLeft;
        public readonly Vector2 BottomRight;

        public int LightingTop;
        public int LightingBottom;
        public int LightingLeft;
        public int LightingRight;
        public int LightingFront;
        public int LightingBack;

        public UVMap(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight, Color colorSide,
            Color colorTop, Color colorBottom)
        {
            LightingTop = UvMapHelp.DefaultLighting[4];
            LightingBottom = UvMapHelp.DefaultLighting[5];

            LightingLeft = UvMapHelp.DefaultLighting[3];
            LightingRight = UvMapHelp.DefaultLighting[2];

            LightingFront = UvMapHelp.DefaultLighting[0];
            LightingBack = UvMapHelp.DefaultLighting[1];

            var lightTop = UvMapHelp.LightColor.ToVector3()*UvMapHelp.CubeBrightness[LightingTop];
            var lightBottom = UvMapHelp.LightColor.ToVector3() * UvMapHelp.CubeBrightness[LightingBottom];
            var lightLeft = UvMapHelp.LightColor.ToVector3() * UvMapHelp.CubeBrightness[LightingLeft];
            var lightRight = UvMapHelp.LightColor.ToVector3() * UvMapHelp.CubeBrightness[LightingRight];
            var lightFront = UvMapHelp.LightColor.ToVector3() * UvMapHelp.CubeBrightness[LightingFront];
            var lightBack = UvMapHelp.LightColor.ToVector3() * UvMapHelp.CubeBrightness[LightingBack];

            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;

            ColorFront = new Color(colorSide.ToVector3() * lightFront);
            ColorBack = new Color(colorSide.ToVector3() * lightBack);
            ColorLeft = new Color(colorSide.ToVector3() * lightLeft);
            ColorRight = new Color(colorSide.ToVector3() * lightRight);
            ColorTop = new Color(colorTop.ToVector3() * lightTop); ;
            ColorBottom = new Color(colorBottom.ToVector3() * lightBottom);
        }
    }

    internal class UvMapHelp
    {
        public static readonly Color LightColor =
          new Color(245, 245, 225);

        /// <summary>
        /// The default lighting information for rendering a block;
        ///  i.e. when the lighting param to CreateUniformCube == null.
        /// </summary>
        public static readonly int[] DefaultLighting =
            new int[]
            {
                15, 15, 15,
                15, 15, 15
            };

        /// <summary>
        /// Maps a light level [0..15] to a brightness modifier for lighting.
        /// </summary>
        public static readonly float[] CubeBrightness =
            new float[]
            {
                0.050f, 0.067f, 0.085f, 0.106f, // [ 0..3 ]
                0.129f, 0.156f, 0.186f, 0.221f, // [ 4..7 ]
                0.261f, 0.309f, 0.367f, 0.437f, // [ 8..11]
                0.525f, 0.638f, 0.789f, 1.000f //  [12..15]
            };

        /// <summary>
        /// The per-face brightness modifier for lighting.
        /// </summary>
        public static readonly float[] FaceBrightness =
            new float[]
            {
                0.6f, 0.6f, // North / South
                0.8f, 0.8f, // East / West
                1.0f, 0.5f // Top / Bottom
            };
    }
}
