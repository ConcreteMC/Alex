using Microsoft.Xna.Framework;

namespace Alex.Utils
{
	public class TextureInfo
	{
		public int Width { get; } = 16;
		public int Height { get; } = 16;

		public Vector2 Position { get; } = Vector2.Zero;

		public TextureInfo(Vector2 position, int width, int height)
		{
			Position = position;
			Width = width;
			Height = height;
		}
	}
}