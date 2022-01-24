using Alex.MoLang.Runtime;
using Alex.ResourcePackLib.Json.Converters.Particles;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Particles.Components
{
	public class AppearanceTintingComponent : ParticleComponent
	{
		[JsonProperty("color")] public ParticleColor Color { get; set; }

		/// <inheritdoc />
		public override void OnCreate(IParticle particle, MoLangRuntime runtime)
		{
			base.OnCreate(particle, runtime);
		}
	}
}