using System.IO;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.IO;
using MiNET.Utils;
using NLog;

namespace Alex.Worlds.Multiplayer.Bedrock.Resources
{
	public class TexturePackEntry : ResourcePackEntry
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(TexturePackEntry));

		private TexturePackInfo Info { get; }
		private ZipFileSystem FileSystem { get; set; } = null;
		public BedrockResourcePack ResourcePack { get; private set; }

		/// <inheritdoc />
		public TexturePackEntry(TexturePackInfo info) : base(info.UUID, info.Version)
		{
			Info = info;

			Log.Info($"Downloading texturepack... ID={Identifier}, Key={info.ContentKey}");
		}

		/// <inheritdoc />
		protected override void OnComplete(byte[] data)
		{
			base.OnComplete(data);

			//if (!Directory.Exists("texturepacks"))
			//	Directory.CreateDirectory("texturepacks");

			//	File.WriteAllBytes(Path.Combine("texturepacks", $"TEXTUREPACK_{Identifier}_{Version}.zip"), data);

			FileSystem = new ZipFileSystem(new MemoryStream(data), Info.ContentIdentity);

			if (!string.IsNullOrWhiteSpace(Info.ContentKey))
			{
				FileSystem.UseEncryption(Info.ContentKey);
			}
			
			ResourcePack = new BedrockResourcePack(FileSystem);

			//	Log.Info($"Texturepack contains {ResourcePack.Textures.Count} textures");
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