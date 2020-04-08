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
            WhiteTexture.SetData(new Color[] { Color.White });
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
			Color color)
		{
			if (effect == null)
			{
				effect = new BasicEffect(sb.GraphicsDevice)
				{
					VertexColorEnabled = true,
					LightingEnabled = false
				};
			}

			var corners = box.GetCorners();
			for (var i = 0; i < 8; i++)
			{
				verts[i].Position = corners[i];
				verts[i].Color = color;
			}

			effect.View = view;
			effect.Projection = projection;

			foreach (var pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();

				sb.GraphicsDevice.DrawUserIndexedPrimitives(
					PrimitiveType.LineList,
					verts,
					0,
					8,
					indices,
					0,
					indices.Length / 2);
			}
		}

		#region Fields

		private static readonly VertexPositionColor[] verts = new VertexPositionColor[8];

		private static readonly short[] indices =
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

		private static BasicEffect effect;
		private static VertexDeclaration vertDecl;

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