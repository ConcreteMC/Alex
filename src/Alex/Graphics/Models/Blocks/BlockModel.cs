using System;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks;
using Alex.ResourcePackLib.Json;
using Alex.Utils;
using Microsoft.Xna.Framework;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;

namespace Alex.Graphics.Models.Blocks
{
	public class BlockModel : Model, IBlockModel
	{
        public BlockModel()
        {

        }

		public float Scale { get; set; } = 1f;

		public virtual VertexPositionNormalTextureColor[] GetVertices(IWorld world, Vector3 position, IBlock baseBlock)
        {
            return new VertexPositionNormalTextureColor[0];
        }

	    public virtual BoundingBox GetBoundingBox(Vector3 position, IBlock requestingBlock)
	    {
			return new BoundingBox(position, position + Vector3.One);
	    }

		protected VertexPositionNormalTextureColor[] GetFaceVertices(BlockFace blockFace, Vector3 startPosition, Vector3 endPosition, UVMap uvmap)
		{
			Color faceColor = Color.White;
			Vector3 normal = Vector3.Zero;
			Vector3 textureTopLeft = Vector3.Zero, textureBottomLeft = Vector3.Zero, textureBottomRight = Vector3.Zero, textureTopRight = Vector3.Zero;

			switch (blockFace)
			{
				case BlockFace.Up: //Positive Y
					textureTopLeft = VectorExtension.From(startPosition, endPosition, endPosition);
					textureTopRight = VectorExtension.From(endPosition, endPosition, endPosition);

					textureBottomLeft = VectorExtension.From(startPosition, endPosition, startPosition);
					textureBottomRight = VectorExtension.From(endPosition, endPosition, startPosition);

					normal = Vector3.Up;
					faceColor = uvmap.ColorTop; //new Color(0x00, 0x00, 0xFF);
					break;
				case BlockFace.Down: //Negative Y
					textureTopLeft = VectorExtension.From(startPosition, startPosition, endPosition);
					textureTopRight = VectorExtension.From(endPosition, startPosition, endPosition);

					textureBottomLeft = VectorExtension.From(startPosition, startPosition, startPosition);
					textureBottomRight = VectorExtension.From(endPosition, startPosition, startPosition);

					normal = Vector3.Down;
					faceColor = uvmap.ColorBottom; //new Color(0xFF, 0xFF, 0x00);
					break;
				case BlockFace.West: //Negative X
					textureTopLeft = VectorExtension.From(startPosition, endPosition, startPosition);
					textureTopRight = VectorExtension.From(startPosition, endPosition, endPosition);

					textureBottomLeft = VectorExtension.From(startPosition, startPosition, startPosition);
					textureBottomRight = VectorExtension.From(startPosition, startPosition, endPosition);

					normal = Vector3.Right;
					faceColor = uvmap.ColorLeft; // new Color(0xFF, 0x00, 0xFF);
					break;
				case BlockFace.East: //Positive X
					textureTopLeft = VectorExtension.From(endPosition, endPosition, startPosition);
					textureTopRight = VectorExtension.From(endPosition, endPosition, endPosition);

					textureBottomLeft = VectorExtension.From(endPosition, startPosition, startPosition);
					textureBottomRight = VectorExtension.From(endPosition, startPosition, endPosition);

					normal = Vector3.Left;
					faceColor = uvmap.ColorRight; //new Color(0x00, 0xFF, 0xFF);
					break;
				case BlockFace.South: //Positive Z
					textureTopLeft = VectorExtension.From(startPosition, endPosition, startPosition);
					textureTopRight = VectorExtension.From(endPosition, endPosition, startPosition);

					textureBottomLeft = VectorExtension.From(startPosition, startPosition, startPosition);
					textureBottomRight = VectorExtension.From(endPosition, startPosition, startPosition);

					normal = Vector3.Forward;
					faceColor = uvmap.ColorFront; // ew Color(0x00, 0xFF, 0x00);
					break;
				case BlockFace.North: //Negative Z
					textureTopLeft = VectorExtension.From(startPosition, endPosition, endPosition);
					textureTopRight = VectorExtension.From(endPosition, endPosition, endPosition);

					textureBottomLeft = VectorExtension.From(startPosition, startPosition, endPosition);
					textureBottomRight = VectorExtension.From(endPosition, startPosition, endPosition);

					normal = Vector3.Backward;
					faceColor = uvmap.ColorBack; // new Color(0xFF, 0x00, 0x00);
					break;
				case BlockFace.None:
					break;
			}

			var topLeft = new VertexPositionNormalTextureColor(textureTopLeft, normal, uvmap.TopLeft, faceColor);
			var topRight = new VertexPositionNormalTextureColor(textureTopRight, normal, uvmap.TopRight, faceColor);
			var bottomLeft = new VertexPositionNormalTextureColor(textureBottomLeft, normal, uvmap.BottomLeft,
				faceColor);
			var bottomRight = new VertexPositionNormalTextureColor(textureBottomRight, normal, uvmap.BottomRight,
				faceColor);

			switch (blockFace)
			{
				case BlockFace.Up:
					return (new[]
					{
						bottomLeft, topLeft, topRight,
						bottomRight, bottomLeft, topRight
					});
				case BlockFace.Down:
					return (new[]
					{
						topLeft, bottomLeft, topRight,
						bottomLeft, bottomRight, topRight
					});
				case BlockFace.North:
					return (new[]
					{
						topLeft, bottomLeft, topRight,
						bottomLeft, bottomRight, topRight
					});
				case BlockFace.East:
					return (new[]
					{
						bottomLeft, topLeft, topRight,
						bottomRight, bottomLeft, topRight
					});
				case BlockFace.South:
					return (new[]
					{
						bottomLeft, topLeft, topRight,
						bottomRight, bottomLeft, topRight
					});
				case BlockFace.West:
					return (new[]
					{
						topLeft, bottomLeft, topRight,
						bottomLeft, bottomRight, topRight
					});
			}

			return new VertexPositionNormalTextureColor[0];
		}

		protected byte GetLight(IWorld world, Vector3 position, bool smooth = false)
		{
			byte skyLight = world.GetSkyLight(position);
			byte blockLight = world.GetBlockLight(position);
			if (!smooth)
		    {
			    return (byte)Math.Min(blockLight + skyLight, 15);
			}
			Vector3 lightOffset = Vector3.Zero;

		    bool initial = true;

		    byte highestBlocklight = 0;
		    byte highestSkylight = 0;
		    bool lightFound = false;
		    for(int i = 0; i < 6; i++)
		    {
			    switch (i)
			    {
					case 0:
						lightOffset = Vector3.Zero;
						break;
					case 1:
						initial = false;
						lightOffset = Vector3.Up;
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
					case 6:
						lightOffset = Vector3.Down;
						break;
			    }

			    skyLight = world.GetSkyLight(position + lightOffset);
			    blockLight = world.GetBlockLight(position + lightOffset);
			    if (initial && (blockLight > 0 || skyLight > 0))
			    {
				    lightFound = true;

					break;
			    }
				else if (skyLight > 0 || blockLight > 0)
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

		    return (byte)Math.Min(blockLight + skyLight, 15);
	    }
		
	    protected UVMap GetTextureUVMap(ResourceManager resources, string texture, float x1, float x2, float y1, float y2, int rot)
	    {
			if (resources == null)
		    {
			    x1 = 0;
			    x2 = 1 / 32f;
			    y1 = 0;
			    y2 = 1 / 32f;

			    return new UVMap(new Microsoft.Xna.Framework.Vector2(x1, y1),
				    new Microsoft.Xna.Framework.Vector2(x2, y1), new Microsoft.Xna.Framework.Vector2(x1, y2),
				    new Microsoft.Xna.Framework.Vector2(x2, y2), Color.White, Color.White, Color.White);
		    }

		    var textureInfo = resources.Atlas.GetAtlasLocation(texture.Replace("blocks/", ""));
		    var textureLocation = textureInfo.Position;

		    var uvSize = resources.Atlas.AtlasSize;

		    var tpw = (1f / uvSize.X); //0.0625
		    var tph = (1f / uvSize.Y);

		    textureLocation.X /= uvSize.X;
		    textureLocation.Y /= uvSize.Y;

		    if (rot > 0)
			{
				var tw = textureInfo.Width;
				var th = textureInfo.Height;

				var w = x2 - x1;
			    var h = y2 - y1;
			    var x = x1;
			    var y = y1;

			    if (rot == 270)
			    {
				    y2 = x + w;
				    x1 = tw  - (y + h);
				    x2 = tw - y;
				    y1 = x;

			    }
			    else if (rot == 180)
			    {
				    y1 = th - (y + h);
				    y2 = th - y;
				    x1 = x + w;
				    x2 = x;

			    }
			    else if (rot == 90)
			    {
				    y1 = x + w;
					y2 = x;
				    
				    x1 = y;
				    x2 = y + h;
				}
		    }

		    x1 = textureLocation.X + (x1 * tpw);
		    x2 = textureLocation.X + (x2 * tpw);
		    y1 = textureLocation.Y + (y1 * tph);
		    y2 = textureLocation.Y + (y2 * tph);

			return new UVMap(new Microsoft.Xna.Framework.Vector2(x1, y1),
			    new Microsoft.Xna.Framework.Vector2(x2, y1), new Microsoft.Xna.Framework.Vector2(x1, y2),
			    new Microsoft.Xna.Framework.Vector2(x2, y2), Color.White, Color.White, Color.White);
		}

		protected static BlockFace[] FACE_ROTATION =
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

		protected BlockFace RotateDirection(BlockFace val, int offset, BlockFace[] rots, BlockFace[] invalid){
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
