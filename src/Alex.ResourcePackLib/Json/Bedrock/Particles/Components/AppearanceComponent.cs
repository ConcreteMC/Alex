using Alex.MoLang.Parser;
using Alex.MoLang.Runtime;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Particles.Components
{
	public class AppearanceComponent : ParticleComponent
	{
		[JsonProperty("size")]
		public MoLangVector2Expression Size { get; set; }
		
		[JsonProperty("facing_camera_mode")]
		public string FacingCameraMode { get; set; }
		
		[JsonProperty("uv")]
		public ParticleUV UV { get; set; }

		/// <inheritdoc />
		public override void OnCreate(IParticle particle, MoLangRuntime runtime)
		{
			base.OnCreate(particle, runtime);
			
			particle.UvPosition = UV.GetUv(runtime);
			particle.UvSize = UV.GetSize(runtime);
			particle.Size = Size.Evaluate(runtime, particle.Size);
			
			var flipbook = UV?.Flipbook;

			if (flipbook != null)
			{
				if (flipbook.MaxFrame != null)
				{
					particle.FrameCount = runtime.Execute(flipbook.MaxFrame).AsFloat();
				}
			}
		}

		/// <inheritdoc />
		public override void Update(IParticle particle, MoLangRuntime runtime)
		{
			base.Update(particle, runtime);
			
			var flipbook = UV?.Flipbook;
						
			particle.Size = Size.Evaluate(runtime, particle.Size);
	
			if (flipbook != null)
			{
				if (flipbook.FPS.HasValue)
				{
					var frame = (int) ((particle.Lifetime * flipbook.FPS.Value) % particle.FrameCount);

					particle.UvPosition = UV.GetUv(runtime)
					                      + flipbook.Step * frame;
				}
			}
		}
	}

	public class ParticleUV
	{
		[JsonProperty("texture_width")]
		public float TextureWidth { get; set; }
		
		[JsonProperty("texture_height")]
		public float TextureHeight { get; set; }
		
		[JsonProperty("uv")]
		public MoLangVector2Expression Uv { get; set; } = null;
		
		[JsonProperty("uv_size")]
		public MoLangVector2Expression Size { get; set; } = null;
		
		[JsonProperty("flipbook")]
		public Flipbook Flipbook { get; set; }

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

	public class Flipbook
	{
		[JsonProperty("base_UV")]
		public MoLangVector2Expression Base { get; set; }
		
		[JsonProperty("size_UV")]
		public Vector2? Size { get; set; } = null;
		
		[JsonProperty("step_UV")]
		public Vector2 Step { get; set; } = Vector2.Zero;

		[JsonProperty("frames_per_second")] public float? FPS { get; set; } = 8;
		
		[JsonProperty("max_frame")] public IExpression[] MaxFrame { get; set; }
		
		[JsonProperty("stretch_to_lifetime")] public bool StretchToLifetime { get; set; }
		
		[JsonProperty("loop")] public bool Loop { get; set; } = true;
	}
}