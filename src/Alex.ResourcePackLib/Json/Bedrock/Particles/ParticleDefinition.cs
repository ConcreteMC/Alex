using System.Collections.Generic;
using System.Linq;
using Alex.ResourcePackLib.Json.Bedrock.Particles.Components;
using Alex.ResourcePackLib.Json.Converters.Particles;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Particles
{
	public class ParticleDefinition
	{
		[JsonProperty("description")] public ParticleDescription Description { get; set; }

		[JsonProperty("components"), JsonConverter(typeof(ParticleComponentConverter))]
		public Dictionary<string, ParticleComponent> Components { get; set; }

		/// <summary>
		///		Maximum number of particles that can be active at once for this emitter
		/// </summary>
		public int MaxParticles
		{
			get
			{
				if (Components.TryGetValue("minecraft:emitter_rate_manual", out var c) && c is EmitterRateComponent erc)
				{
					return erc.MaxParticles;
				}

				return 50;
			}
		}

		public bool TryGetComponent<T>(out T component) where T : ParticleComponent
		{
			var result = Components.FirstOrDefault(x => x.Value.GetType() == typeof(T));

			if (result.Value != null)
			{
				component = (T)result.Value;

				return true;
			}

			component = null;

			return false;
		}
	}

	public class ParticleDescription
	{
		[JsonProperty("identifier")] public string Identifier { get; set; }

		[JsonProperty("basic_render_parameters")]
		public ParticleBasicRenderParameters BasicRenderParameters { get; set; }
	}

	public class ParticleBasicRenderParameters
	{
		[JsonProperty("material")] public string Material { get; set; }

		[JsonProperty("texture")] public string Texture { get; set; }
	}
}