using System;
using Alex.Common.Services;

namespace Alex.Utils.Caching
{
	public class BlobCache : CacheProvider<ulong>
	{
		private IDisposable _useChunkCacheOptionsAccessor;
		public BlobCache(IStorageSystem storage, IOptionsProvider optionsProvider) : base(storage, "blobs")
		{
			Enabled = optionsProvider.AlexOptions.MiscelaneousOptions.UseChunkCache.Value;
			_useChunkCacheOptionsAccessor = optionsProvider.AlexOptions.MiscelaneousOptions.UseChunkCache.Bind(CacheStatusChanges);
		}

		private void CacheStatusChanges(bool oldvalue, bool newvalue)
		{
			Enabled = newvalue;
		}

		/// <inheritdoc />
		public override void Dispose()
		{
			_useChunkCacheOptionsAccessor.Dispose();
		}
	}
}