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

		private MCBedrockResourcePack[] _resourcePacks = null;

		/// <inheritdoc />
		public TexturePackEntry(TexturePackInfo info) : base(info.UUID, info.Version)
		{
			Info = info;
		}

		/// <inheritdoc />
		protected override void OnComplete(byte[] data)
		{
			base.OnComplete(data);

			FileSystem = new ZipFileSystem(new MemoryStream(data), Info.ContentIdentity);

			_resourcePacks = Alex.Instance.Resources.LoadBedrockTexturePack(FileSystem, null, Info.ContentKey)
			   .ToArray();
			
			Log.Info($"Texturepack completed: {Identifier}_{Version}");
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