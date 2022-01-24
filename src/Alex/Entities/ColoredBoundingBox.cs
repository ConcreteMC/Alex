using Microsoft.Xna.Framework;

namespace Alex.Entities
{
	public class ColoredBoundingBox
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