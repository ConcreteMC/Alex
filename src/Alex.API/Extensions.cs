using System;
using Alex.API.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API
{
    public static class Extensions
    {
	    private static Texture2D WhiteTexture { get; set; }

	    public static void Init(GraphicsDevice gd)
	    {
		    WhiteTexture = GpuResourceManager.GetTexture2D("Alex.Extensions", gd, 1, 1);
		    WhiteTexture.SetData(new Color[] {Color.White});

		    var size = Vector3.One;
		    
		    // Calculate the position of the vertices on the top face.
		    Vector3 topLeftFront = new Vector3(size.X, size.Y, size.Z);
		    Vector3 topLeftBack = new Vector3(0, size.Y, size.Z);
		    Vector3 topRightFront = new Vector3(size.X, size.Y, 0);
		    Vector3 topRightBack = new Vector3(0, size.Y, 0);

		    // Calculate the position of the vertices on the bottom face.
		    Vector3 btmLeftFront = new Vector3(size.X, 0, size.Z);
		    Vector3 btmLeftBack = new Vector3(0, 0, size.Z);
		    Vector3 btmRightFront = new Vector3(size.X, 0, 0);
		    Vector3 btmRightBack = Vector3.Zero;

		    // UV texture coordinates
		    Color textureTopLeft = Color.White;
		    Color textureTopRight = Color.White;
		    Color textureBottomLeft = Color.White;
		    Color textureBottomRight = Color.White;

		    // Add the vertices for the FRONT face.
		    CubeVertices[0] = new VertexPositionColor(topLeftFront,textureTopLeft);
		    CubeVertices[1] = new VertexPositionColor(btmLeftFront,  textureBottomLeft);
		    CubeVertices[2] = new VertexPositionColor(topRightFront, textureTopRight);
		    CubeVertices[3] = new VertexPositionColor(btmLeftFront,  textureBottomLeft);
		    CubeVertices[4] = new VertexPositionColor(btmRightFront,textureBottomRight);
		    CubeVertices[5] = new VertexPositionColor(topRightFront, textureTopRight);

		    // Add the vertices for the BACK face.
		    CubeVertices[6] = new VertexPositionColor(topLeftBack, textureTopRight);
		    CubeVertices[7] = new VertexPositionColor(topRightBack,  textureTopLeft);
		    CubeVertices[8] = new VertexPositionColor(btmLeftBack, textureBottomRight);
		    CubeVertices[9] = new VertexPositionColor(btmLeftBack, textureBottomRight);
		    CubeVertices[10] = new VertexPositionColor(topRightBack, textureTopLeft);
		    CubeVertices[11] = new VertexPositionColor(btmRightBack,  textureBottomLeft);

		    // Add the vertices for the TOP face.
		    CubeVertices[12] = new VertexPositionColor(topLeftFront, textureBottomLeft);
		    CubeVertices[13] = new VertexPositionColor(topRightBack,  textureTopRight);
		    CubeVertices[14] = new VertexPositionColor(topLeftBack, textureTopLeft);
		    CubeVertices[15] = new VertexPositionColor(topLeftFront, textureBottomLeft);
		    CubeVertices[16] = new VertexPositionColor(topRightFront, textureBottomRight);
		    CubeVertices[17] = new VertexPositionColor(topRightBack,textureTopRight);

		    // Add the vertices for the BOTTOM face. 
		    CubeVertices[18] = new VertexPositionColor(btmLeftFront,  textureTopLeft);
		    CubeVertices[19] = new VertexPositionColor(btmLeftBack,  textureBottomLeft);
		    CubeVertices[20] = new VertexPositionColor(btmRightBack,  textureBottomRight);
		    CubeVertices[21] = new VertexPositionColor(btmLeftFront,  textureTopLeft);
		    CubeVertices[22] = new VertexPositionColor(btmRightBack,  textureBottomRight);
		    CubeVertices[23] = new VertexPositionColor(btmRightFront,  textureTopRight);

		    // Add the vertices for the LEFT face.
		    CubeVertices[24] = new VertexPositionColor(topLeftFront, textureTopRight);
		    CubeVertices[25] = new VertexPositionColor(btmLeftBack, textureBottomLeft);
		    CubeVertices[26] = new VertexPositionColor(btmLeftFront, textureBottomRight);
		    CubeVertices[27] = new VertexPositionColor(topLeftBack, textureTopLeft);
		    CubeVertices[28] = new VertexPositionColor(btmLeftBack, textureBottomLeft);
		    CubeVertices[29] = new VertexPositionColor(topLeftFront, textureTopRight);

		    // Add the vertices for the RIGHT face. 
		    CubeVertices[30] = new VertexPositionColor(topRightFront, textureTopLeft);
		    CubeVertices[31] = new VertexPositionColor(btmRightFront, textureBottomLeft);
		    CubeVertices[32] = new VertexPositionColor(btmRightBack, textureBottomRight);
		    CubeVertices[33] = new VertexPositionColor(topRightBack, textureTopRight);
		    CubeVertices[34] = new VertexPositionColor(topRightFront, textureTopLeft);
		    CubeVertices[35] = new VertexPositionColor(btmRightBack, textureBottomRight);
	    }

	    /// <summary>
        /// Draw a line between the two supplied points.
        /// </summary>
        /// <param name="start">Starting point.</param>
        /// <param name="end">End point.</param>
        /// <param name="color">The draw color.</param>
        public static void DrawLine(this SpriteBatch sb, float thickness, Vector2 start, Vector2 end, Color color, Vector2 scale, float layerdepth)
		{
			float length = (end - start).Length();
			float rotation = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
			sb.Draw(WhiteTexture, start, null, color, rotation, Vector2.Zero, new Vector2(scale.X * length, scale.Y * (thickness)), SpriteEffects.None, layerdepth);
		}

		/// <summary>
		/// Draw a rectangle.
		/// </summary>
		/// <param name="rectangle">The rectangle to draw.</param>
		/// <param name="color">The draw color.</param>
		public static void DrawRectangle(this SpriteBatch sb, Rectangle rectangle, Color color)
		{
			sb.Draw(WhiteTexture, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, 1), color);
			sb.Draw(WhiteTexture, new Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, 1), color);
			sb.Draw(WhiteTexture, new Rectangle(rectangle.Left, rectangle.Top, 1, rectangle.Height), color);
			sb.Draw(WhiteTexture, new Rectangle(rectangle.Right, rectangle.Top, 1, rectangle.Height + 1), color);
		}

		/// <summary>
		/// Fill a rectangle.
		/// </summary>
		/// <param name="rectangle">The rectangle to fill.</param>
		/// <param name="color">The fill color.</param>
		public static void FillRectangle(this SpriteBatch sb, Rectangle rectangle, Color color)
		{
			sb.Draw(WhiteTexture, rectangle, color);
		}
		
		public static void FillRectangle(this SpriteBatch sb, Rectangle rectangle, Color color, float layerDepth)
		{
			sb.Draw(WhiteTexture, rectangle, null, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
		}

		public static void RenderBoundingBox(
			this SpriteBatch sb,
			BoundingBox box,
			Matrix view,
			Matrix projection,
			Color color, bool asCube = false)
		{
			if (_effect == null)
			{
				_effect = new BasicEffect(sb.GraphicsDevice)
				{
					VertexColorEnabled = true,
					FogEnabled = false,
					DiffuseColor = Color.White.ToVector3(),
					LightingEnabled = false
				};
			}

			var corners = box.GetCorners();
			for (var i = 0; i < 8; i++)
			{
				Verts[i].Position = corners[i];
				Verts[i].Color = color;
			}

			_effect.View = view;
			_effect.Projection = projection;
			
			var oldDiffuse = _effect.DiffuseColor;
			//effect.DiffuseColor = color.ToVector3();

			var alpha = _effect.Alpha;
			if (asCube)
			{
				for (var index = 0; index < CubeVertices.Length; index++)
				{
				//	var vert = CubeVertices[index];
					CubeVertices[index].Color = color;
				}

				const float inflation = 0.025f;

				var x = (box.Max.X - box.Min.X) + inflation;
				var y = (box.Max.Y - box.Min.Y) + inflation;
				var z = (box.Max.Z - box.Min.Z) + inflation;
				_effect.World = Matrix.CreateScale(new Vector3(x,y,z)) * Matrix.CreateTranslation(box.Min - new Vector3(inflation / 2f));
				_effect.Alpha = 0.5f;
			}

			foreach (var pass in _effect.CurrentTechnique.Passes)
			{
				pass.Apply();

				if (!asCube)
				{
					sb.GraphicsDevice.DrawUserIndexedPrimitives(
						PrimitiveType.LineList,
						Verts,
						0,
						8,
						Indices,
						0,
						Indices.Length / 2);
				}
				else
				{
					sb.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, CubeVertices, 0, CubeVertices.Length / 3, VertexPositionColor.VertexDeclaration);
				}
			}

			_effect.DiffuseColor = oldDiffuse;
			_effect.World = Matrix.Identity;
			_effect.Alpha = alpha;
		}

		#region Fields

		private static readonly VertexPositionColor[] CubeVertices = new VertexPositionColor[36];
		
		private static readonly VertexPositionColor[] Verts = new VertexPositionColor[8];

		private static readonly short[] Indices =
		{
			0, 1,
			1, 2,
			2, 3,
			3, 0,
			0, 4,
			1, 5,
			2, 6,
			3, 7,
			4, 5,
			5, 6,
			6, 7,
			7, 4
		};

		private static BasicEffect _effect;
		private static VertexDeclaration _vertDecl;

		#endregion
		
        public static string GetBytesReadable(long i)
        {
            // Get absolute value
            long absolute_i = (i < 0 ? -i : i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);
            // Return formatted number with suffix
            return readable.ToString("0.### ") + suffix;
        }
    }
}