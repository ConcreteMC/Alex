using System;
using System.Diagnostics;
using System.IO;
using Alex.Common.Services;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class BlobCache : IDisposable
	{
		private IStorageSystem Storage  { get; }
		private string         BasePath { get; }
		public  bool           Enabled  { get; set; } = false;

		private IDisposable _useChunkCacheOptionsAccessor;
		public BlobCache(IStorageSystem storage, IOptionsProvider optionsProvider)
		{
			Storage = storage;
			BasePath = "blobs";

			if (!Storage.TryGetDirectory(BasePath, out _))
			{
				Storage.TryCreateDirectory(BasePath);
			}

			Enabled = optionsProvider.AlexOptions.MiscelaneousOptions.UseChunkCache.Value;
			_useChunkCacheOptionsAccessor = optionsProvider.AlexOptions.MiscelaneousOptions.UseChunkCache.Bind(CacheStatusChanges);
		}

		private void CacheStatusChanges(bool oldvalue, bool newvalue)
		{
			Enabled = newvalue;
		}

		private string GetPath(ulong hash)
		{
			return Path.Combine(BasePath, $"{hash}.bin");
		}

		public bool Contains(ulong hash)
		{
			return Storage.Exists(GetPath(hash));
		}

		public bool TryGet(ulong hash, out byte[] data)
		{
			return Storage.TryReadBytes(GetPath(hash), out data);
		}

		public bool TryStore(ulong hash, byte[] data)
		{
			return Storage.TryWriteBytes(GetPath(hash), data);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_useChunkCacheOptionsAccessor.Dispose();
		}
	}
}