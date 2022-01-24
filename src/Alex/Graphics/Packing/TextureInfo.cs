using System;
using Alex.Common.Resources;
using Alex.ResourcePackLib.Json.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Alex.Graphics.Packing
{
	/// <summary>
	/// Represents a Texture in an atlas
	/// </summary>
	public class TextureInfo : IComparable<TextureInfo>
	{
		public ResourceLocation ResourceLocation;

		/// <summary>
		/// Source image
		/// </summary>
		public Image<Rgba32> Source;

		/// <summary>
		/// Width in Pixels
		/// </summary>
		public int Width;

		/// <summary>
		/// Height in Pixels
		/// </summary>
		public int Height;

		public TextureMeta Meta;

		/// <inheritdoc />
		public int CompareTo(TextureInfo other)
		{
			if (ReferenceEquals(this, other)) return 0;
			if (ReferenceEquals(null, other)) return 1;
			var widthComparison = Width.CompareTo(other.Width);

			if (widthComparison != 0) return widthComparison;

			return Height.CompareTo(other.Height);
		}
	}
}