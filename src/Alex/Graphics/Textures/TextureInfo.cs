using Alex.Common.Resources;
using Alex.ResourcePackLib.Json.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Alex.Graphics.Textures
{
	/// <summary>
	/// Represents a Texture in an atlas
	/// </summary>
	public class TextureInfo
	{
		public ResourceLocation ResourceLocation;
        
		/// <summary>
		/// Path of the source texture on disk
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
	}
}