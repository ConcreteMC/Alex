using Alex.Graphics.Items;
using Alex.Graphics.Models;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Blocks
{
    public class Slab : Block
    {
        public Slab(byte metadata) : base(43, metadata)
        {
            BlockModel = new SlabModel();
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

            var textureTopLeft = new Vector2(x, y);
            var textureTopRight = new Vector2(x + tileSizeX, y);
            if (dir == TextureSide.Side)
            {
                textureTopLeft = new Vector2(x, y + (tileSizeY/2));
                textureTopRight = new Vector2(x + tileSizeX, y + (tileSizeY/2));
            }
            var textureBottomLeft = new Vector2(x, y + tileSizeY);
            var textureBottomRight = new Vector2(x + tileSizeX, y + tileSizeY);

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
