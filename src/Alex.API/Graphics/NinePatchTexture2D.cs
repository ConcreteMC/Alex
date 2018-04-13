using Alex.API.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Graphics
{
	public class NinePatchTexture2D
	{
		public Thickness Padding { get; }

		public Texture2D Texture { get; }

		public Rectangle Bounds { get; set; }

		public Rectangle[] SourceRegions { get; private set; }

		public NinePatchTexture2D(Texture2D texture, Rectangle bounds, int sizeSlice = 0) : this(texture, bounds,
			new Thickness(sizeSlice))
		{
		}
		public NinePatchTexture2D(Texture2D texture, Rectangle bounds, Rectangle innerSliceBounds) : this(texture, bounds,
			new Thickness(innerSliceBounds.Left - bounds.Left, innerSliceBounds.Top - bounds.Top, bounds.Right - innerSliceBounds.Right, bounds.Bottom - innerSliceBounds.Bottom))
		{
		}

		public NinePatchTexture2D(Texture2D texture, Rectangle bounds, Thickness padding)
		{
			Texture = texture;
			Bounds  = bounds == Rectangle.Empty ? texture.Bounds : bounds;
			Padding = padding;

			SourceRegions = CreateRegions(Bounds);
		}

		private Rectangle[] CreateRegions(Rectangle rectangle)
		{
			var x       = rectangle.X;
			var y       = rectangle.Y;
			var w       = rectangle.Width;
			var h       = rectangle.Height;
			var mWidth  = w     - Padding.Horizontal;
			var mHeight = h     - Padding.Vertical;
			var minY    = y     + Padding.Top;
			var maxY    = y + h - Padding.Bottom;
			var maxX    = x + w - Padding.Right;
			var minX    = x     + Padding.Left;

			return new Rectangle[]
			{
				new Rectangle(x,    y,    Padding.Left,  Padding.Top),
				new Rectangle(minX, y,    mWidth,        Padding.Top),
				new Rectangle(maxX, y,    Padding.Right, Padding.Top),
				new Rectangle(x,    minY, Padding.Left,  mHeight),
				new Rectangle(minX, minY, mWidth,        mHeight),
				new Rectangle(maxX, minY, Padding.Right, mHeight),
				new Rectangle(x,    maxY, Padding.Left,  Padding.Bottom),
				new Rectangle(minX, maxY, mWidth,        Padding.Bottom),
				new Rectangle(maxX, maxY, Padding.Right, Padding.Bottom),
			};
		}

		public Rectangle[] ProjectRegions(Rectangle rectangle)
		{
			return CreateRegions(rectangle);
		}

		public static implicit operator NinePatchTexture2D(Texture2D texture)
		{
			if (texture == null) return null;
			return new NinePatchTexture2D(texture, texture.Bounds);
		}
	}
}