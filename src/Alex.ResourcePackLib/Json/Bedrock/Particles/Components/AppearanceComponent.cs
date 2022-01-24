using Alex.MoLang.Runtime;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Particles.Components
{
	public class AppearanceComponent : ParticleComponent
	{
		[JsonProperty("size")] public MoLangVector2Expression Size { get; set; }

		[JsonProperty("facing_camera_mode")] public string FacingCameraMode { get; set; }

		[JsonProperty("uv")] public ParticleUV UV { get; set; }

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
					var frame = (int)((particle.Lifetime * flipbook.FPS.Value) % particle.FrameCount);

					particle.UvPosition = UV.GetUv(runtime) + flipbook.Step * frame;
				}
			}
		}
	}
}