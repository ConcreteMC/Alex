using Alex.MoLang.Runtime;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using Microsoft.Xna.Framework;
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

		public Vector2 GetUv(MoLangRuntime runtime)
		{
			if (Flipbook?.Base != null)
			{
				return Flipbook.Base.Evaluate(runtime, Vector2.Zero);
			}

			return Uv?.Evaluate(runtime, Vector2.Zero) ?? Vector2.Zero;
		}

		public Vector2 GetSize(MoLangRuntime runtime)
		{
			if (Flipbook?.Size != null)
			{
				return Flipbook.Size.Value;
			}

			return (Size?.Evaluate(runtime, Vector2.One) ?? (Vector2.One));
		}
	}
}