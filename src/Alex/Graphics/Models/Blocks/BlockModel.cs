using System.Collections.Generic;
using System.Linq;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Resources;
using Alex.API.Utils;
using Alex.API.Utils.Vectors;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Utils;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Blocks
{
	public abstract class BlockModel : Model
	{
		public float Scale { get; set; } = 1f;

		public virtual void GetVertices(IBlockAccess blockAccess, ChunkData chunkBuilder, BlockCoordinates blockCoordinates, Vector3 position, BlockState state)
        {
			
        }

		public virtual IEnumerable<BoundingBox> GetBoundingBoxes(BlockState blockState, Vector3 blockPos)
	    {
		    yield return new BoundingBox(blockPos, blockPos + Vector3.One);
	    }

	    protected BlockShaderVertex[] GetFaceVertices(BlockFace blockFace, Vector3 startPosition, Vector3 endPosition, BlockTextureData uvmap)
		{
			Color faceColor = Color.White;
			Vector3 normal = Vector3.Zero;
			Vector3 positionTopLeft = Vector3.Zero, positionBottomLeft = Vector3.Zero, positionBottomRight = Vector3.Zero, positionTopRight = Vector3.Zero;

			switch (blockFace)
			{
				case BlockFace.Up: //Positive Y
					positionTopLeft = new Vector3(startPosition.X, endPosition.Y, endPosition.Z);
					positionTopRight = new Vector3(endPosition.X, endPosition.Y, endPosition.Z);

					positionBottomLeft = new Vector3(startPosition.X, endPosition.Y, startPosition.Z);
					positionBottomRight = new Vector3(endPosition.X, endPosition.Y, startPosition.Z);

					normal = Vector3.Up;
					faceColor = uvmap.ColorTop; //new Color(0x00, 0x00, 0xFF);
					break;
				case BlockFace.Down: //Negative Y
					positionTopLeft = new Vector3(startPosition.X, startPosition.Y, endPosition.Z);
					positionTopRight = new Vector3(endPosition.X, startPosition.Y, endPosition.Z);

					positionBottomLeft = new Vector3(startPosition.X, startPosition.Y, startPosition.Z);
					positionBottomRight = new Vector3(endPosition.X, startPosition.Y, startPosition.Z);

					normal = Vector3.Down;
					faceColor = uvmap.ColorBottom; //new Color(0xFF, 0xFF, 0x00);
					break;
				case BlockFace.West: //Negative X
					positionTopLeft = new Vector3(startPosition.X, endPosition.Y, startPosition.Z);
					positionTopRight = new Vector3(startPosition.X, endPosition.Y, endPosition.Z);

					positionBottomLeft = new Vector3(startPosition.X, startPosition.Y, startPosition.Z);
					positionBottomRight = new Vector3(startPosition.X, startPosition.Y, endPosition.Z);

					normal = Vector3.Left;
					faceColor = uvmap.ColorLeft; // new Color(0xFF, 0x00, 0xFF);
					break;
				case BlockFace.East: //Positive X
					positionTopLeft = new Vector3(endPosition.X, endPosition.Y, startPosition.Z);
					positionTopRight = new Vector3(endPosition.X, endPosition.Y, endPosition.Z);

					positionBottomLeft = new Vector3(endPosition.X, startPosition.Y, startPosition.Z);
					positionBottomRight = new Vector3(endPosition.X, startPosition.Y, endPosition.Z);

					normal = Vector3.Right;
					faceColor = uvmap.ColorRight; //new Color(0x00, 0xFF, 0xFF);
					break;
				case BlockFace.South: //Positive Z
					positionTopLeft = new Vector3(startPosition.X, endPosition.Y, endPosition.Z);
					positionTopRight = new Vector3(endPosition.X, endPosition.Y, endPosition.Z);

					positionBottomLeft = new Vector3(startPosition.X, startPosition.Y, endPosition.Z);
					positionBottomRight = new Vector3(endPosition.X, startPosition.Y, endPosition.Z);

					normal = Vector3.Backward;
					faceColor = uvmap.ColorFront; // ew Color(0x00, 0xFF, 0x00);
					break;
				case BlockFace.North: //Negative Z
					positionTopLeft = new Vector3(startPosition.X, endPosition.Y, startPosition.Z);
					positionTopRight = new Vector3(endPosition.X, endPosition.Y, startPosition.Z);

					positionBottomLeft = new Vector3(startPosition.X, startPosition.Y, startPosition.Z);
					positionBottomRight = new Vector3(endPosition.X, startPosition.Y, startPosition.Z);

					normal = Vector3.Forward;
					faceColor = uvmap.ColorBack; // new Color(0xFF, 0x00, 0x00);
					break;
				case BlockFace.None:
					break;
			}

			var topLeft = new BlockShaderVertex(positionTopLeft, normal, uvmap.TopLeft, faceColor);
			var topRight = new BlockShaderVertex(positionTopRight, normal, uvmap.TopRight, faceColor);
			var bottomLeft = new BlockShaderVertex(positionBottomLeft, normal, uvmap.BottomLeft,
				faceColor);
			var bottomRight = new BlockShaderVertex(positionBottomRight, normal, uvmap.BottomRight,
				faceColor);

			switch (blockFace)
			{
				case BlockFace.Up:
					return new BlockShaderVertex[]
					{
						bottomLeft, topLeft, topRight,
						bottomRight, bottomLeft, topRight
					};
				case BlockFace.Down:
					return new BlockShaderVertex[]
					{
						topLeft, bottomLeft, topRight,
						bottomLeft, bottomRight, topRight
					};
				case BlockFace.South:
					return new BlockShaderVertex[]
					{
						topLeft, bottomLeft, topRight,
						bottomLeft, bottomRight, topRight
					};
				case BlockFace.East:
					return new BlockShaderVertex[]
					{
						bottomLeft, topLeft, topRight,
						bottomRight, bottomLeft, topRight
					};
				case BlockFace.North:
					return new BlockShaderVertex[]
					{
						bottomLeft, topLeft, topRight,
						bottomRight, bottomLeft, topRight
					};
				case BlockFace.West:
					return new BlockShaderVertex[]
					{
						topLeft, bottomLeft, topRight,
						bottomLeft, bottomRight, topRight
					};
					break;
				default:
					return new BlockShaderVertex[0];
			}
		}

	    protected static void GetLight(IBlockAccess world, Vector3 facePosition, out byte blockLight, out byte skyLight, bool smooth = false)
		{
			var faceBlock = world.GetBlockState(facePosition).Block;
			
			skyLight = world.GetSkyLight(facePosition);
			blockLight = world.GetBlockLight(facePosition);

			//if (skyLight == 15 || blockLight == 15)
			//	return;
			
			if (!smooth && !faceBlock.Transparent && !(skyLight > 0 || blockLight > 0))
			{
				return;// (byte)Math.Min(blockLight + skyLight, 15);
			}


			Vector3 lightOffset = Vector3.Zero;

			byte highestBlocklight = blockLight;
		    byte highestSkylight = skyLight;
		    bool lightFound = false;
		    for(int i = 0; i < 6; i++)
		    {
			    switch (i)
			    {
					case 0:
						lightOffset = Vector3.Up;
						break;
					case 1:
						lightOffset = Vector3.Down;
						break;
					case 2:
						lightOffset = Vector3.Forward;
						break;
					case 3:
						lightOffset = Vector3.Backward;
						break;
					case 4:
						lightOffset = Vector3.Left;
						break;
					case 5:
						lightOffset = Vector3.Right;
						break;
			    }

			    skyLight = world.GetSkyLight(facePosition + lightOffset);
			    blockLight = world.GetBlockLight(facePosition + lightOffset);
			    
				if (skyLight > 0 || blockLight > 0)
			    {
				    if (skyLight > 0)
				    {
					    lightFound = true;
						break;
				    }
				    else if (blockLight > highestBlocklight)
				    {
					    highestBlocklight = blockLight;
					    highestSkylight = skyLight;
				    }
			    }
			}

		    if (!lightFound)
		    {
			    skyLight = highestSkylight;
			    if (highestBlocklight > 0)
			    {
				    blockLight = (byte)(highestBlocklight - 1);
				}
			    else
			    {
				    blockLight = 0;
			    }
		    }

		    //(byte)Math.Min(Math.Max(0, blockLight + skyLight), 15);
	    }

	    protected BlockTextureData GetTextureUVMap(ResourceManager resources,
			ResourceLocation texture,
			float x1,
			float x2,
			float y1,
			float y2,
			int rot,
			Color color,
			TextureInfo? ti)
		{
			if (resources == null)
			{
				x1 = 0;
				x2 = 1 / 32f;
				y1 = 0;
				y2 = 1 / 32f;

				return new BlockTextureData(new TextureInfo(new Vector2(), Vector2.Zero, 16, 16, false), 
					new Microsoft.Xna.Framework.Vector2(x1, y1), new Microsoft.Xna.Framework.Vector2(x2, y1),
					new Microsoft.Xna.Framework.Vector2(x1, y2), new Microsoft.Xna.Framework.Vector2(x2, y2), color,
					color, color);
			}

			if (ti == null)
			{
				ti = resources.Atlas.GetAtlasLocation(texture);
			}

			var textureInfo = ti.Value;

			var tw = textureInfo.Width;
			var th = textureInfo.Height;

			x1 = (x1 * (tw));
			x2 = (x2 * (tw ));
			y1 = (y1 * (th));
			y2 = (y2 * (th));

			var map = new BlockTextureData(textureInfo,
				new Microsoft.Xna.Framework.Vector2(x1, y1), new Microsoft.Xna.Framework.Vector2(x2, y1),
				new Microsoft.Xna.Framework.Vector2(x1, y2), new Microsoft.Xna.Framework.Vector2(x2, y2), color, color,
				color, textureInfo.Animated);

			return map;
		}

		public static BlockFace[] INVALID_FACE_ROTATION = new BlockFace[]
	    {
		    BlockFace.Up,
		    BlockFace.Down,
		    BlockFace.None
	    };
	    

	    public static BlockFace[] FACE_ROTATION =
		{
			BlockFace.North,
			BlockFace.East,
			BlockFace.South,
			BlockFace.West
		};

	    protected static BlockFace[] FACE_ROTATION_X =
		{
			BlockFace.North,
			BlockFace.Down,
			BlockFace.South,
			BlockFace.Up
		};

		protected static BlockFace[] INVALID_FACE_ROTATION_X = new BlockFace[]
		{
			BlockFace.East,
			BlockFace.West,
			BlockFace.None
		};


		public static BlockFace RotateDirection(BlockFace val, int offset, BlockFace[] rots, BlockFace[] invalid){
			if (invalid.Any(d => d == val))
				return val;

			int pos = 0;
			for (var index = 0; index < rots.Length; index++)
			{
				if (rots[index] != val)
					continue;
				
				pos = index;
			}

			return rots[(rots.Length + pos + offset) % rots.Length];
		}
		
		public Color CombineColors(params Color[] aColors)
		{
			int r = 0;
			int g = 0;
			int b = 0;
			foreach (Color c in aColors)
			{
				r += c.R;
				g += c.G;
				b += c.B;
			}

			r /= aColors.Length;
			g /= aColors.Length;
			b /= aColors.Length;

			return new Color(r, g, b);
		}
	}
}
