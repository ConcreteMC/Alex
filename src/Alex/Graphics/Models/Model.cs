using System.Linq;
using Alex.API.Blocks;
using Alex.API.Utils;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models
{
    public abstract class Model
    {
	    static Model()
	    {
		    QuadMesh = new Vector3[4][];

		    QuadMesh[0] = new[]
		    {
			    new Vector3(1, 0, 1),
			    new Vector3(0, 0, 0),
			    new Vector3(0, 1, 0),
			    new Vector3(1, 1, 1)
		    };

		    QuadMesh[1] = new[]
		    {
			    new Vector3(0, 0, 0),
			    new Vector3(1, 0, 1),
			    new Vector3(1, 1, 1),
			    new Vector3(0, 1, 0)
		    };

		    QuadMesh[2] = new[]
		    {
			    new Vector3(0, 0, 1),
			    new Vector3(1, 0, 0),
			    new Vector3(1, 1, 0),
			    new Vector3(0, 1, 1)
		    };

		    QuadMesh[3] = new[]
		    {
			    new Vector3(1, 0, 0),
			    new Vector3(0, 0, 1),
			    new Vector3(0, 1, 1),
			    new Vector3(1, 1, 0)
		    };
	    }

	    protected static readonly Vector3[] QuadNormals =
	    {
		    new Vector3(0, 0, 1),
		    new Vector3(0, 0, -1),
		    new Vector3(1, 0, 0),
		    new Vector3(-1, 0, 0),
		    new Vector3(0, 1, 0),
		    new Vector3(0, -1, 0)
	    };

	    protected static readonly Vector3[][] QuadMesh;

	    protected static readonly Vector3[][] FlatMesh = new Vector3[4][]
	    {
		    new[]
		    {
			    new Vector3(0, 0, 0),
			    new Vector3(1, 0, 0),
			    new Vector3(1, 1, 0),
			    new Vector3(0, 1, 0)
		    },
		    new[]
		    {
			    new Vector3(0, 0, 0),
			    new Vector3(0, 0, 1),
			    new Vector3(0, 1, 1),
			    new Vector3(0, 1, 0)
		    },
		    new[] //Flat BOTTOM
		    {
			    new Vector3(0, 0, 0),
			    new Vector3(1, 0, 0),
			    new Vector3(1, 0, 1),
			    new Vector3(0, 0, 1)
		    },
		    new[] //Flat TOP
		    {
			    new Vector3(0, 1, 0),
			    new Vector3(1, 1, 0),
			    new Vector3(1, 1, 1),
			    new Vector3(0, 1, 1)
		    }
	    };
	    
	    private static readonly Color LightColor =
		    new Color(245, 245, 225);

	    /// <summary>
	    /// The default lighting information for rendering a block;
	    ///  i.e. when the lighting param to CreateUniformCube == null.
	    /// </summary>
	    private static readonly int[] DefaultLighting =
		    new int[]
		    {
			    15, 15, 15,
			    15, 15, 15
		    };

	    /// <summary>
	    /// Maps a light level [0..15] to a brightness modifier for lighting.
	    /// </summary>
	    private static readonly float[] CubeBrightness =
		    new float[]
		    {
			    0.050f, 0.067f, 0.085f, 0.106f, // [ 0..3 ]
			    0.129f, 0.156f, 0.186f, 0.221f, // [ 4..7 ]
			    0.261f, 0.309f, 0.367f, 0.437f, // [ 8..11]
			    0.525f, 0.638f, 0.789f, 1.000f //  [12..15]
		    };

	    /// <summary>
	    /// The per-face brightness modifier for lighting.
	    /// </summary>
	    private static readonly float[] FaceBrightness =
		    new float[]
		    {
			    0.6f, 0.6f, // North / South
			    0.8f, 0.8f, // East / West
			    1.0f, 0.5f // MinY / MaxY
		    };

	    protected static Color AdjustColor(Color color, BlockFace face, bool shade = true)
	    {
		    float brightness = 1f;
		    if (shade)
		    {
			    switch (face)
			    {
				    case BlockFace.Down:
					    brightness = FaceBrightness[5];
					    break;
				    case BlockFace.Up:
					    brightness = FaceBrightness[4];
					    break;
				    case BlockFace.East:
					    brightness = FaceBrightness[2];
					    break;
				    case BlockFace.West:
					    brightness = FaceBrightness[3];
					    break;
				    case BlockFace.North:
					    brightness = FaceBrightness[0];
					    break;
				    case BlockFace.South:
					    brightness = FaceBrightness[1];
					    break;
				    case BlockFace.None:

					    break;
			    }
		    }

		   // var cubeBrightness = ((1f / 16f) * lighting);
		    
		    //var lightColor = LightColor;
		 //   var light = lightColor.ToVector3() * cubeBrightness;//CubeBrightness[lighting];
		    return new Color(brightness * color.ToVector3());
	    }

		public sealed class Cube
		{
			private static readonly Color   DefaultColor = Color.White;
			private readonly        Vector2 _textureSize;

			private readonly EntityModel _model;
			private readonly Vector3     _pivot;
			private readonly bool        _mirror = false;
			
			private static Vector3 FlipZ(Vector3 origin, Vector3 size)
			{
				//return origin;
				var newOrigin = new Vector3(origin.X, origin.Y, origin.Z);
				if (newOrigin.Z >= 0)
				{
					newOrigin.Z = -(((MathF.Abs(origin.Z) / size.Z) + 1f) * size.Z);
				}
				else
				{
					newOrigin.Z = ((MathF.Abs(origin.Z) / size.Z) - 1f) * size.Z;
				}

				return newOrigin;
			}
			
			private static Vector3 FlipX(Vector3 origin, Vector3 size)
			{
				//return origin;
				var newOrigin = new Vector3(origin.X, origin.Y, origin.Z);
				if (newOrigin.X >= 0)
				{
					newOrigin.X = -(((MathF.Abs(origin.X) / size.X) + 1f) * size.X);
				}
				else
				{
					newOrigin.X = ((MathF.Abs(origin.X) / size.X) - 1f) * size.X;
				}

				return newOrigin;
			}
			
			public Cube(EntityModel model, EntityModelCube cube, Vector2 textureSize, bool mirrored, float inflation)
			{
				_model = model;
				_mirror = mirrored;
				//Mirrored = mirrored;
				var uv     = cube.Uv ?? new EntityModelUV();
				var size   = cube.InflatedSize(inflation);
				var origin =  cube.InflatedOrigin(inflation);//, size;

				_pivot = (cube.Pivot ?? (origin + (size / 2f)));

				this._textureSize = textureSize;
		//		var xScale = (textureSize.X / model.Description.TextureWidth);
		//		var yScale = (textureSize.Y / model.Description.TextureHeight);

				//front verts with position and texture stuff
				_topLeftFront = new Vector3(origin.X, origin.Y + size.Y, origin.Z);
				_topLeftBack = new Vector3(origin.X, origin.Y + size.Y, origin.Z + size.Z);// * size;

				_topRightFront = new Vector3(origin.X + size.X, origin.Y + size.Y, origin.Z);// * size;
				_topRightBack = new Vector3(origin.X + size.X, origin.Y + size.Y, origin.Z + size.Z);// * size;

				// Calculate the position of the vertices on the bottom face.
				_btmLeftFront = new Vector3(origin.X, origin.Y, origin.Z);// * size;
				_btmLeftBack = new Vector3(origin.X, origin.Y, origin.Z + size.Z);//* size;
				_btmRightFront = new Vector3(origin.X + size.X, origin.Y, origin.Z);// * size;
				_btmRightBack =  new Vector3(origin.X + size.X, origin.Y, origin.Z + size.Z);// * size;

				Front = Modify(cube, GetFrontVertex(
					uv.South.WithSize(size.X, size.Y),
					uv.South.Size.HasValue ?  Vector2.Zero : new Vector2(cube.Size.Z, cube.Size.Z)));

				Back = Modify(cube, GetBackVertex(
					uv.North.WithSize(size.X, size.Y),
					uv.North.Size.HasValue ?  Vector2.Zero : new Vector2(cube.Size.Z + cube.Size.Z + cube.Size.X, cube.Size.Z)));

				Left = Modify(cube, GetLeftVertex(
					uv.West.WithSize(size.Z, size.Y),
					uv.West.Size.HasValue ?  Vector2.Zero : new Vector2(0, cube.Size.Z)));

				Right = Modify(cube, GetRightVertex(
					uv.East.WithSize(size.Z, size.Y), 
					uv.East.Size.HasValue ?  Vector2.Zero : new Vector2(cube.Size.Z + cube.Size.X, cube.Size.Z)));

				Top = Modify(cube, GetTopVertex(
					uv.Up.WithSize(size.X, size.Z),
					uv.Up.Size.HasValue ?  Vector2.Zero : new Vector2(cube.Size.Z, 0)));

				Bottom = Modify(cube, GetBottomVertex(
					uv.Down.WithSize(size.X, size.Z),
					uv.Down.Size.HasValue ? Vector2.Zero : new Vector2(cube.Size.Z + cube.Size.X, 0)));
			}

			private (VertexPositionColorTexture[] vertices, short[] indexes) Modify(EntityModelCube cube,
				(VertexPositionColorTexture[] vertices, short[] indexes) data)
			{
				Matrix cubeMatrix = Matrix.Identity;
				if (cube.Rotation.HasValue)
				{
					var rotation = cube.Rotation.Value;

					cubeMatrix = Matrix.CreateTranslation(-_pivot)
					             * Matrix.CreateRotationX(MathUtils.ToRadians(rotation.X))
					             * Matrix.CreateRotationY(MathUtils.ToRadians(rotation.Y))
					             * Matrix.CreateRotationZ(MathUtils.ToRadians(rotation.Z))
					             * Matrix.CreateTranslation(_pivot);
				}

				return (data.vertices.Select(
					x =>
					{
						x.Position = Vector3.Transform(x.Position, cubeMatrix);

						return x;
					}).ToArray(), data.indexes);
			}

			public (VertexPositionColorTexture[] vertices, short[] indexes) Front, Back, Left, Right, Top, Bottom;

			private readonly Vector3 _topLeftFront;
			private readonly Vector3 _topLeftBack;
			private readonly Vector3 _topRightFront;
			private readonly Vector3 _topRightBack;
			private readonly Vector3 _btmLeftFront;
			private readonly Vector3 _btmLeftBack;
			private readonly Vector3 _btmRightFront;
			private readonly Vector3 _btmRightBack;

			private (VertexPositionColorTexture[] vertices, short[] indexes) GetLeftVertex(EntityModelUVData uv, Vector2 size)
			{
				//Vector3 normal = new Vector3(-1.0f, 0.0f, 0.0f) * Size;
				Color normal = AdjustColor(DefaultColor, BlockFace.West);
				
				//var map = GetTextureMapping(uv + new Vector2(Size.Z + Size.X, Size.Z), Size.Z, Size.Y);
				var map = GetTextureMapping(uv.Origin + size, uv.Size.Value.X, uv.Size.Value.Y);
				
				// Add the vertices for the RIGHT face. 
				return (new VertexPositionColorTexture[]
				{
					new VertexPositionColorTexture(_topLeftFront, normal, map.TopRight),
					new VertexPositionColorTexture(_btmLeftFront , normal, map.BotRight),
					new VertexPositionColorTexture(_btmLeftBack, normal, map.BotLeft),
					new VertexPositionColorTexture(_topLeftBack , normal, map.TopLeft),
					//new VertexPositionNormalTexture(_topLeftFront , normal, map.TopLeft),
					//new VertexPositionNormalTexture(_btmLeftBack, normal, map.BotRight),
				}, new short[]
				{
					0, 1, 2, 
					3, 0, 2
					//0, 1, 2, 3, 0, 2
				});
			}

			private (VertexPositionColorTexture[] vertices, short[] indexes) GetRightVertex(EntityModelUVData uv, Vector2 size)
			{
				//Vector3 normal = new Vector3(1.0f, 0.0f, 0.0f) * Size;
				Color normal = AdjustColor(DefaultColor, BlockFace.East);
				
				var map = GetTextureMapping(uv.Origin + size, uv.Size.Value.X, uv.Size.Value.Y);
				//var map = GetTextureMapping(uv + new Vector2(0, Size.Z), Size.Z, Size.Y);

				// Add the vertices for the RIGHT face. 
				return (new VertexPositionColorTexture[]
				{
					new VertexPositionColorTexture(_topRightFront, normal, map.TopLeft),
					new VertexPositionColorTexture(_btmRightBack , normal, map.BotRight),
					new VertexPositionColorTexture(_btmRightFront, normal, map.BotLeft),
					new VertexPositionColorTexture(_topRightBack , normal, map.TopRight),
					//new VertexPositionNormalTexture(_btmRightBack , normal, map.BotLeft),
					//new VertexPositionNormalTexture(_topRightFront, normal, map.TopRight),
				}, new short[]
				{
					0, 1, 2, 
					3, 1, 0
				});
			}

			private (VertexPositionColorTexture[] vertices, short[] indexes) GetFrontVertex(EntityModelUVData uv, Vector2 size)
			{
				//Vector3 normal = new Vector3(0.0f, 0.0f, 1.0f) * Size;
				Color normal = AdjustColor(DefaultColor, BlockFace.South);
				
				var map = GetTextureMapping(uv.Origin + size, uv.Size.Value.X, uv.Size.Value.Y);

				// Add the vertices for the RIGHT face. 
				return (new VertexPositionColorTexture[]
				{
					new VertexPositionColorTexture(_topLeftFront , normal, map.TopLeft),
					new VertexPositionColorTexture(_topRightFront, normal, map.TopRight),
					new VertexPositionColorTexture(_btmLeftFront , normal, map.BotLeft),
					//new VertexPositionNormalTexture(_btmLeftFront , normal, map.BotLeft),
					//new VertexPositionNormalTexture(_topRightFront, normal, map.TopRight),
					new VertexPositionColorTexture(_btmRightFront, normal, map.BotRight),
				}, new short[]
				{
					0, 1, 2, 
					2, 1, 3
					//0, 2, 1, 2, 3, 1
				});
			}
			private (VertexPositionColorTexture[] vertices, short[] indexes) GetBackVertex(EntityModelUVData uv, Vector2 size)
			{
				//Vector3 normal = new Vector3(0.0f, 0.0f, -1.0f) * Size;
				Color   normal = AdjustColor(DefaultColor, BlockFace.North);
				
				var map = GetTextureMapping(uv.Origin + size, uv.Size.Value.X, uv.Size.Value.Y);
				// Add the vertices for the RIGHT face. 
				return (new VertexPositionColorTexture[]
				{
					new VertexPositionColorTexture(_topLeftBack , normal, map.TopRight),
					new VertexPositionColorTexture(_btmLeftBack , normal, map.BotRight),
					new VertexPositionColorTexture(_topRightBack, normal, map.TopLeft),
					//new VertexPositionNormalTexture(_btmLeftBack , normal, map.BotRight),
					new VertexPositionColorTexture(_btmRightBack, normal, map.BotLeft),
					//new VertexPositionNormalTexture(_topRightBack, normal, map.TopLeft),
				}, new short[]
				{
					0, 1, 2,
					1, 3, 2
					//0, 1, 2, 1, 3, 2
				});
			}

			private (VertexPositionColorTexture[] vertices, short[] indexes) GetTopVertex(EntityModelUVData uv, Vector2 size)
			{
			//	Vector3 normal = new Vector3(0.0f, 1.0f, 0.0f) * Size;
			Color   normal = AdjustColor(DefaultColor, BlockFace.Up);
			
				var map    = GetTextureMapping(uv.Origin + size, uv.Size.Value.X, uv.Size.Value.Y);

				// Add the vertices for the RIGHT face. 
				return (new VertexPositionColorTexture[]
				{
					new VertexPositionColorTexture(_topLeftFront , normal, map.BotLeft),
					new VertexPositionColorTexture(_topLeftBack  , normal, map.TopLeft),
					new VertexPositionColorTexture(_topRightBack , normal, map.TopRight),
					//new VertexPositionNormalTexture(_topLeftFront , normal, map.BotLeft),
				//	new VertexPositionNormalTexture(_topRightBack , normal, map.TopRight),
					new VertexPositionColorTexture(_topRightFront, normal, map.BotRight),
				}, new short[]
				{
					0, 1, 2, 
					0, 2, 3
				});
			}

			private (VertexPositionColorTexture[] vertices, short[] indexes) GetBottomVertex(EntityModelUVData uv, Vector2 size)
			{
			//	Vector3 normal = new Vector3(0.0f, -1.0f, 0.0f) * Size;
			Color normal = AdjustColor(DefaultColor, BlockFace.Down);
			var   map    = GetTextureMapping(uv.Origin + size, uv.Size.Value.X, uv.Size.Value.Y);

				// Add the vertices for the RIGHT face. 
				return (new VertexPositionColorTexture[]
				{
					new VertexPositionColorTexture(_btmLeftFront , normal, map.TopLeft),
					new VertexPositionColorTexture(_btmRightBack , normal, map.BotRight),
					new VertexPositionColorTexture(_btmLeftBack  , normal, map.BotLeft),
					//new VertexPositionNormalTexture(_btmLeftFront , normal, map.TopLeft),
					new VertexPositionColorTexture(_btmRightFront, normal, map.TopRight),
					//new VertexPositionNormalTexture(_btmRightBack , normal, map.BotRight),
				}, new short[]
				{
					0, 1, 2, 
					0, 3, 1
				});
			}

			private TextureMapping GetTextureMapping(Vector2 textureOffset, float regionWidth, float regionHeight)
			{
				return new TextureMapping(new Vector2(_model.Description.TextureWidth, _model.Description.TextureHeight), _textureSize, textureOffset, regionWidth, regionHeight, _mirror);
			}

			private class TextureMapping
			{
				public Vector2 TopLeft { get; }
				public Vector2 TopRight { get; }
				public Vector2 BotLeft { get; }
				public Vector2 BotRight { get; }

				public TextureMapping(Vector2 modelTexture,
					Vector2 textureSize,
					Vector2 textureOffset,
					float width,
					float height,
					bool mirrored)
				{
					var texelWidth  = (1f / textureSize.X);
					var texelHeight = (1f / textureSize.Y);

					var x1 = texelWidth * textureOffset.X;
					var x2 = x1 + (width * texelWidth);
					var y1 = texelHeight * (textureOffset.Y);
					var y2 = y1 + (height * texelHeight);

					if (mirrored)
					{
						TopLeft = new Vector2(x2, y1);
						TopRight = new Vector2(x1, y1);
						BotLeft = new Vector2(x2, y2);
						BotRight = new Vector2(x1, y2);
					}
					else
					{
						TopLeft = new Vector2(x1, y1);
						TopRight = new Vector2(x2, y1);
						BotLeft = new Vector2(x1, y2);
						BotRight = new Vector2(x2, y2);
					}
				}
			}
		}
	}
}
