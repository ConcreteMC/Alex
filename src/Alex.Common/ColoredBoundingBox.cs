using Microsoft.Xna.Framework;

namespace Alex.Common
{
	public struct ColoredBoundingBox
	{
		public BoundingBox Box { get; }
		public Color Color { get; }

		public ColoredBoundingBox(BoundingBox box, Color color)
		{
			Box = box;
			Color = color;
		}
	}
}