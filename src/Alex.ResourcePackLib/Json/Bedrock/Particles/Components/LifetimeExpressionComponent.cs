using Alex.MoLang.Parser;
using Alex.MoLang.Runtime;
using LibNoise.Combiner;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Particles.Components
{
	public class LifetimeExpressionComponent : ParticleComponent
	{
		public LifetimeExpressionComponent()
		{
			
		}
		
		[JsonProperty("max_lifetime")]
		public IExpression[] MaxLifetime { get; set; }

		public double CalculateLifetime(MoLangRuntime runtime)
		{
			return runtime.Execute(MaxLifetime).AsDouble();
		}

		/// <inheritdoc />
		public override void OnCreate(IParticle particle, MoLangRuntime runtime)
		{
			base.OnCreate(particle, runtime);
			particle.MaxLifetime = CalculateLifetime(runtime);
		}
	}
}