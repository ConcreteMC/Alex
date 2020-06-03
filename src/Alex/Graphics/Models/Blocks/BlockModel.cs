using System;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.ResourcePackLib.Json;
using Alex.Utils;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Blocks
{
	public class BlockModel : Model
	{
        public BlockModel()
        {

        }

        public virtual BoundingBox BoundingBox { get; } = new BoundingBox(Vector3.Zero, Vector3.One);
        
		public float Scale { get; set; } = 1f;

		public virtual (BlockShaderVertex[] vertices, int[] indexes) GetVertices(IBlockAccess world, Vector3 position, Block baseBlock)
        {
            return (new BlockShaderVertex[0], new int[0]);
        }

	    public virtual BoundingBox GetBoundingBox(Vector3 position, Block requestingBlock)
	    {
			return new BoundingBox(position, position + Vector3.One);
	    }

	    public virtual BoundingBox? GetPartBoundingBox(Vector3 position, BoundingBox entityBox)
	    {
		    return new BoundingBox(position, position + Vector3.One);
	    }

	    public virtual BoundingBox[] GetIntersecting(Vector3 position, BoundingBox box)
	    {
		    return new BoundingBox[0];
	    }

	    protected BlockShaderVertex[] GetFaceVertices(BlockFace blockFace, Vector3 startPosition, Vector3 endPosition, UVMap uvmap, out int[] indexes)
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
					positionTopLeft = new Vector3(startPosition.X, endPosition.Y, startPosition.Z);
					positionTopRight = new Vector3(endPosition.X, endPosition.Y, startPosition.Z);

					positionBottomLeft = new Vector3(startPosition.X, startPosition.Y, startPosition.Z);
					positionBottomRight = new Vector3(endPosition.X, startPosition.Y, startPosition.Z);

					normal = Vector3.Backward;
					faceColor = uvmap.ColorFront; // ew Color(0x00, 0xFF, 0x00);
					break;
				case BlockFace.North: //Negative Z
					positionTopLeft = new Vector3(startPosition.X, endPosition.Y, endPosition.Z);
					positionTopRight = new Vector3(endPosition.X, endPosition.Y, endPosition.Z);

					positionBottomLeft = new Vector3(startPosition.X, startPosition.Y, endPosition.Z);
					positionBottomRight = new Vector3(endPosition.X, startPosition.Y, endPosition.Z);

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
					indexes = new int[]
					{
						2, 0, 1,
						3, 2, 1
					};
					break;
				case BlockFace.Down:
					indexes = new[]
					{
						0, 2, 1,
						2, 3, 1
					};
					break;
				case BlockFace.North:
					indexes = new[]
					{
						0, 2, 1,
						2, 3, 1
					};
					break;
				case BlockFace.East:
					indexes = new[]
					{
						2, 0, 1,
						3, 2, 1
					};
					break;
				case BlockFace.South:
					indexes = new[]
					{
						2, 0, 1,
						3, 2, 1
					};
					break;
				case BlockFace.West:
					indexes = new[]
					{
						0, 2, 1,
						2, 3, 1
					};
					break;
				default:
					indexes = new int[0];
					break;
			}
			
			return new[]
			{
				topLeft, topRight, bottomLeft, bottomRight
			};

			return new BlockShaderVertex[0];
		}

		protected void GetLight(IBlockAccess world, Vector3 facePosition, out byte blockLight, out byte skyLight, bool smooth = false)
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

		protected UVMap GetTextureUVMap(ResourceManager resources,
			string texture,
			float x1,
			float x2,
			float y1,
			float y2,
			int rot,
			Color color)
		{
			if (resources == null)
			{
				x1 = 0;
				x2 = 1 / 32f;
				y1 = 0;
				y2 = 1 / 32f;

				return new UVMap(
					new Microsoft.Xna.Framework.Vector2(x1, y1), new Microsoft.Xna.Framework.Vector2(x2, y1),
					new Microsoft.Xna.Framework.Vector2(x1, y2), new Microsoft.Xna.Framework.Vector2(x2, y2), color,
					color, color);
			}

			var textureInfo     = resources.Atlas.GetAtlasLocation(texture, out var uvSize);
			var textureLocation = textureInfo.Position;

			var xw = (textureInfo.Width / 16f) / uvSize.X;
			var yh = (textureInfo.Height / 16f) / uvSize.Y;

			textureLocation.X /= uvSize.X;
			textureLocation.Y /= uvSize.Y;

			x1 = textureLocation.X + (x1 * xw);
			x2 = textureLocation.X + (x2 * xw);
			y1 = textureLocation.Y + (y1 * yh);
			y2 = textureLocation.Y + (y2 * yh);

			var map = new UVMap(
				new Microsoft.Xna.Framework.Vector2(x1, y1), new Microsoft.Xna.Framework.Vector2(x2, y1),
				new Microsoft.Xna.Framework.Vector2(x1, y2), new Microsoft.Xna.Framework.Vector2(x2, y2), color, color,
				color);

			 if (rot > 0)
				 map.Rotate(rot);

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
			/*BlockFace.West,
			BlockFace.North,
			BlockFace.East,
			BlockFace.South*/
			
			BlockFace.East,
			BlockFace.South,
			BlockFace.West,
			BlockFace.North
		};

		public static BlockFace[] FACE_ROTATION_X =
		{
			BlockFace.North,
			BlockFace.Down,
			BlockFace.South,
			BlockFace.Up
		};
		
		public static BlockFace[] INVALID_FACE_ROTATION_X = new BlockFace[]
		{
			BlockFace.East,
			BlockFace.West,
			BlockFace.None
		};


		public static BlockFace RotateDirection(BlockFace val, int offset, BlockFace[] rots, BlockFace[] invalid){
			foreach(var d in invalid) {
				if (d == val) {
					return val;
				}
			}

			int pos = 0;
			for (var index = 0; index < rots.Length; index++)
			{
				var rot = rots[index];

				if (rot == val)
				{
					pos = index;
					break;
				}
			}

			return rots[(rots.Length + pos + offset) % rots.Length];
		}
	}
}
