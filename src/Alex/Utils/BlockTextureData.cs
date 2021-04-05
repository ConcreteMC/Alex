using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace Alex.Utils
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    // ReSharper disable once InconsistentNaming
    public struct BlockTextureData
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

        public TextureInfo TextureInfo;
        public BlockTextureData(TextureInfo textureInfo, Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight, Color colorSide,
            Color colorTop, Color colorBottom, bool isAnimated = false)
        {
            TextureInfo = textureInfo;
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
            if (rot > 0)
            {
               /* var tw = TextureInfo.Width;
                var th = TextureInfo.Height;

                var x1 = TopLeft.X;
                var x2 = TopRight.X;
                var y1 = TopLeft.Y;
                var y2 = BottomLeft.Y;
            
                x1 = (x1 * (tw));
                x2 = (x2 * (tw ));
                y1 = (y1 * (th));
                y2 = (y2 * (th));
                
                var ox1 = x1;
                var ox2 = x2;
                var oy1 = y1;
                var oy2 = y2;
                switch (rot)
                {
                    case 270:
                        y1 = tw * 16 - ox2;
                        y2 = tw * 16 - ox1;
                        x1 = oy1;
                        x2 = oy2;
                        break;
                    case 180:
                        y1 = th * 16 - oy2;
                        y2 = th * 16 - oy1;
                        x1 = tw * 16 - ox2;
                        x2 = tw * 16 - ox1;
                        break;
                    case 90:
                        y1 = ox1;
                        y2 = ox2;
                        x1 = th * 16 - oy2;
                        x2 = th * 16 - oy1;
                        break;
                }
                
                TopLeft = new Microsoft.Xna.Framework.Vector2(x1, y1);
                TopRight = new Microsoft.Xna.Framework.Vector2(x2, y1);
                BottomLeft = new Microsoft.Xna.Framework.Vector2(x1, y2);
                BottomRight = new Microsoft.Xna.Framework.Vector2(x2, y2);*/
            }
        }
    }
}
