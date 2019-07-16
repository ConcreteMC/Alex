using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Alex.API.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiNET;

namespace Alex
{
	public static class Extensions
	{
		private static Texture2D WhiteTexture { get; set; }

		static Extensions()
		{
			
		}

		public static byte[] ReadAllBytes(this Stream reader)
		{
			const int bufferSize = 4096;
			using (var ms = new MemoryStream())
			{
				byte[] buffer = new byte[bufferSize];
				int count;
				while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
					ms.Write(buffer, 0, count);
				return ms.ToArray();
			}

		}

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

		public static BoundingBox OffsetBy(this BoundingBox box, Vector3 offset)
		{
			box.Min += offset;
			box.Max += offset;
			return box;
		}

	    public static Vector3 Floor(this Vector3 toFloor)
	    {
	        return new Vector3((float)Math.Floor(toFloor.X), (float)Math.Floor(toFloor.Y), (float)Math.Floor(toFloor.Z));
	    }

		public static BlockFace GetBlockFace(this Vector3 vector)
		{
			BlockFace face = BlockFace.None;

			if (vector == Vector3.Up)
			{
				face = BlockFace.Up;
			}
			else if (vector == Vector3.Down)
			{
				face = BlockFace.Down;
			}
			else if (vector == Vector3.Backward)
			{
				face = BlockFace.South;
			}
			else if (vector == Vector3.Forward)
			{
				face = BlockFace.North;
			}
			else if (vector == Vector3.Left)
			{
				face = BlockFace.West;
			}
			else if (vector == Vector3.Right)
			{
				face = BlockFace.East;
			}

			return face;
		}

		public static void Fill<TType>(this TType[] data, TType value)
		{
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = value;
			}
		}

		public static Guid GuidFromBits(long least, long most)
		{
			byte[] uuidMostSignificantBytes = BitConverter.GetBytes(most);
			byte[] uuidLeastSignificantBytes = BitConverter.GetBytes(least);
			byte[] guidBytes = new byte[16] {
				uuidMostSignificantBytes[4],
				uuidMostSignificantBytes[5],
				uuidMostSignificantBytes[6],
				uuidMostSignificantBytes[7],
				uuidMostSignificantBytes[2],
				uuidMostSignificantBytes[3],
				uuidMostSignificantBytes[0],
				uuidMostSignificantBytes[1],
				uuidLeastSignificantBytes[7],
				uuidLeastSignificantBytes[6],
				uuidLeastSignificantBytes[5],
				uuidLeastSignificantBytes[4],
				uuidLeastSignificantBytes[3],
				uuidLeastSignificantBytes[2],
				uuidLeastSignificantBytes[1],
				uuidLeastSignificantBytes[0]
			};

			return new Guid(guidBytes);
		}

		public static bool IsBitSet(this byte b, int pos)
		{
			return (b & (1 << pos)) != 0;
		}
	}
}
