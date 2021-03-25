using Alex.MoLang.Parser;
using Alex.MoLang.Runtime;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Particles.Components
{
	public class EmitterInitComponent: ParticleComponent
	{
		public const string ComponentName = "minecraft:emitter_initialization";
		
		[JsonProperty("creation_expression")]
		public IExpression[] OnCreateExpressions { get; set; }
		
		[JsonProperty("per_update_expression")]
		public IExpression[] UpdateExpressions { get; set; }
		
		public EmitterInitComponent()
		{
			
		}

		/// <inheritdoc />
		public override void OnCreate(MoLangRuntime runtime)
		{
			base.OnCreate(runtime);
			
			if (OnCreateExpressions != null)
				runtime.Execute(OnCreateExpressions);
		}

		/// <inheritdoc />
		public override void Update(MoLangRuntime runtime)
		{
			base.Update(runtime);

			if (UpdateExpressions != null)
				runtime.Execute(UpdateExpressions);
		}
	}
}