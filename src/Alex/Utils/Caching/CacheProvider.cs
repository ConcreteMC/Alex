using System;
using System.IO;
using Alex.Common.Services;

namespace Alex.Utils.Caching
{
	public class CacheProvider<TKey> : IDisposable
	{
		private IStorageSystem Storage { get; }
		private string BasePath { get; }
		public bool Enabled { get; set; } = false;

		public CacheProvider(IStorageSystem storage, string cacheIdentifier)
		{
			Storage = storage;
			BasePath = Path.Combine("cache", cacheIdentifier); // "blobs";

			if (!Storage.TryGetDirectory(BasePath, out _))
			{
				Storage.TryCreateDirectory(BasePath);
			}
		}

		private string GetPath(TKey hash)
		{
			return Path.Combine(BasePath, $"{hash}.bin");
		}

		public bool Contains(TKey hash)
		{
			return Storage.Exists(GetPath(hash));
		}

		public bool TryGet(TKey hash, out byte[] data)
		{
			return Storage.TryReadBytes(GetPath(hash), out data);
		}

		public bool TryStore(TKey hash, byte[] data)
		{
			return Storage.TryWriteBytes(GetPath(hash), data);
		}

		/// <inheritdoc />
		public virtual void Dispose() { }
	}
}