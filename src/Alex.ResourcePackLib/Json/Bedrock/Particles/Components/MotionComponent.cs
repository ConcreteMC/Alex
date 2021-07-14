using Alex.MoLang.Parser;
using Alex.MoLang.Runtime;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Particles.Components
{
	public class MotionComponent : ParticleComponent
	{
		[JsonProperty("linear_acceleration")] 
		public MoLangVector3Expression LinearAcceleration { get; set; }

		[JsonProperty("linear_drag_coefficient")]
		public IExpression[] LinearDragCoEfficientExpressions { get; set; }

		public float LinearDragCoEfficient(MoLangRuntime runtime) =>
			runtime.Execute(LinearDragCoEfficientExpressions).AsFloat();
		
		/// <inheritdoc />
		public override void OnCreate(IParticle particle, MoLangRuntime runtime)
		{
			base.OnCreate(particle, runtime);

			if (LinearAcceleration != null)
			{
				particle.Acceleration = LinearAcceleration.Evaluate(runtime, particle.Acceleration);
			}

			if (LinearDragCoEfficientExpressions != null)
			{
				particle.DragCoEfficient = LinearDragCoEfficient(runtime);
			}
		}
	}
}