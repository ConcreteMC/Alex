using Alex.API.Blocks;
using Alex.ResourcePackLib.Json;
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

	    protected Color AdjustColor(Color color, BlockFace face, bool shade = true)
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
			public Vector3 Size;

			private readonly Vector2 _textureSize;

			public bool Mirrored { get; set; } = false;
			public Cube(Vector3 size, Vector2 textureSize)
			{
				this.Size = size;
				this._textureSize = textureSize; //new Vector2((size.X + size.Z) * 2, size.Y + size.Z);

				//front verts with position and texture stuff
				_topLeftFront = new Vector3(0.0f, 1.0f, 0.0f) * Size;
				_topLeftBack = new Vector3(0.0f, 1.0f, 1.0f) * Size;
				
				_topRightFront = new Vector3(1.0f, 1.0f, 0.0f) * Size;
				_topRightBack = new Vector3(1.0f, 1.0f, 1.0f) * Size;
				
				/*
				 * _topLeftFront = new Vector3(0.0f, 1.0f, 0.0f) * Size;
				_topLeftBack = new Vector3(0.0f, 1.0f, 1.0f) * Size;
				
				_topRightFront = new Vector3(1.0f, 1.0f, 0.0f) * Size;
				_topRightBack = new Vector3(1.0f, 1.0f, 1.0f) * Size;
				 */

				// Calculate the position of the vertices on the bottom face.
				_btmLeftFront = new Vector3(0.0f, 0.0f, 0.0f) * Size;
				_btmLeftBack = new Vector3(0.0f, 0.0f, 1.0f) * Size;
				_btmRightFront = new Vector3(1.0f, 0.0f, 0.0f) * Size;
				_btmRightBack = new Vector3(1.0f, 0.0f, 1.0f) * Size;
			}

			public (VertexPositionNormalTexture[] vertices, short[] indexes) Front, Back, Left, Right, Top, Bottom;

			private readonly Vector3 _topLeftFront;
			private readonly Vector3 _topLeftBack;
			private readonly Vector3 _topRightFront;
			private readonly Vector3 _topRightBack;
			private readonly Vector3 _btmLeftFront;
			private readonly Vector3 _btmLeftBack;
			private readonly Vector3 _btmRightFront;
			private readonly Vector3 _btmRightBack;

			public void BuildCube(Vector2 uv)
			{
				Front = GetFrontVertex(uv);
				Back = GetBackVertex(uv);
				Left = GetLeftVertex(uv);
				Right = GetRightVertex(uv);
				Top = GetTopVertex(uv);
				Bottom = GetBottomVertex(uv);
			}

			public void BuildCube(Vector2 front, Vector2 back, Vector2 left, Vector2 right, Vector2 top, Vector2 bottom)
			{
				Front = GetFrontVertex(front);
				Back = GetBackVertex(back);
				Left = GetLeftVertex(left);
				Right = GetRightVertex(right);
				Top = GetTopVertex(top);
				Bottom = GetBottomVertex(bottom);
			}

			private (VertexPositionNormalTexture[] vertices, short[] indexes) GetLeftVertex(Vector2 uv)
			{
				Vector3 normal = new Vector3(-1.0f, 0.0f, 0.0f) * Size;

				//var map = GetTextureMapping(uv + new Vector2(Size.Z + Size.X, Size.Z), Size.Z, Size.Y);
				var map = GetTextureMapping(uv + new Vector2(0, Size.Z), Size.Z, Size.Y);
				
				// Add the vertices for the RIGHT face. 
				return (new VertexPositionNormalTexture[]
				{
					new VertexPositionNormalTexture(_topLeftFront, normal, map.TopRight),
					new VertexPositionNormalTexture(_btmLeftFront , normal, map.BotRight),
					new VertexPositionNormalTexture(_btmLeftBack, normal, map.BotLeft),
					new VertexPositionNormalTexture(_topLeftBack , normal, map.TopLeft),
					//new VertexPositionNormalTexture(_topLeftFront , normal, map.TopLeft),
					//new VertexPositionNormalTexture(_btmLeftBack, normal, map.BotRight),
				}, new short[]
				{
					0, 1, 2, 
					3, 0, 2
					//0, 1, 2, 3, 0, 2
				});
			}

			private (VertexPositionNormalTexture[] vertices, short[] indexes) GetRightVertex(Vector2 uv)
			{
				Vector3 normal = new Vector3(1.0f, 0.0f, 0.0f) * Size;

				var map = GetTextureMapping(uv + new Vector2(Size.Z + Size.X, Size.Z), Size.Z, Size.Y);
				//var map = GetTextureMapping(uv + new Vector2(0, Size.Z), Size.Z, Size.Y);

				// Add the vertices for the RIGHT face. 
				return (new VertexPositionNormalTexture[]
				{
					new VertexPositionNormalTexture(_topRightFront, normal, map.TopLeft),
					new VertexPositionNormalTexture(_btmRightBack , normal, map.BotRight),
					new VertexPositionNormalTexture(_btmRightFront, normal, map.BotLeft),
					new VertexPositionNormalTexture(_topRightBack , normal, map.TopRight),
					//new VertexPositionNormalTexture(_btmRightBack , normal, map.BotLeft),
					//new VertexPositionNormalTexture(_topRightFront, normal, map.TopRight),
				}, new short[]
				{
					0, 1, 2, 
					3, 1, 0
				});
			}

			private (VertexPositionNormalTexture[] vertices, short[] indexes) GetFrontVertex(Vector2 uv)
			{
				Vector3 normal = new Vector3(0.0f, 0.0f, 1.0f) * Size;

				var map = GetTextureMapping(uv + new Vector2(Size.Z, Size.Z), Size.X, Size.Y);

				// Add the vertices for the RIGHT face. 
				return (new VertexPositionNormalTexture[]
				{
					new VertexPositionNormalTexture(_topLeftFront , normal, map.TopLeft),
					new VertexPositionNormalTexture(_topRightFront, normal, map.TopRight),
					new VertexPositionNormalTexture(_btmLeftFront , normal, map.BotLeft),
					//new VertexPositionNormalTexture(_btmLeftFront , normal, map.BotLeft),
					//new VertexPositionNormalTexture(_topRightFront, normal, map.TopRight),
					new VertexPositionNormalTexture(_btmRightFront, normal, map.BotRight),
				}, new short[]
				{
					0, 1, 2, 
					2, 1, 3
					//0, 2, 1, 2, 3, 1
				});
			}
			private (VertexPositionNormalTexture[] vertices, short[] indexes) GetBackVertex(Vector2 uv)
			{
				Vector3 normal = new Vector3(0.0f, 0.0f, -1.0f) * Size;

				var map = GetTextureMapping(uv + new Vector2(Size.Z + Size.Z + Size.X, Size.Z), Size.X, Size.Y);

				// Add the vertices for the RIGHT face. 
				return (new VertexPositionNormalTexture[]
				{
					new VertexPositionNormalTexture(_topLeftBack , normal, map.TopRight),
					new VertexPositionNormalTexture(_btmLeftBack , normal, map.BotRight),
					new VertexPositionNormalTexture(_topRightBack, normal, map.TopLeft),
					//new VertexPositionNormalTexture(_btmLeftBack , normal, map.BotRight),
					new VertexPositionNormalTexture(_btmRightBack, normal, map.BotLeft),
					//new VertexPositionNormalTexture(_topRightBack, normal, map.TopLeft),
				}, new short[]
				{
					0, 1, 2,
					1, 3, 2
					//0, 1, 2, 1, 3, 2
				});
			}

			private (VertexPositionNormalTexture[] vertices, short[] indexes) GetTopVertex(Vector2 uv)
			{
				Vector3 normal = new Vector3(0.0f, 1.0f, 0.0f) * Size;

				var map = GetTextureMapping(uv + new Vector2(Size.Z, 0), Size.X, Size.Z);

				// Add the vertices for the RIGHT face. 
				return (new VertexPositionNormalTexture[]
				{
					new VertexPositionNormalTexture(_topLeftFront , normal, map.BotLeft),
					new VertexPositionNormalTexture(_topLeftBack  , normal, map.TopLeft),
					new VertexPositionNormalTexture(_topRightBack , normal, map.TopRight),
					//new VertexPositionNormalTexture(_topLeftFront , normal, map.BotLeft),
				//	new VertexPositionNormalTexture(_topRightBack , normal, map.TopRight),
					new VertexPositionNormalTexture(_topRightFront, normal, map.BotRight),
				}, new short[]
				{
					0, 1, 2, 
					0, 2, 3
				});
			}

			private (VertexPositionNormalTexture[] vertices, short[] indexes) GetBottomVertex(Vector2 uv)
			{
				Vector3 normal = new Vector3(0.0f, -1.0f, 0.0f) * Size;

				var map = GetTextureMapping(uv + new Vector2(Size.Z + Size.X, 0), Size.X, Size.Z);

				// Add the vertices for the RIGHT face. 
				return (new VertexPositionNormalTexture[]
				{
					new VertexPositionNormalTexture(_btmLeftFront , normal, map.TopLeft),
					new VertexPositionNormalTexture(_btmRightBack , normal, map.BotRight),
					new VertexPositionNormalTexture(_btmLeftBack  , normal, map.BotLeft),
					//new VertexPositionNormalTexture(_btmLeftFront , normal, map.TopLeft),
					new VertexPositionNormalTexture(_btmRightFront, normal, map.TopRight),
					//new VertexPositionNormalTexture(_btmRightBack , normal, map.BotRight),
				}, new short[]
				{
					0, 1, 2, 
					0, 3, 1
				});
			}

			private TextureMapping GetTextureMapping(Vector2 textureOffset, float regionWidth, float regionHeight)
			{
				return new TextureMapping(_textureSize, textureOffset, regionWidth, regionHeight, Mirrored);
			}

			private class TextureMapping
			{
				public Vector2 TopLeft { get; }
				public Vector2 TopRight { get; }
				public Vector2 BotLeft { get; }
				public Vector2 BotRight { get; }

				public TextureMapping(Vector2 textureSize, Vector2 textureOffset, float width, float height, bool mirrored)
				{
					var pixelWidth = (1f / textureSize.X);
					var pixelHeight = (1f / textureSize.Y);

					var x1 = pixelWidth * textureOffset.X;
					var x2 = pixelWidth * (textureOffset.X + width);
					var y1 = pixelHeight * textureOffset.Y;
					var y2 = pixelHeight * (textureOffset.Y + height);

					/*if (mirrored)
					{
						TopLeft = new Vector2(x2, y1);
						TopRight = new Vector2(x1, y1);
						BotLeft = new Vector2(x2, y2);
						BotRight = new Vector2(x1, y2);
					}
					else
					{*/
						TopLeft = new Vector2(x1, y1);
						TopRight = new Vector2(x2, y1);
						BotLeft = new Vector2(x1, y2);
						BotRight = new Vector2(x2, y2);
					//}
				}
			}
		}
	}
}
