using Newtonsoft.Json;

namespace Alex.Particles
{
	public class ParticleMapping
	{
		[JsonProperty("bedrockId")]
		public string BedrockId { get; set; }
	}
}