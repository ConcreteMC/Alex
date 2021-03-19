using Alex.MoLang.Parser;
using Alex.MoLang.Runtime;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Particles.Components
{
	public class ParticleInitComponent : ParticleComponent
	{
		public const string ComponentName = "minecraft:particle_initialization";
		
		[JsonProperty("per_render_expression")]
		public IExpression[] PerRender { get; set; }
		
		public ParticleInitComponent()
		{
			
		}

		/// <inheritdoc />
		public override void PreRender(MoLangRuntime runtime)
		{
			base.PreRender(runtime);

			if (PerRender != null)
				runtime.Execute(PerRender);
		}
	}
}