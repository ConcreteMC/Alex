using System;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.ResourcePackLib.Json;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Blocks
{
	public class BlockModel : Model, IBlockModel
	{
        public BlockModel()
        {

        }

		public float Scale { get; set; } = 1f;

		public virtual (VertexPositionNormalTextureColor[] vertices, int[] indexes) GetVertices(IWorld world, Vector3 position, IBlock baseBlock)
        {
            return (new VertexPositionNormalTextureColor[0], new int[0]);
        }

	    public virtual BoundingBox GetBoundingBox(Vector3 position, IBlock requestingBlock)
	    {
			return new BoundingBox(position, position + Vector3.One);
	    }

	    protected VertexPositionNormalTextureColor[] GetQuadVertices(BlockFace face, Vector3 from, Vector3 to, UVMap uv, out int[] indicies)
	    {
		    indicies = new[] { 0, 1, 3, 1, 2, 3 };
		    
		    Vector3[] unit;
		    Vector3 normal;
		    Color faceColor;
		    switch (face)
		    {
			    case BlockFace.West: //Negative X
				    normal = Vector3.Left;
				    faceColor = uv.ColorLeft;
				    unit = QuadMesh[1];
				    break;
			    case BlockFace.East: //Positive X
				    normal = Vector3.Right;
				    faceColor = uv.ColorRight;
				    unit = QuadMesh[0];
				    break;
			    case BlockFace.South: //Positive Z
				    normal = Vector3.Backward;
				    faceColor = uv.ColorBack;
				    unit = QuadMesh[3];
				    break;
			    case BlockFace.North: //Negative Z
				    normal = Vector3.Forward;
				    faceColor = uv.ColorFront;
				    unit = QuadMesh[2];
				    break;
				default:
				    return null;	
		    }

		    var size = (to - from);
		    var quad = new VertexPositionNormalTextureColor[4];
		    
		    for (int i = 0; i < 4; i++)
		    {
			    Vector2 texture = Vector2.Zero;
			    Vector3 pos = unit[i];
			    switch (i)
			    {
				    case 0:
					    texture = uv.BottomLeft;
					    break;
				    case 1:
					    texture = uv.BottomRight;
					    break;
				    case 2:
					    texture = uv.TopRight;
					    break;
				    case 3:
					    texture = uv.TopLeft;
					    break;
			    }
			    
			    quad[i] = new VertexPositionNormalTextureColor( (pos * size) , normal, texture, faceColor);
		    }
		    return quad;
	    }
	    
	    protected VertexPositionNormalTextureColor[] GetFlatVertices(BlockFace face, Vector3 from, Vector3 to, UVMap uv, out int[] indicies)
	    {
		    indicies = new[] { 0, 1, 3, 1, 2, 3 };

		    Vector3[] unit;
		    Vector3 normal;
		    Color faceColor;
		    switch (face)
		    {
			    case BlockFace.South: //Positive Z
				    normal = Vector3.Backward;
				    faceColor = uv.ColorFront;
				    unit = FlatMesh[0];
				    break;
			    case BlockFace.North: //Negative Z
				    normal = Vector3.Forward;
				    faceColor = uv.ColorBack;
				    unit = FlatMesh[0];
				    break;
			    case BlockFace.Up:
				    normal = Vector3.Up;
				    faceColor = uv.ColorTop;
				    unit = FlatMesh[3];
				    break;
			    case BlockFace.Down:
				    normal = Vector3.Down;
				    faceColor = uv.ColorBottom;
				    unit = FlatMesh[2];
				    break;
			    default:
				    return null;	
		    }

		    var size = (to - from);
		    var quad = new VertexPositionNormalTextureColor[4];
		    
		    for (int i = 0; i < 4; i++)
		    {
			    Vector2 texture = Vector2.Zero;
			    Vector3 pos = unit[i];
			    switch (i)
			    {
				    case 0:
					    texture = uv.BottomLeft;
					    break;
				    case 1:
					    texture = uv.BottomRight;
					    break;
				    case 2:
					    texture = uv.TopRight;
					    break;
				    case 3:
					    texture = uv.TopLeft;
					    break;
			    }
			    
			    quad[i] = new VertexPositionNormalTextureColor( (pos * size) , normal, texture, faceColor);
		    }
		    return quad;
	    }
	    
	    protected VertexPositionNormalTextureColor[] GetFaceVertices(BlockFace blockFace, Vector3 startPosition, Vector3 endPosition, UVMap uvmap, out int[] indexes)
		{
			var size = (endPosition - startPosition);
			
			Color faceColor = Color.White;
			Vector3 normal = Vector3.Zero;
			Vector3 textureTopLeft = Vector3.Zero, textureBottomLeft = Vector3.Zero, textureBottomRight = Vector3.Zero, textureTopRight = Vector3.Zero;

			switch (blockFace)
			{
				case BlockFace.Up: //Positive Y
					textureTopLeft = From(startPosition, endPosition, endPosition);
					textureTopRight = From(endPosition, endPosition, endPosition);

					textureBottomLeft = From(startPosition, endPosition, startPosition);
					textureBottomRight = From(endPosition, endPosition, startPosition);

					normal = Vector3.Up;
					faceColor = uvmap.ColorTop; //new Color(0x00, 0x00, 0xFF);
					break;
				case BlockFace.Down: //Negative Y
					textureTopLeft = From(startPosition, startPosition, endPosition);
					textureTopRight = From(endPosition, startPosition, endPosition);

					textureBottomLeft = From(startPosition, startPosition, startPosition);
					textureBottomRight = From(endPosition, startPosition, startPosition);

					normal = Vector3.Down;
					faceColor = uvmap.ColorBottom; //new Color(0xFF, 0xFF, 0x00);
					break;
				case BlockFace.West: //Negative X
					textureTopLeft = From(startPosition, endPosition, startPosition);
					textureTopRight = From(startPosition, endPosition, endPosition);

					textureBottomLeft = From(startPosition, startPosition, startPosition);
					textureBottomRight = From(startPosition, startPosition, endPosition);

					normal = Vector3.Left;
					faceColor = uvmap.ColorLeft; // new Color(0xFF, 0x00, 0xFF);
					break;
				case BlockFace.East: //Positive X
					textureTopLeft = From(endPosition, endPosition, startPosition);
					textureTopRight = From(endPosition, endPosition, endPosition);

					textureBottomLeft = From(endPosition, startPosition, startPosition);
					textureBottomRight = From(endPosition, startPosition, endPosition);

					normal = Vector3.Right;
					faceColor = uvmap.ColorRight; //new Color(0x00, 0xFF, 0xFF);
					break;
				case BlockFace.South: //Positive Z
					textureTopLeft = From(startPosition, endPosition, startPosition);
					textureTopRight = From(endPosition, endPosition, startPosition);

					textureBottomLeft = From(startPosition, startPosition, startPosition);
					textureBottomRight = From(endPosition, startPosition, startPosition);

					normal = Vector3.Backward;
					faceColor = uvmap.ColorFront; // ew Color(0x00, 0xFF, 0x00);
					break;
				case BlockFace.North: //Negative Z
					textureTopLeft = From(startPosition, endPosition, endPosition);
					textureTopRight = From(endPosition, endPosition, endPosition);

					textureBottomLeft = From(startPosition, startPosition, endPosition);
					textureBottomRight = From(endPosition, startPosition, endPosition);

					normal = Vector3.Forward;
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

			/*
			 * return new[]
			{
				topLeft, topRight, bottomLeft, bottomRight
			};
			 */
			switch (blockFace)
			{
				case BlockFace.Up:
					indexes = new int[]
					{
						2, 0, 1,
						3, 2, 1
					};
					break;
				/*return (new[]
				{
					bottomLeft, topLeft, topRight,
					bottomRight, bottomLeft, topRight
				});*/
				case BlockFace.Down:
					indexes = new[]
					{
						0, 2, 1,
						2, 3, 1
					};
					break;
				/*return (new[]
				{
					topLeft, bottomLeft, topRight,
					bottomLeft, bottomRight, topRight
				});*/
				case BlockFace.North:
					indexes = new[]
					{
						0, 2, 1,
						2, 3, 1
					};
					break;
					/*return (new[]
					{
						topLeft, bottomLeft, topRight,
						bottomLeft, bottomRight, topRight
					});*/
				case BlockFace.East:
					indexes = new[]
					{
						2, 0, 1,
						3, 2, 1
					};
					break;
					/*return (new[]
					{
						bottomLeft, topLeft, topRight,
						bottomRight, bottomLeft, topRight
					});*/
				case BlockFace.South:
					indexes = new[]
					{
						2, 0, 1,
						3, 2, 1
					};
					break;
					/*return (new[]
					{
						bottomLeft, topLeft, topRight,
						bottomRight, bottomLeft, topRight
					});*/
				case BlockFace.West:
					indexes = new[]
					{
						0, 2, 1,
						2, 3, 1
					};
					break;
					/*return (new[]
					{
						topLeft, bottomLeft, topRight,
						bottomLeft, bottomRight, topRight
					});*/
				default:
					indexes = new int[0];
					break;
			}
			
			return new[]
			{
				topLeft, topRight, bottomLeft, bottomRight
			};

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

		    var textureInfo = resources.Atlas.GetAtlasLocation(texture);
		    var textureLocation = textureInfo.Position;

		    var uvSize = resources.Atlas.AtlasSize;

		    var tpw = (resources.Atlas.TextureWidth / QuickMath.Max(x1, x2)) / uvSize.X; //0.0625
            var tph = (resources.Atlas.TextureHeight / QuickMath.Max(y1, y2)) / uvSize.Y;

		    textureLocation.X /= uvSize.X;
		    textureLocation.Y /= uvSize.Y;
            
		   /* x1 = textureLocation.X + ((x1) * tpw);
		    x2 = textureLocation.X + ((x2) * tpw);
		    y1 = textureLocation.Y + ((y1) * tph);
		    y2 = textureLocation.Y + ((y2) * tph);*/

            x1 = textureLocation.X + x1 * (((textureInfo.Width / 16f) / uvSize.X));
            x2 = textureLocation.X + x2 * (((textureInfo.Width / 16f) / uvSize.X));
            y1 = textureLocation.Y + y1 * (((textureInfo.Height / 16f) / uvSize.Y));
            y2 = textureLocation.Y + y2 * (((textureInfo.Height / 16f) / uvSize.Y));

            var map = new UVMap(new Microsoft.Xna.Framework.Vector2(x1, y1),
			    new Microsoft.Xna.Framework.Vector2(x2, y1), new Microsoft.Xna.Framework.Vector2(x1, y2),
			    new Microsoft.Xna.Framework.Vector2(x2, y2), Color.White, Color.White, Color.White);

			if (rot > 0)
			{
				map.Rotate(rot);
			}

			return map;
	    }

	    protected static BlockFace[] INVALID_FACE_ROTATION = new BlockFace[]
	    {
		    BlockFace.Up,
		    BlockFace.Down,
		    BlockFace.None
	    };
	    
		protected static BlockFace[] FACE_ROTATION =
		{
			BlockFace.East,
			BlockFace.South,
			BlockFace.West,
			BlockFace.North
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

		public static Vector3 From(Vector3 x, Vector3 y, Vector3 z)
		{
			return new Vector3(x.X, y.Y, z.Z);
		}
	}
}
