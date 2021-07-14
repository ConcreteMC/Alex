using Alex.MoLang.Parser;
using Alex.MoLang.Runtime;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;

namespace Alex.ResourcePackLib.Json.Bedrock.Particles.Components
{
	public class InitialSpeedComponent : ParticleComponent
	{
		public MoLangVector3Expression Value { get; set; }

		/// <inheritdoc />
		public override void OnCreate(IParticle particle, MoLangRuntime runtime)
		{
			base.OnCreate(particle, runtime);

			particle.Velocity = Value.Evaluate(runtime, particle.Velocity);
		}
	}
}