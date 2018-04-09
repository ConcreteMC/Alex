using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models
{
    public abstract class Model
    {
	    protected static VertexPositionNormalTextureColor[] QuadVertices { get; private set; } = null;
	    protected static VertexPositionNormalTextureColor[] CubeVertices { get; private set; } = null;

	    static Model()
	    {
			if (QuadVertices == null)
				CalculateQuad();

		    if (CubeVertices == null)
			    CalculateCube();
	    }

	    private static void CalculateQuad()
	    {
		    QuadVertices = new VertexPositionNormalTextureColor[4];
		    QuadVertices[0] = new VertexPositionNormalTextureColor();
		    QuadVertices[0].Position = new Vector3(-1, 1, 0);
		    QuadVertices[0].Color = Color.White;
		    QuadVertices[1] = new VertexPositionNormalTextureColor();
		    QuadVertices[1].Position = new Vector3(1, 1, 0);
		    QuadVertices[1].Color = Color.White;
		    QuadVertices[2] = new VertexPositionNormalTextureColor();
		    QuadVertices[2].Position = new Vector3(-1, -1, 0);
		    QuadVertices[2].Color = Color.White;
		    QuadVertices[3] = new VertexPositionNormalTextureColor();
		    QuadVertices[3].Position = new Vector3(1, -1, 0);
		    QuadVertices[3].Color = Color.White;
		}

	    private static void CalculateCube()
	    {

	    }

		protected class Cube
		{
			public Vector3 Size;

			private readonly Vector2 _textureSize;

			public Cube(Vector3 size, Vector2 textureSize)
			{
				this.Size = size;
				this._textureSize = textureSize; //new Vector2((size.X + size.Z) * 2, size.Y + size.Z);

				//front verts with position and texture stuff
				_topLeftFront = new Vector3(0.0f, 1.0f, 0.0f) * Size;
				_topLeftBack = new Vector3(0.0f, 1.0f, 1.0f) * Size;
				_topRightFront = new Vector3(1.0f, 1.0f, 0.0f) * Size;
				_topRightBack = new Vector3(1.0f, 1.0f, 1.0f) * Size;

				// Calculate the position of the vertices on the bottom face.
				_btmLeftFront = new Vector3(0.0f, 0.0f, 0.0f) * Size;
				_btmLeftBack = new Vector3(0.0f, 0.0f, 1.0f) * Size;
				_btmRightFront = new Vector3(1.0f, 0.0f, 0.0f) * Size;
				_btmRightBack = new Vector3(1.0f, 0.0f, 1.0f) * Size;
			}

			public VertexPositionNormalTexture[] Front, Back, Left, Right, Top, Bottom;

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

			private VertexPositionNormalTexture[] GetLeftVertex(Vector2 uv)
			{
				Vector3 normal = new Vector3(-1.0f, 0.0f, 0.0f) * Size;

				var map = GetTextureMapping(uv + new Vector2(Size.Z + Size.X, Size.Z), Size.Z, Size.Y);

				// Add the vertices for the RIGHT face. 
				return new VertexPositionNormalTexture[]
				{
					new VertexPositionNormalTexture(_topLeftFront, normal, map.TopLeft),
					new VertexPositionNormalTexture(_btmLeftFront , normal, map.BotLeft),
					new VertexPositionNormalTexture(_btmLeftBack, normal, map.BotRight),
					new VertexPositionNormalTexture(_topLeftBack , normal, map.TopRight),
					new VertexPositionNormalTexture(_topLeftFront , normal, map.TopLeft),
					new VertexPositionNormalTexture(_btmLeftBack, normal, map.BotRight),
				};
			}

			private VertexPositionNormalTexture[] GetRightVertex(Vector2 uv)
			{
				Vector3 normal = new Vector3(1.0f, 0.0f, 0.0f) * Size;

				var map = GetTextureMapping(uv + new Vector2(0, Size.Z), Size.Z, Size.Y);

				// Add the vertices for the RIGHT face. 
				return new VertexPositionNormalTexture[]
				{
					new VertexPositionNormalTexture(_topRightFront, normal, map.TopRight),
					new VertexPositionNormalTexture(_btmRightBack , normal, map.BotLeft),
					new VertexPositionNormalTexture(_btmRightFront, normal, map.BotRight),
					new VertexPositionNormalTexture(_topRightBack , normal, map.TopLeft),
					new VertexPositionNormalTexture(_btmRightBack , normal, map.BotLeft),
					new VertexPositionNormalTexture(_topRightFront, normal, map.TopRight),
				};
			}

			private VertexPositionNormalTexture[] GetFrontVertex(Vector2 uv)
			{
				Vector3 normal = new Vector3(0.0f, 0.0f, 1.0f) * Size;

				var map = GetTextureMapping(uv + new Vector2(Size.Z, Size.Z), Size.X, Size.Y);

				// Add the vertices for the RIGHT face. 
				return new VertexPositionNormalTexture[]
				{
					new VertexPositionNormalTexture(_topLeftFront , normal, map.TopLeft),
					new VertexPositionNormalTexture(_topRightFront, normal, map.TopRight),
					new VertexPositionNormalTexture(_btmLeftFront , normal, map.BotLeft),
					new VertexPositionNormalTexture(_btmLeftFront , normal, map.BotLeft),
					new VertexPositionNormalTexture(_topRightFront, normal, map.TopRight),
					new VertexPositionNormalTexture(_btmRightFront, normal, map.BotRight),
				};
			}
			private VertexPositionNormalTexture[] GetBackVertex(Vector2 uv)
			{
				Vector3 normal = new Vector3(0.0f, 0.0f, -1.0f) * Size;

				var map = GetTextureMapping(uv + new Vector2(Size.Z + Size.Z + Size.X, Size.Z), Size.X, Size.Y);

				// Add the vertices for the RIGHT face. 
				return new VertexPositionNormalTexture[]
				{
					new VertexPositionNormalTexture(_topLeftBack , normal, map.TopRight),
					new VertexPositionNormalTexture(_btmLeftBack , normal, map.BotRight),
					new VertexPositionNormalTexture(_topRightBack, normal, map.TopLeft),
					new VertexPositionNormalTexture(_btmLeftBack , normal, map.BotRight),
					new VertexPositionNormalTexture(_btmRightBack, normal, map.BotLeft),
					new VertexPositionNormalTexture(_topRightBack, normal, map.TopLeft),
				};
			}

			private VertexPositionNormalTexture[] GetTopVertex(Vector2 uv)
			{
				Vector3 normal = new Vector3(0.0f, 1.0f, 0.0f) * Size;

				var map = GetTextureMapping(uv + new Vector2(Size.Z, 0), Size.X, Size.Z);

				// Add the vertices for the RIGHT face. 
				return new VertexPositionNormalTexture[]
				{
					new VertexPositionNormalTexture(_topLeftFront , normal, map.BotLeft),
					new VertexPositionNormalTexture(_topLeftBack  , normal, map.TopLeft),
					new VertexPositionNormalTexture(_topRightBack , normal, map.TopRight),
					new VertexPositionNormalTexture(_topLeftFront , normal, map.BotLeft),
					new VertexPositionNormalTexture(_topRightBack , normal, map.TopRight),
					new VertexPositionNormalTexture(_topRightFront, normal, map.BotRight),
				};
			}

			private VertexPositionNormalTexture[] GetBottomVertex(Vector2 uv)
			{
				Vector3 normal = new Vector3(0.0f, -1.0f, 0.0f) * Size;

				var map = GetTextureMapping(uv + new Vector2(Size.Z + Size.X, 0), Size.X, Size.Z);

				// Add the vertices for the RIGHT face. 
				return new VertexPositionNormalTexture[]
				{
					new VertexPositionNormalTexture(_btmLeftFront , normal, map.TopLeft),
					new VertexPositionNormalTexture(_btmRightBack , normal, map.BotRight),
					new VertexPositionNormalTexture(_btmLeftBack  , normal, map.BotLeft),
					new VertexPositionNormalTexture(_btmLeftFront , normal, map.TopLeft),
					new VertexPositionNormalTexture(_btmRightFront, normal, map.TopRight),
					new VertexPositionNormalTexture(_btmRightBack , normal, map.BotRight),
				};
			}

			private TextureMapping GetTextureMapping(Vector2 textureOffset, float regionWidth, float regionHeight)
			{
				return new TextureMapping(_textureSize, textureOffset, regionWidth, regionHeight);
			}

			private class TextureMapping
			{
				public Vector2 TopLeft { get; }
				public Vector2 TopRight { get; }
				public Vector2 BotLeft { get; }
				public Vector2 BotRight { get; }

				public TextureMapping(Vector2 textureSize, Vector2 textureOffset, float width, float height)
				{
					var pixelWidth = (1f / textureSize.X);
					var pixelHeight = (1f / textureSize.Y);

					var x1 = pixelWidth * textureOffset.X;
					var x2 = pixelWidth * (textureOffset.X + width);
					var y1 = pixelHeight * textureOffset.Y;
					var y2 = pixelHeight * (textureOffset.Y + height);

					TopLeft = new Vector2(x1, y1);
					TopRight = new Vector2(x2, y1);
					BotLeft = new Vector2(x1, y2);
					BotRight = new Vector2(x2, y2);
				}
			}
		}
	}
}
