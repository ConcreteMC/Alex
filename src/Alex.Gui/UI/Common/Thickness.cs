using Microsoft.Xna.Framework;

namespace Alex.Graphics.UI.Common
{
	public struct Thickness
	{
		public static Thickness Zero => new Thickness(0);
		
		public int Left { get; }
		public int Top { get; }
		public int Right { get; }
		public int Bottom { get; }

		public int Vertical => Top + Bottom;
		public int Horizontal => Left + Right;


		public Thickness(int all) : this(all, all)
		{
		}

		public Thickness(int vertical, int horizontal) : this(horizontal, vertical, horizontal, vertical)
		{
		}

		public Thickness(int left, int top, int right, int bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		public static Thickness operator +(Thickness a, Thickness b)
		{
			return new Thickness(
				a.Left + b.Left,
				a.Top + b.Top,
				a.Right + b.Right,
				a.Bottom + b.Bottom
				);
		}

		public static Thickness operator -(Thickness a, Thickness b)
		{
			return new Thickness(
				a.Left   - b.Left,
				a.Top    - b.Top,
				a.Right  - b.Right,
				a.Bottom - b.Bottom
			);
		}

		public static Rectangle operator -(Rectangle rect, Thickness b)
		{
			return new Rectangle(
				rect.Left + b.Left,
				rect.Top + b.Top,
				rect.Width - b.Horizontal,
				rect.Height - b.Vertical
			);
		}

		public static Rectangle operator +(Rectangle rect, Thickness b)
		{
			return new Rectangle(
				rect.Left - b.Left,
				rect.Top - b.Top,
				rect.Width + b.Horizontal,
				rect.Height + b.Vertical
			);
		}
	}
}
