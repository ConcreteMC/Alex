using Alex.ResourcePackLib.Json.Bedrock.Entity;

namespace Alex.ResourcePackLib.Abstraction
{
	public interface IRenderControllerProvider
	{
		bool TryGetRenderController(string key, out RenderController renderController);
	}
}