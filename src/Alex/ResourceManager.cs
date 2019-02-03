using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using Alex.API;
using Alex.API.Json;
using Alex.API.Services;
using Alex.Entities;
using Alex.Rendering;
using Alex.ResourcePackLib;
using Alex.Utils;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using NLog;

namespace Alex
{
	public class ResourceManager
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ResourceManager));

		private LinkedList<McResourcePack> ActiveResourcePacks { get; } = new LinkedList<McResourcePack>();
		public McResourcePack ResourcePack => ActiveResourcePacks.First.Value;
		public BedrockResourcePack BedrockResourcePack { get; private set; }
		public Registries Registries { get; private set; }
        public AtlasGenerator Atlas { get; private set; }
		
		private GraphicsDevice Graphics { get; set; }

		private IStorageSystem Storage { get; }
		public ResourceManager(GraphicsDevice graphics, IStorageSystem storageSystem)
		{
			Storage = storageSystem;
			Graphics = graphics;
			Atlas = new AtlasGenerator(Graphics);
		}

		private static readonly string VersionFile = Path.Combine("assets", "version.txt");
		private byte[] GetLatestAssets()
		{
			try
			{
				using (WebClient wc = new WebClient())
				{
					var rawJson = wc.DownloadString("https://launchermeta.mojang.com/mc/game/version_manifest.json?_t=" +
					                                (long) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);

					VersionManifest manifest = VersionManifest.FromJson(rawJson);
					Version latestSnapshotVersion =
						manifest.Versions.OrderByDescending(x => x.ReleaseTime.ToUnixTimeSeconds()).FirstOrDefault();

					Log.Info($"Using assets version {latestSnapshotVersion.Id}");

					byte[] data;
					string savedPath = Path.Combine("assets", $"java-{latestSnapshotVersion.Id}.zip");
					if (Storage.TryReadBytes(savedPath, out data))
					{
						return data;
					}
					else
					{
						Log.Info("Downloading latest vanilla Minecraft resources...");
						LauncherMeta meta = LauncherMeta.FromJson(wc.DownloadString(latestSnapshotVersion.Url));
						byte[] clientData = wc.DownloadData(meta.Downloads.Client.Url);
						if (Storage.TryWriteBytes(savedPath, clientData))
						{
							Storage.TryWriteBytes(VersionFile, Encoding.Unicode.GetBytes(savedPath));
						}
						return clientData;
					}
				}
			}
			catch
			{
				Log.Warn($"Failed to check for latest assets!");
				if (Storage.TryReadBytes(VersionFile, out byte[] value))
				{
					string content = Encoding.Unicode.GetString(value);
					if (Storage.TryReadBytes(content, out value))
					{
						return value;
					}
				}
			}

			return null;
		}

		private McResourcePack LoadResourcePack(IProgressReceiver progressReceiver, GraphicsDevice graphics, Stream stream, bool replaceModels = false,
			bool replaceTextures = false, bool reportMissingModels = false, McResourcePack.McResourcePackPreloadCallback preloadCallback = null)
		{
			McResourcePack resourcePack = null;

			using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, false))
			{
				resourcePack = new McResourcePack(archive, graphics, preloadCallback);
			}

			Log.Info($"Loaded {resourcePack.BlockModels.Count} block models from resourcepack");
			Log.Info($"Loaded {resourcePack.ItemModels.Count} item models from resourcepack");

			if (!replaceTextures)
			{
				Atlas.LoadResourcePackOnTop(resourcePack.TexturesAsBitmaps.Where(x => x.Key.StartsWith("block")).ToArray(),
					progressReceiver);
			}
			else
			{
				Atlas.GenerateAtlas(resourcePack.TexturesAsBitmaps.Where(x => x.Key.StartsWith("block")).ToArray(),
					progressReceiver);
			}

			Stopwatch sw = Stopwatch.StartNew();
			int imported = BlockFactory.LoadResources(this, resourcePack, replaceModels, reportMissingModels, progressReceiver);
			sw.Stop();

			Log.Info($"Imported {imported} blockstate variants from resourcepack in {sw.ElapsedMilliseconds}ms!");

			ItemFactory.Init(this, resourcePack, progressReceiver);

			var language = resourcePack.Languages.Values.FirstOrDefault();
			if (language != null)
			{
				foreach (var translation in language)
				{
					ChatParser.TranslationRules[translation.Key] = translation.Value;
				}
			}

			return resourcePack;
		}

		private static string Root = "assets";
		private static readonly string ResourcePackDirectory = Path.Combine(Root, "resourcepacks");
		private static readonly string BedrockResourcePackPath = Path.Combine(Root, "bedrock.zip");

		private bool CheckRequiredPaths(out byte[] javaResources, out byte[] bedrockResources)
		{
			if (!Directory.Exists(Root))
			{
				Directory.CreateDirectory(Root);
			}

			if (!Directory.Exists(ResourcePackDirectory))
			{
				Directory.CreateDirectory(ResourcePackDirectory);
			}

			if (!Storage.TryReadBytes(BedrockResourcePackPath, out bedrockResources))
			{
				Log.Error(
					$"Missing bedrock edition resources! Please put a copy of the bedrock resources in a zip archive with the path '{BedrockResourcePackPath}'");
				javaResources = null;
				bedrockResources = null;
				return false;
			}

			try
			{
				javaResources = GetLatestAssets();
				if (javaResources == null)
				{
					Log.Error($"Could not load any assets! Are you connected to the internet?");

					javaResources = null;
					bedrockResources = null;
					return false;
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Could not check for latests assets! Do you have a internet connection up?");
				javaResources = null;
				bedrockResources = null;
				return false;
			}

			return true;
		}

		public bool CheckResources(GraphicsDevice device, Settings setings, IProgressReceiver progressReceiver, McResourcePack.McResourcePackPreloadCallback preloadCallback)
		{
			byte[] defaultResources;
			byte[] bedrockResources;

			if (!CheckRequiredPaths(out defaultResources, out bedrockResources))
			{
				return false;
			}

			Log.Info($"Loading registries...");
			progressReceiver?.UpdateProgress(0, "Loading registries...");
			Registries = JsonConvert.DeserializeObject<Registries>(Resources.registries);
			progressReceiver?.UpdateProgress(100, "Loading registries...");

            Log.Info($"Loading vanilla resources...");
			using (MemoryStream stream = new MemoryStream(defaultResources))
			{
				ActiveResourcePacks.AddFirst(LoadResourcePack(progressReceiver, device, stream, true, true, true, preloadCallback));
			}

			var bedrockPath = Path.Combine("assets", "bedrock");

            DirectoryInfo directory;
            if (!Storage.TryGetDirectory(bedrockPath, out directory))
			{
				Log.Warn($"The bedrock resources required to play this game are not set-up correctly!");
				Environment.Exit(1);
				return false;
			}

            var directories = directory.GetDirectories();

            if (!directories.Any(x => x.Name.Equals("models")))
            {
				Log.Warn($"Please make sure to extract the MC:Bedrock resource pack into \"{directory.FullName}\"");
				Environment.Exit(1);
				return false;
            }

            if (!directories.Any(x => x.Name.Equals("definitions")))
            {
				Log.Warn($"The required definition files are not found. We sadly cannot legally provide you those. Any questions can be asked on Discord.");
				Environment.Exit(1);
                return false;
            }
			//report(ResourcePack.AsciiFont);

			Log.Info($"Loading bedrock resources...");
			BedrockResourcePack = new BedrockResourcePack(directory);

			EntityFactory.LoadModels(this, device, true, progressReceiver);


			Log.Info($"Loading known entity data...");
			EntityFactory.Load(progressReceiver);

			foreach (string file in setings.ResourcePacks)
			{
				try
				{
					string resourcePackPath = Path.Combine(ResourcePackDirectory, file);
					if (File.Exists(resourcePackPath))
					{
						Log.Info($"Loading resourcepack {file}...");

						using (FileStream stream = new FileStream(resourcePackPath, FileMode.Open))
						{
							ActiveResourcePacks.AddLast(LoadResourcePack(progressReceiver, device, stream, true, false, false, preloadCallback));
						}
					}
				}
				catch (Exception e)
				{
					Log.Warn(e, $"Could not load resourcepack {file}!");
				}
			}
			
            return true;
		}
	}

	public class Registries
	{
        [JsonProperty("minecraft:fluid")]
        public RegistryBase Fluids { get; set; }

        [JsonProperty("minecraft:block")]
		public RegistryBase Blocks { get; set; }

		[JsonProperty("minecraft:item")]
		public RegistryBase Items { get; set; }

        public class RegistryBase
		{
			[JsonProperty("default")]
			public string Default { get; set; } = null;

			[JsonProperty("protocol_id")]
			public int ProtocolId { get; set; }

			[JsonProperty("entries")]
			public IReadOnlyDictionary<string, RegistryEntry> Entries { get; set; }
		}

		public class RegistryEntry
		{
			[JsonProperty("protocol_id")]
			public int ProtocolId { get; set; }
		}
	}
}
