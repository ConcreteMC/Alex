using System;
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

		public readonly int YFrames;

		public readonly int XFrames;
		//public readonly vecto

		public int FrameWidth => Animated ? Width / XFrames : Width;
		public int FrameHeight => Animated ? Height / YFrames : Height;

		public TextureInfo(Vector2 atlasSize,
			Vector2 position,
			int width,
			int height,
			bool animated,
			int framesInWidth,
			int framesInHeight)
		{
			AtlasSize = atlasSize;
			Position = position;
			Width = width;
			Height = height;
			Animated = animated;

			YFrames = Math.Max(framesInHeight, 1);
			XFrames = Math.Max(framesInWidth, 1);
			//int framesInWidth  = width / frameWidth;
			//int framesInHeight = height / frameHeight;
		}
	}
}