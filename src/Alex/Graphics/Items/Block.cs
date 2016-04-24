using Alex.Graphics.Models;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Items
{
    public class Block
    {
        public ushort BlockId { get; private set; }
        public byte Metadata { get; private set; }
        public bool Solid { get; protected set; }
		public bool Transparent { get; protected set; }
        public Model BlockModel { get; protected set; }

        public Block(ushort blockId, byte metadata)
        {
            BlockId = blockId;
            Metadata = metadata;

            TopColor = Color.White;
            BottomColor = Color.White;
            SideColor = Color.White;

            SetTexture(TextureSide.All, "dirt");
            BlockModel = new Cube();

            Solid = true;
	        Transparent = false;
        }

	    public BoundingBox GetBoundingBox(Vector3 blockPosition)
	    {
		    return new BoundingBox(blockPosition + BlockModel.Offset,
					blockPosition + BlockModel.Offset + BlockModel.Size);
		}

        public BoundingBox BoundingBox
        {
            get
            {
                return new BoundingBox(Vector3.Zero,
                    Vector3.Zero + BlockModel.Size);
            }
        }

        public VertexPositionNormalTextureColor[] GetVertices(Vector3 position)
        {
            return BlockModel.GetShape(position + BlockModel.Offset, this);
        }

        public void SetTexture(TextureSide side, string textureName)
        {
            Vector2 textureLocation = ResManager.GetAtlasLocation(textureName);
            switch (side)
            {
                case TextureSide.Top:
                    TopTexture = textureLocation;
                    break;
                case TextureSide.Bottom:
                    BottomTexture = textureLocation;
                    break;
                case TextureSide.Side:
                    SideTexture = textureLocation;
                    break;
                case TextureSide.All:
                    TopTexture = textureLocation;
                    BottomTexture = textureLocation;
                    SideTexture = textureLocation;
                    break;
            }
        }

        public void SetColor(TextureSide side, Color color)
        {
            switch (side)
            {
                case TextureSide.Top:
                    TopColor = color;
                    break;
                case TextureSide.Bottom:
                    BottomColor = color;
                    break;
                case TextureSide.Side:
                    SideColor = color;
                    break;
                case TextureSide.All:
                    TopColor = color;
                    BottomColor = color;
                    SideColor = color;
                    break;
            }
        }

        // ReSharper disable once InconsistentNaming
        public virtual UVMap CreateUVMapping(TextureSide dir)
        {
            var tileSizeX = 1/ResManager.InWidth; //0.0625
            var tileSizeY = 1/ResManager.InHeigth;

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

            var x = tile.X/uvSize.X; //0.9375
            var y = tile.Y/uvSize.Y; //0.0

            var textureTopLeft = new Vector2(x, y);
            var textureTopRight = new Vector2(x + tileSizeX, y);
            var textureBottomLeft = new Vector2(x, y + tileSizeY);
            var textureBottomRight = new Vector2(x + tileSizeX, y + tileSizeY);

            return new UVMap(textureTopLeft, textureTopRight, textureBottomLeft, textureBottomRight, SideColor, TopColor, BottomColor);
        }

        public Vector2 TopTexture { get; protected set; }
        public Vector2 SideTexture { get; protected set; }
        public Vector2 BottomTexture { get; protected set; }

        public Color TopColor { get; protected set; }
        public Color SideColor { get; protected set; }
        public Color BottomColor { get; protected set; }

        #region Old code

        /*var path = string.Format("assets\\minecraft\\textures\\blocks\\{0}.png", textureName);
            using (var stream = TitleContainer.OpenStream(path))
            {
                var texture = Texture2D.FromStream(Game.Instance.GraphicsDevice, stream);
                switch (side)
                {
                    case Utils.TextureSide.Top:
                        TextureTop = texture;
                        break;
                    case Utils.TextureSide.Side:
                        TextureSide = texture;
                        break;
                    case Utils.TextureSide.Bottom:
                        TextureBottom = texture;
                        break;
                    case Utils.TextureSide.All:
                        TextureTop = texture;
                        TextureSide = texture;
                        TextureBottom = texture;
                        break;
                }
            }*/

        #endregion
    }
}