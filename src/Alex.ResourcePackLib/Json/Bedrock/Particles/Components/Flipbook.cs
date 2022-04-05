using Alex.Interfaces;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using ConcreteMC.MolangSharp.Parser;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Particles.Components
{
	public class Flipbook
	{
		[JsonProperty("base_UV")] public MoLangVector2Expression Base { get; set; }

		[JsonProperty("size_UV")] public IVector2 Size { get; set; } = null;

		[JsonProperty("step_UV")] public IVector2 Step { get; set; } = VectorUtils.VectorFactory.Vector2Zero;

		[JsonProperty("frames_per_second")] public float? FPS { get; set; } = 8;

		[JsonProperty("max_frame")] public IExpression MaxFrame { get; set; }

		[JsonProperty("stretch_to_lifetime")] public bool StretchToLifetime { get; set; }

		[JsonProperty("loop")] public bool Loop { get; set; } = true;
	}
}