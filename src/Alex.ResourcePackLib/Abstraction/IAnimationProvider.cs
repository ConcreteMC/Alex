using Alex.ResourcePackLib.Json.Bedrock.Entity;

namespace Alex.ResourcePackLib.Abstraction
{
	public interface IAnimationProvider
	{
		bool TryGetAnimationController(string key, out AnimationController animationController);
		bool TryGetAnimation(string key, out Animation animation);
	}
}