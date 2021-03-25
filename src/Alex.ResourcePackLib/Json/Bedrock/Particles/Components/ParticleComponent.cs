using Alex.MoLang.Runtime;

namespace Alex.ResourcePackLib.Json.Bedrock.Particles.Components
{
	public class ParticleComponent
	{
		public virtual void OnCreate(MoLangRuntime runtime){}
		public virtual void Update(MoLangRuntime runtime){}
		public virtual void PreRender(MoLangRuntime runtime){}
	}
}