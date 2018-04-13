using System;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui
{
	public struct Thickness : IEquatable<Thickness>
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

		public Vector2 ToVector2()
		{
			return new Vector2(Horizontal, Vertical);
		}

		public Point ToPoint()
		{
			return new Point(Horizontal, Vertical);
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

		public static bool operator ==(Thickness a, Thickness b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Thickness a, Thickness b)
		{
			return !(a == b);
		}


		public bool Equals(Thickness other)
		{
			return Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Thickness && Equals((Thickness) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Left;
				hashCode = (hashCode * 397) ^ Top;
				hashCode = (hashCode * 397) ^ Right;
				hashCode = (hashCode * 397) ^ Bottom;
				return hashCode;
			}
		}
	}
}
