using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Particles.Components
{
	public class EmitterRateComponent : ParticleComponent
	{
		[JsonProperty("max_particles")] public int MaxParticles { get; set; } = 30;
	}
}