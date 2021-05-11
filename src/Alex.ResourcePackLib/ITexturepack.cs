using System;
using System.Collections.Generic;
using Alex.API.Resources;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Alex.ResourcePackLib
{
	public interface ITexturePack
	{
		IReadOnlyDictionary<ResourceLocation, Lazy<Image<Rgba32>>> Textures { get; }
	}
}