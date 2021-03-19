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
				return Flipbook.Size;
			}
			
			return Size?.Evaluate(runtime, Vector2.Zero) ?? Vector2.One;
		}
	}

	public class Flipbook
	{
		[JsonProperty("base_UV")]
		public MoLangVector2Expression Base { get; set; }
		
		[JsonProperty("size_UV")]
		public Vector2 Size { get; set; }
		
		[JsonProperty("step_UV")]
		public Vector2 Step { get; set; } = Vector2.Zero;

		[JsonProperty("frames_per_second")] public float? FPS { get; set; } = 8;
		
		[JsonProperty("max_frame")] public IExpression[] MaxFrame { get; set; }
		
		[JsonProperty("stretch_to_lifetime")] public bool StretchToLifetime { get; set; }
		
		[JsonProperty("loop")] public bool Loop { get; set; } = true;
	}
}