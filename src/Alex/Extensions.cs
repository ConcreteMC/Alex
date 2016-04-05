using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex
{
	public static class Extensions
	{
		private static Texture2D WhiteTexture { get; set; }

		static Extensions()
		{
			WhiteTexture = new Texture2D(Alex.Instance.GraphicsDevice, 1, 1);
			WhiteTexture.SetData(new Color[] { Color.White });
		}

		/// <summary>
		/// Draw a line between the two supplied points.
		/// </summary>
		/// <param name="start">Starting point.</param>
		/// <param name="end">End point.</param>
		/// <param name="color">The draw color.</param>
		public static void DrawLine(this SpriteBatch sb, Vector2 start, Vector2 end, Color color)
		{
			float length = (end - start).Length();
			float rotation = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
			sb.Draw(WhiteTexture, start, null, color, rotation, Vector2.Zero, new Vector2(length, 1), SpriteEffects.None, 0);
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
	}
}
