using Alex.Interfaces;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using ConcreteMC.MolangSharp.Runtime;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Particles.Components
{
	public class ParticleUV
	{
		[JsonProperty("texture_width")] public float TextureWidth { get; set; }

		[JsonProperty("texture_height")] public float TextureHeight { get; set; }

		[JsonProperty("uv")] public MoLangVector2Expression Uv { get; set; } = null;

		[JsonProperty("uv_size")] public MoLangVector2Expression Size { get; set; } = null;

		[JsonProperty("flipbook")] public Flipbook Flipbook { get; set; }

		public IVector2 GetUv(MoLangRuntime runtime)
		{
			if (Flipbook?.Base != null)
			{
				return Flipbook.Base.Evaluate(runtime, VectorUtils.VectorFactory.Vector2Zero);
			}

			return Uv?.Evaluate(runtime, VectorUtils.VectorFactory.Vector2Zero) ?? VectorUtils.VectorFactory.Vector2Zero;
		}

		public IVector2 GetSize(MoLangRuntime runtime)
		{
			if (Flipbook?.Size != null)
			{
				return Flipbook.Size;
			}

			return (Size?.Evaluate(runtime, VectorUtils.VectorFactory.Vector2Zero) ?? VectorUtils.VectorFactory.Vector2Zero);
		}
	}
}