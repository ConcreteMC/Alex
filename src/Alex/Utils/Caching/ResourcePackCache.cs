using Alex.Common.Services;

namespace Alex.Utils.Caching
{
	public class ResourcePackCache : CacheProvider<string>
	{
		/// <inheritdoc />
		public ResourcePackCache(IStorageSystem storage) : base(storage, "bedrockresources")
		{
			
		}
	}
}