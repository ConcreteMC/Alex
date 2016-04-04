using Alex.Graphics.Items;
using Alex.Graphics.Models;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Blocks
{
    public class Torch : Block
    {
        public Torch() : base(50, 0)
        {
            SetTexture(TextureSide.All, "torch_on");
            BlockModel = new TorchModel();
	        Solid = false;
        }

        public override UVMap CreateUVMapping(TextureSide dir)
        {
            var tileSizeX = 1 / 16.0f;
            var tileSizeY = 1 / 22.0f;

            var uvSize = ResManager.AtlasSize;
            var tile = Vector2.Zero;

            switch (dir)
            {
                case TextureSide.Top:
                    tile = TopTexture;
                    break;
                case TextureSide.Bottom:
                    tile = BottomTexture;
                    break;
                case TextureSide.Side:
                    tile = SideTexture;
                    break;
            }

            var x = tile.X / uvSize.X; //0.9375
            var y = tile.Y / uvSize.Y; //0.0

            var textureTopLeft = new Vector2(x + (tileSizeX / 2) - 0.00390625f, y + (0.01704545454f));
            var textureTopRight = new Vector2(x + (tileSizeX / 2) + 0.00390625f, y + (0.01704545454f));
            var textureBottomLeft = new Vector2(x + (tileSizeX / 2) - 0.00390625f, y + tileSizeY);
            var textureBottomRight = new Vector2(x + (tileSizeX / 2) + 0.00390625f, y + tileSizeY);

            if (dir == TextureSide.Top)
            {
                textureTopLeft = new Vector2(x + (tileSizeX / 2) - 0.00390625f, y + (0.01988636363f));

                textureTopRight = textureTopLeft + new Vector2(0.00390625f, 0);
                textureBottomRight = textureTopLeft + new Vector2(0.00390625f, 0);
                textureBottomLeft = textureTopLeft;
            }

            return new UVMap(
                textureTopLeft,
                textureTopRight,
                textureBottomLeft,
                textureBottomRight,
                SideColor,
                TopColor,
                BottomColor
                );
        }
    }
}
