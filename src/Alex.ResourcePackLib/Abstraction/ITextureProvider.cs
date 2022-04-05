using System;
using System.Collections.Generic;
using Alex.Interfaces.Resources;
using Alex.ResourcePackLib.Json.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Alex.ResourcePackLib.Abstraction
{
	public interface ITextureProvider
	{
		//IReadOnlyDictionary<ResourceLocation, Lazy<Image<Rgba32>>> Textures { get; }

		bool TryGetBitmap(ResourceLocation textureName, out Image<Rgba32> bitmap);

		bool TryGetTextureMeta(ResourceLocation textureName, out TextureMeta meta);
	}
}