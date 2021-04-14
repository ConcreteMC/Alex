using Alex.MoLang.Runtime;

namespace Alex.ResourcePackLib.Json.Bedrock.Particles.Components
{
	public class ParticleComponent
	{
		public virtual void OnCreate(IParticle particle, MoLangRuntime runtime){}
		public virtual void Update(IParticle particle, MoLangRuntime runtime){}
		public virtual void PreRender(IParticle particle, MoLangRuntime runtime){}
	}
}