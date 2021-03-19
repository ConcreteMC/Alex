using Microsoft.Xna.Framework;

namespace Alex.Utils
{
	public struct TextureInfo
	{
		public readonly int Width;
		public readonly int Height;

		public readonly Vector2 AtlasSize;
		public readonly Vector2 Position;
		public readonly bool Animated;
		
		public TextureInfo(Vector2 atlasSize, Vector2 position, int width, int height, bool animated)
		{
			AtlasSize = atlasSize;
			Position = position;
			Width = width;
			Height = height;
			Animated = animated;
		}
	}
}