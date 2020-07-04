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

        public Vector2 TopLeft;
        public Vector2 TopRight;

        public Vector2 BottomLeft;
        public Vector2 BottomRight;

        public bool IsAnimated;
        
        public UVMap(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight, Color colorSide,
            Color colorTop, Color colorBottom, bool isAnimated = false)
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

            IsAnimated = isAnimated;
        }

        public void Rotate(int rot)
        {
            var topLeft = TopLeft;
            var topRight = TopRight;
            var bottomLeft = BottomLeft;
            var bottomRight = BottomRight;
				
            if (rot == 90)
            {
                TopLeft = topRight;
                TopRight = bottomRight;
                BottomLeft = topLeft;
                BottomRight = bottomLeft;
            }
            else if (rot == 180)
            {
                TopLeft = bottomRight;
                TopRight = bottomLeft;
                BottomLeft = topRight;
                BottomRight = topLeft;
            }
            else if (rot == 270)
            {
                // Y2 = X1
                // X1 = Y1;
               // TopLeft = new Vector2(topLeft.Y, topLeft.X);
              //  TopRight = new Vector2(topRight.X, topRight.X);
               // BottomLeft = new Vector2(topLeft., topRight.X);
                //TopLeft = bottomLeft;
               // TopRight = topLeft;
                //BottomLeft = bottomRight;
                //BottomRight = topRight;
            }
        }
    }
}
