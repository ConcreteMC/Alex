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
		//public BedrockResourcePack ResourcePack { get; private set; }

		/// <inheritdoc />
		public BehaviorPackEntry(ResourcePackInfo info) : base(info.UUID, info.Version)
		{
			Info = info;
			Log.Info($"Downloading behaviorpack... ID={Identifier}, Key={info.ContentKey}");
		}

		protected override void OnComplete(byte[] data)
		{
			base.OnComplete(data);

			if (!Alex.Instance.Options.AlexOptions.MiscelaneousOptions.LoadServerResources.Value)
				return;
			
			if (!Directory.Exists("texturepacks"))
				Directory.CreateDirectory("texturepacks");

			File.WriteAllBytes($"BEHAVIORPACK_{Identifier}_{Version}.zip", data);

			if (!string.IsNullOrWhiteSpace(Info.ContentKey))
			{
				Log.Warn($"Skipping behaviorpack as they seem to require encryption.");
				return;
				//FileSystem.UseEncryption(Info.ContentKey);
			}
			
			FileSystem = new ZipFileSystem(new MemoryStream(data), Info.ContentIdentity);
			//ResourcePack = new BedrockResourcePack(FileSystem);

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