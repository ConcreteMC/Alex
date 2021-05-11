using System.IO;
using Alex.ResourcePackLib;
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
		public BedrockResourcePack ResourcePack { get; private set; }

		/// <inheritdoc />
		public BehaviorPackEntry(ResourcePackInfo info) : base(info.UUID, info.Version)
		{
			Info = info;
			//Log.Info($"BehaviorPack: {info.PackIdVersion.Id}_{info.PackIdVersion.Version}");
		}

		protected override void OnComplete(byte[] data)
		{
			base.OnComplete(data);

			//if (!Directory.Exists("texturepacks"))
			//	Directory.CreateDirectory("texturepacks");

			//File.WriteAllBytes($"BEHAVIORPACK_{Identifier}_{Version}.zip", data);

			FileSystem = new ZipFileSystem(new MemoryStream(data), Info.ContentIdentity);
			
			if (!string.IsNullOrWhiteSpace(Info.ContentKey))
			{
				FileSystem.UseEncryption(Info.ContentKey);
			}
			
			ResourcePack = new BedrockResourcePack(FileSystem);

			//Log.Info($"Behaviorpack contains {ResourcePack.Textures.Count} textures");
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