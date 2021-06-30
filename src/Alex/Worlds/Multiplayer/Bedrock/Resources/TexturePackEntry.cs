using System.IO;
using System.Linq;
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
		//public BedrockResourcePack ResourcePack { get; private set; }

		private BedrockResourcePack[] _resourcePacks = null;
		/// <inheritdoc />
		public TexturePackEntry(TexturePackInfo info) : base(info.UUID, info.Version)
		{
			Info = info;

			//Log.Info($"Downloading texturepack... ID={Identifier}, Key={info.ContentKey}");
		}

		/// <inheritdoc />
		protected override void OnComplete(byte[] data)
		{
			base.OnComplete(data);

			//if (!Directory.Exists("texturepacks"))
			//	Directory.CreateDirectory("texturepacks");

		//	File.WriteAllBytes(Path.Combine("texturepacks", $"TEXTUREPACK_{Identifier}_{Version}.zip"), data);
		//	File.WriteAllText(Path.Combine("texturepacks", $"TEXTUREPACK_{Identifier}_{Version}.txt"), Info.ContentKey ?? "");
			if (!string.IsNullOrWhiteSpace(Info.ContentKey))
			{
				Log.Warn($"Skipping resources as they seem to require encryption.");
				return;
			}
			
			FileSystem = new ZipFileSystem(new MemoryStream(data), Info.ContentIdentity);
			
			Log.Info($"Loading textures etc from bedrock pack...");
			_resourcePacks = Alex.Instance.Resources.LoadBedrockTexturePack(FileSystem, null, Info.ContentKey).ToArray();
			//ResourcePack = new BedrockResourcePack(FileSystem);

			Log.Info($"Texturepack completed: {Identifier}_{Version}");
			//	Log.Info($"Texturepack contains {ResourcePack.Textures.Count} textures");
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (_resourcePacks != null)
				{
					foreach (var pack in _resourcePacks)
					{
						Alex.Instance.Resources.Remove(pack);
						pack.Dispose();
					}

					_resourcePacks = null;
				}

				FileSystem?.Dispose();
				FileSystem = null;
			}
		}
	}
}