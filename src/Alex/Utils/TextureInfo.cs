using Microsoft.Xna.Framework;
using Rectangle = System.Drawing.Rectangle;

namespace Alex.Utils
{
	public struct TextureInfo
	{
		public readonly int Width;
		public readonly int Height;

		public readonly Vector2 AtlasSize;
		public readonly Vector2 Position;
		public readonly bool Animated;

		public readonly int YFrames;

		public readonly int XFrames;
		//public readonly vecto
		
		public TextureInfo(Vector2 atlasSize, Vector2 position, int width, int height, bool animated, int framesInWidth, int framesInHeight)
		{
			AtlasSize = atlasSize;
			Position = position;
			Width = width;
			Height = height;
			Animated = animated;

			YFrames = framesInHeight;
			XFrames = framesInWidth;
			//int framesInWidth  = width / frameWidth;
			//int framesInHeight = height / frameHeight;
		}
	}
}