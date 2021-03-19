using System.Collections.Generic;
using Alex.API.Resources;
using Alex.MoLang.Runtime;
using Alex.ResourcePackLib.Json.Bedrock.Particles.Components;
using Alex.ResourcePackLib.Json.Converters.Particles;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Particles
{
	public class ParticleDefinition
	{
		[JsonProperty("description")]
		public ParticleDescription Description { get; set; }
		
		[JsonProperty("components"), JsonConverter(typeof(ParticleComponentConverter))]
		public Dictionary<string, ParticleComponent> Components { get; set; }

		public int MaxParticles
		{
			get
			{
				if (Components.TryGetValue("minecraft:emitter_rate_manual", out var c) && c is EmitterRateComponent erc)
				{
					return erc.MaxParticles;
				}

				return 500;
			}
		}

		public Vector3 GetInitialSpeed(MoLangRuntime runtime)
		{
			if (Components.TryGetValue("minecraft:particle_initial_speed", out var c) && c is InitialSpeedComponent es)
			{
				return es.Value.Evaluate(runtime, Vector3.Zero);
			}

			return Vector3.Zero;
		}

		public double GetMaxLifetime(MoLangRuntime runtime)
		{
			if (Components.TryGetValue("minecraft:particle_lifetime_expression", out var lft)
			    && lft is LifetimeExpressionComponent lec)
			{
				return lec.CalculateLifetime(runtime);
			}

			return 0.5d;
		}

		public Vector3 GetLinearAcceleration(MoLangRuntime runtime)
		{
			if (Components.TryGetValue("minecraft:particle_motion_dynamic", out var pc) && pc is MotionComponent mc && mc.LinearAcceleration != null)
			{
				return mc.LinearAcceleration.Evaluate(runtime, Vector3.Zero);
			}
			
			return Vector3.Zero;
		}
		
		public float GetLinearDragCoEfficient(MoLangRuntime runtime)
		{
			if (Components.TryGetValue("minecraft:particle_motion_dynamic", out var pc) && pc is MotionComponent mc)
			{
				return mc.LinearDragCoEfficient(runtime);
			}
			
			return 0f;
		}
	}

	public class ParticleDescription
	{
		[JsonProperty("identifier")]
		public string Identifier { get; set; }
		
		[JsonProperty("basic_render_parameters")]
		public ParticleBasicRenderParameters BasicRenderParameters { get; set; }
	}

	public class ParticleBasicRenderParameters
	{
		[JsonProperty("material")]
		public string Material { get; set; }
		
		[JsonProperty("texture")]
		public string Texture { get; set; }
	}
}