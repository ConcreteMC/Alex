using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace Alex.Utils
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	// ReSharper disable once InconsistentNaming
	public struct BlockTextureData
	{
		public Color ColorLeft;
		public Color ColorRight;

		public Color ColorFront;
		public Color ColorBack;

		public Color ColorTop;
		public Color ColorBottom;

		public Vector2 TopLeft;
		public Vector2 TopRight;

		public Vector2 BottomLeft;
		public Vector2 BottomRight;

		public bool IsAnimated;

		public TextureInfo TextureInfo;

		public BlockTextureData(TextureInfo textureInfo,
			Vector2 topLeft,
			Vector2 topRight,
			Vector2 bottomLeft,
			Vector2 bottomRight,
			Color colorSide,
			Color colorTop,
			Color colorBottom,
			bool isAnimated = false)
		{
			TextureInfo = textureInfo;
			TopLeft = topLeft;
			TopRight = topRight;
			BottomLeft = bottomLeft;
			BottomRight = bottomRight;

			ColorFront = colorSide;
			ColorBack = colorSide;

			ColorLeft = colorSide;
			ColorRight = colorSide;

			ColorTop = colorTop;
			ColorBottom = colorBottom;

			IsAnimated = isAnimated;
		}
	}
}