using Alex.MoLang.Parser;
using Alex.ResourcePackLib.Json.Bedrock.Entity;

namespace Alex.ResourcePackLib.Json.Bedrock.Particles.Components
{
	public class InitialSpeedComponent : ParticleComponent
	{
		public MoLangVector3Expression Value { get; set; }
	}
}