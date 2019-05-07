using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace Alex.Utils
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    // ReSharper disable once InconsistentNaming
    public struct UVMap
    {
        public Color ColorLeft;
        public Color ColorRight;

        public Color ColorFront;
        public Color ColorBack;

        public Color ColorTop;
        public Color ColorBottom;

        public readonly Vector2 TopLeft;
        public readonly Vector2 TopRight;

        public readonly Vector2 BottomLeft;
        public readonly Vector2 BottomRight;

        public UVMap(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight, Color colorSide,
            Color colorTop, Color colorBottom)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;

	        ColorFront = colorSide;
	        ColorBack = colorSide;

	        ColorLeft = colorSide;
	        ColorRight = colorSide;

	        ColorTop = colorTop;
	        ColorBottom = colorBottom;
        }
    }
}
