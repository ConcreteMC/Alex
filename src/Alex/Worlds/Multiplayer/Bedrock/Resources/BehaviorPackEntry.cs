using System.IO;
using Alex.ResourcePackLib.IO;
using MiNET.Utils;
using NLog;

namespace Alex.Worlds.Multiplayer.Bedrock.Resources
{
	public class BehaviorPackEntry : ResourcePackEntry
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BehaviorPackEntry));
		private ResourcePackInfo Info { get; }

		private ZipFileSystem FileSystem { get; set; } = null;

		/// <inheritdoc />
		public BehaviorPackEntry(ResourcePackInfo info) : base(info.UUID, info.Version)
		{
			Info = info;
		}

		protected override void OnComplete(byte[] data)
		{
			base.OnComplete(data);

			FileSystem = new ZipFileSystem(new MemoryStream(data), Info.ContentIdentity);

			Log.Info($"Behaviorpack completed: {Identifier}_{Version}");
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				FileSystem?.Dispose();
				FileSystem = null;
			}
		}
	}
}