using System.Diagnostics;
using System.IO;
using Alex.API.Services;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class BlobCache
	{
		private IStorageSystem Storage  { get; }
		private string        BasePath { get; }
		public BlobCache(IStorageSystem storage)
		{
			Storage = storage;
			BasePath = "blobs";

			if (!Storage.TryGetDirectory(BasePath, out _))
			{
				Storage.TryCreateDirectory(BasePath);
			}
		}

		private string GetPath(ulong hash)
		{
			return Path.Combine(BasePath, $"{hash}.bin");
		}

		public bool Contains(ulong hash)
		{
			return Storage.Exists(GetPath(hash));
		}
	}
}