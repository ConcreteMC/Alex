using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Alex
{
	public static class Extensions
	{
		private static Texture2D WhiteTexture { get; set; }

		static Extensions()
		{
			
		}

	    public static void Init(GraphicsDevice gd)
	    {
            WhiteTexture = new Texture2D(gd, 1, 1);
            WhiteTexture.SetData(new Color[] { Color.White });
        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N)
        {
            return source.Skip(Math.Max(0, source.Count() - N));
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int MapVirtualKey(int uCode, int uMapType);

        public static bool RepresentsPrintableChar(this Keys key)
        {
            return !char.IsControl((char)MapVirtualKey((int)key, 2));
        }

        public static bool IsKeyAChar(this Keys key)
        {
            return key >= Keys.A && key <= Keys.Z;
        }

        public static bool IsKeyADigit(this Keys key)
        {
            return (key >= Keys.D0 && key <= Keys.D9) || (key >= Keys.NumPad0 && key <= Keys.NumPad9);
        }

        [DllImport("user32.dll")]
        static extern short VkKeyScan(char c);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int ToAscii(
            uint uVirtKey,
            uint uScanCode,
            byte[] lpKeyState,
            out uint lpChar,
            uint flags
            );

        public static char GetModifiedChar(this char c)
        {
            short vkKeyScanResult = VkKeyScan(c);

            if (vkKeyScanResult == -1)
                return c;

            uint code = (uint)vkKeyScanResult & 0xff;

            byte[] b = new byte[256];
            b[0x10] = 0x80;

            uint r;
            if (1 != ToAscii(code, code, b, out r, 0))
                throw new ApplicationException("Could not translate modified state");

            return (char)r;
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

	    public static string StripIllegalCharacters(this string input)
	    {
            return input.ToArray()
                    .Where(i => !Alex.Font.Characters.Contains(i))
                    .Aggregate(input, (current, i) => current.Replace(i.ToString(), ""));
        }

	    public static string StripColors(this string input)
	    {
            if (input == null)
                throw new ArgumentNullException("input");
            if (input.IndexOf('§') == -1)
            {
                return input;
            }
            else
            {
                StringBuilder output = new StringBuilder(input.Length);
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] == '§')
                    {
                        if (i == input.Length - 1)
                        {
                            break;
                        }
                        else if (input[i + 1] == '§')
                        {
                            output.Append('§');
                        }
                        i++;
                    }
                    else
                    {
                        output.Append(input[i]);
                    }
                }
                return output.ToString();
            }
        }

	    public static Vector3 Floor(this Vector3 toFloor)
	    {
	        return new Vector3((float)Math.Floor(toFloor.X), (float)Math.Floor(toFloor.Y), (float)Math.Floor(toFloor.Z));
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
