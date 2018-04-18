using System;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui
{
	public struct Thickness : IEquatable<Thickness>
	{
		public static Thickness Zero => new Thickness(0);
		public static Thickness One => new Thickness(1);
		
		public int Left { get; set; }
		public int Top { get; set; }
		public int Right { get; set;}
		public int Bottom { get; set;}

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

		public double Size()
		{
			return Math.Sqrt(Vertical * Vertical + Horizontal * Horizontal);
		}

		public static Thickness Max(Thickness a, Thickness b)
		{
			return new Thickness(Math.Max(a.Left, b.Left), Math.Max(a.Top, b.Top), Math.Max(a.Right, b.Right), Math.Max(a.Bottom, b.Bottom));
		}
		public static Thickness Min(Thickness a, Thickness b)
		{
			return new Thickness(Math.Min(a.Left, b.Left), Math.Min(a.Top, b.Top), Math.Min(a.Right, b.Right), Math.Min(a.Bottom, b.Bottom));
		}
		public static Thickness Clamp(Thickness value, Thickness minValue, Thickness maxValue)
		{
			return new Thickness(MathHelper.Clamp(value.Left, minValue.Left, maxValue.Left), MathHelper.Clamp(value.Top, minValue.Top, maxValue.Top), MathHelper.Clamp(value.Right, minValue.Right, maxValue.Right), MathHelper.Clamp(value.Bottom, minValue.Bottom, maxValue.Bottom));
		}

		public Vector2 ToVector2()
		{
			return new Vector2(Horizontal, Vertical);
		}

		public Point ToPoint()
		{
			return new Point(Horizontal, Vertical);
		}

		#region Operator Overloads

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

		public static Rectangle operator +(Rectangle rect, Thickness b)
		{
			return new Rectangle(
				rect.Left - b.Left,
				rect.Top - b.Top,
				rect.Width + b.Horizontal,
				rect.Height + b.Vertical
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

		public static Point operator +(Point p, Thickness b)
		{
			return new Point(p.X + b.Horizontal, p.Y + b.Vertical);
		}
		public static Point operator -(Point p, Thickness b)
		{
			return new Point(p.X - b.Horizontal, p.Y - b.Vertical);
		}

		public static Size operator +(Size a, Thickness b)
		{
			return new Size(a.Width + b.Horizontal, a.Height + b.Vertical);
		} 
		public static Size operator -(Size a, Thickness b)
		{
			return new Size(a.Width - b.Horizontal, a.Height - b.Vertical);
		} 


		public static bool operator ==(Thickness a, Thickness b)
		{
			return a.Equals(b);
		}
		public static bool operator !=(Thickness a, Thickness b)
		{
			return !(a == b);
		}

		#endregion

		#region Equality

		public bool Equals(Thickness other)
		{
			return Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Thickness thickness && Equals(thickness);
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
	
		#endregion
	}
}
