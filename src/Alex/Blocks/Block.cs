using Alex.Graphics.Models;
using Alex.Rendering;
using Alex.Utils;
using log4net;
using Microsoft.Xna.Framework;
using ResourcePackLib.Json.BlockStates;

namespace Alex.Blocks
{
    public class Block
    {
	    private static readonly ILog Log = LogManager.GetLogger(typeof(Block));
	    
		public uint BlockStateID { get; }

        public int BlockId { get; }
        public byte Metadata { get; }
        public bool Solid { get; set; }
		public bool Transparent { get; set; }
		public bool Renderable { get; set; }
		public bool HasHitbox { get; set; }
		public float Drag { get; set; }

	    public double AmbientOcclusionLightValue = 1.0;
	    public int LightValue = 0;
	    public int LightOpacity = 0;

		public Model BlockModel { get; set; }
	    protected Block(byte blockId, byte metadata) : this(GetBlockStateID(blockId, metadata))
	    {
		    
	    }

	    public Block(uint blockStateId)
	    {
		    BlockStateID = blockStateId;

		    Solid = true;
		    Transparent = false;
		    Renderable = true;
		    HasHitbox = true;

		    SetColor(TextureSide.All, Color.White);
			SetTexture(TextureSide.All, "no_texture");

			BlockId = (int)(blockStateId >> 4);
		    Metadata = (byte)(blockStateId & 0x0F);
		}

	    public BoundingBox GetBoundingBox(Vector3 blockPosition)
	    {
			if (BlockModel == null)
				return new BoundingBox(blockPosition, blockPosition + Vector3.One);
		    return BlockModel.GetBoundingBox(blockPosition, this);
		}

        public VertexPositionNormalTextureColor[] GetVertices(Vector3 position, World world)
        {
            return BlockModel.GetShape(world, position + BlockModel.Offset, this);
        }

        public void SetTexture(TextureSide side, string textureName)
        {
            switch (side)
            {
                case TextureSide.Top:
                    TopTexture = textureName;
                    break;
                case TextureSide.Bottom:
                    BottomTexture = textureName;
                    break;
                case TextureSide.Side:
                    SideTexture = textureName;
                    break;
                case TextureSide.All:
                    TopTexture = textureName;
                    BottomTexture = textureName;
                    SideTexture = textureName;
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

        public string TopTexture { get; private set; }
        public string SideTexture { get; private set; }
        public string BottomTexture { get; private set; }

        public Color TopColor { get; set; }
        public Color SideColor { get; set; }
		public Color BottomColor { get; set; }

	    public string DisplayName { get; set; } = null;
	    public override string ToString()
	    {
		    return DisplayName ?? GetType().Name;
	    }

	    public static uint GetBlockStateID(int id, byte meta)
	    {
		   return (uint)(id << 4 | meta);
		}
	}
}