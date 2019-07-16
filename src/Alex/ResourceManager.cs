using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Alex.API.Data.Options;
using Alex.API.Json;
using Alex.API.Services;
using Alex.Entities;
using Alex.Networking.Java;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Generic;
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
		private IOptionsProvider Options { get; }
		public ResourceManager(GraphicsDevice graphics, IStorageSystem storageSystem, IOptionsProvider options)
		{
			Storage = storageSystem;
			Graphics = graphics;
			Atlas = new AtlasGenerator(Graphics);

			Options = options;
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
					Version version =
						manifest.Versions.FirstOrDefault(x => x.Id == JavaProtocol.VersionId);//.OrderByDescending(x => x.ReleaseTime.ToUnixTimeSeconds()).FirstOrDefault();

					var latestVersion = manifest.Versions.OrderByDescending(x => x.ReleaseTime.ToUnixTimeSeconds())
						.FirstOrDefault();
					
					Log.Info($"Using java assets version {version.Id} ({version.Type.ToString()})");

					if (version.ReleaseTime < latestVersion.ReleaseTime)
					{
						Log.Info($"Java assets outdated, latest version is: {latestVersion.Id} ({latestVersion.Type.ToString()})");
					}
					
					byte[] data;
					string savedPath = Path.Combine("assets", $"java-{version.Id}.zip");
					if (Storage.TryReadBytes(savedPath, out data))
					{
						return data;
					}
					else
					{
						Log.Info("Downloading Minecraft:Java edition assets...");
						LauncherMeta meta = LauncherMeta.FromJson(wc.DownloadString(version.Url));
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

		public bool TryLoadResourcePackInfo(string file, out ResourcePackManifest manifest)
		{
			manifest = default;
			if (!File.Exists(file))
			{
				return false;
			}

			ResourcePackLib.ResourcePack info;
			using(FileStream stream = File.OpenRead(file))
			using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
				info = ResourcePackLib.ResourcePack.LoadFromArchive(archive);

			if (info == null || info == default)
			{
				return false;
			}

			manifest = info.Info;
			return true;
		}

		private McResourcePack LoadResourcePack(IProgressReceiver progressReceiver, GraphicsDevice graphics, Stream stream, McResourcePack.McResourcePackPreloadCallback preloadCallback = null)
		{
			McResourcePack resourcePack = null;

			using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, false))
			{
				resourcePack = new McResourcePack(archive, graphics, preloadCallback);
			}

			Log.Info($"Loaded {resourcePack.BlockModels.Count} block models from resourcepack");
			Log.Info($"Loaded {resourcePack.ItemModels.Count} item models from resourcepack");

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

        private void LoadModels(IProgressReceiver progressReceiver, McResourcePack resourcePack, bool replaceModels,
            bool reportMissingModels)
        {
            Stopwatch sw = Stopwatch.StartNew();
            int imported = BlockFactory.LoadResources(this, resourcePack, replaceModels, reportMissingModels, progressReceiver);
            sw.Stop();

            Log.Info($"Imported {imported} blockstate variants from resourcepack in {sw.ElapsedMilliseconds}ms!");
        }

        private void LoadTextures(GraphicsDevice device, IProgressReceiver progressReceiver,
            McResourcePack resourcePack, bool isFirst)
        {
            if (!isFirst)
            {
                Atlas.LoadResourcePackOnTop(ActiveResourcePacks.First().TexturesAsBitmaps.Where(x => x.Key.StartsWith("block")).ToArray(), resourcePack.TexturesAsBitmaps.Where(x => x.Key.StartsWith("block")).ToArray(),
                    progressReceiver);
            }
            else
            {
                Atlas.GenerateAtlas(resourcePack.TexturesAsBitmaps.Where(x => x.Key.StartsWith("block")).ToArray(),
                    progressReceiver);


                //Atlas.Atlas.Save("atlas.png", ImageFormat.Png);
            }
        }

        private bool CheckRequiredPaths(out byte[] javaResources)
		{
            /*	if (!Storage.TryReadBytes(BedrockResourcePackPath, out bedrockResources))
                {
                    Log.Error(
                        $"Missing bedrock edition resources! Please put a copy of the bedrock resources in a zip archive with the path '{BedrockResourcePackPath}'");
                    javaResources = null;
                    bedrockResources = null;
                    return false;
                }*/

			try
			{
				javaResources = GetLatestAssets();
				if (javaResources == null)
				{
					Log.Error($"Could not load any assets! Are you connected to the internet?");

					javaResources = null;
					//bedrockResources = null;
					return false;
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Could not check for latests assets! Do you have a internet connection up?");
				javaResources = null;
				//bedrockResources = null;
				return false;
			}

			return true;
		}

        public DirectoryInfo ResourcePackDirectory { get; private set; } = null;
        public bool CheckResources(GraphicsDevice device, IProgressReceiver progressReceiver, McResourcePack.McResourcePackPreloadCallback preloadCallback)
		{
			byte[] defaultResources;

			if (!CheckRequiredPaths(out defaultResources))
			{
				return false;
			}

			Log.Info($"Loading registries...");
			progressReceiver?.UpdateProgress(0, "Loading registries...");
			Registries = JsonConvert.DeserializeObject<Registries>(ReadStringResource("Alex.Resources.registries.json"));
			progressReceiver?.UpdateProgress(100, "Loading registries...");

            Log.Info($"Loading vanilla resources...");
			using (MemoryStream stream = new MemoryStream(defaultResources))
			{
				ActiveResourcePacks.AddFirst(LoadResourcePack(progressReceiver, device, stream, preloadCallback));
			}

			var bedrockPath = Path.Combine("assets", "bedrock");

            DirectoryInfo directory;
            if (!Storage.TryGetDirectory(bedrockPath, out directory))
            {
	            Log.Warn($"The bedrock resources required to play this game are not set-up correctly!");
	            Console.ReadLine();
	            Environment.Exit(1);
	            return false;
            }

            /*if (!directories.Any(x => x.Name.Equals("models")))
            {
				Log.Warn($"Please make sure to extract the MC:Bedrock resource pack into \"{directory.FullName}\"");
                Console.ReadLine();
                Environment.Exit(1);
				return false;
            }*/

            if (directory.GetFileSystemInfos().Length == 0)
            {
	            Log.Info($"Extracting required resources...");
	            progressReceiver?.UpdateProgress(50, "Extracting resources...");
	            
	            byte[] zipped = ReadResource("Alex.Resources.resources.zip");
	            using (MemoryStream ms = new MemoryStream(zipped))
	            {
		            using (ZipArchive archive = new ZipArchive(ms, ZipArchiveMode.Read))
		            {
			            archive.ExtractToDirectory(directory.FullName);
		            }
	            }
            }
            
            var directories = directory.GetDirectories();

            if (!directories.Any(x => x.Name.Equals("definitions")))
            {
				Log.Warn($"The required definition files are not found. Any questions can be asked on Discord.");
                Console.ReadLine();
				Environment.Exit(1);
                return false;
            }
			//report(ResourcePack.AsciiFont);

			Log.Info($"Loading bedrock resources...");
			BedrockResourcePack = new BedrockResourcePack(directory);

			EntityFactory.LoadModels(this, device, true, progressReceiver);


			Log.Info($"Loading known entity data...");
			EntityFactory.Load(this, progressReceiver);

            Storage.TryGetDirectory(Path.Combine("assets", "resourcepacks"), out DirectoryInfo root);
            ResourcePackDirectory = root;
            
            foreach (string file in Options.AlexOptions.ResourceOptions.LoadedResourcesPacks.Value)
			{
				try
				{
					string resourcePackPath = Path.Combine(root.FullName, file);
					if (File.Exists(resourcePackPath))
					{
						Log.Info($"Loading resourcepack {file}...");

						using (FileStream stream = new FileStream(resourcePackPath, FileMode.Open))
						{
							ActiveResourcePacks.AddLast(LoadResourcePack(progressReceiver, device, stream, null));
						}
					}
				}
				catch (Exception e)
				{
					Log.Warn(e, $"Could not load resourcepack {file}: {e.ToString()}");
				}
			}

            bool isFirst = true;
            foreach (var resourcePack in ActiveResourcePacks)
            {
                LoadTextures(device, progressReceiver, resourcePack, isFirst);

                if (isFirst)
                    isFirst = false;
            }

            isFirst = true;
            foreach (var resourcePack in ActiveResourcePacks)
            {
                LoadModels(progressReceiver, resourcePack, true, isFirst);
                if (isFirst)
                    isFirst = false;
            }

            return true;
		}

		public static string ReadStringResource(string resource)
		{
			return Encoding.UTF8.GetString(ReadResource(resource));
		}
		
		public static byte[] ReadResource(string resource)
		{
			var fontStream = Assembly.GetEntryAssembly().GetManifestResourceStream(resource);
			return fontStream.ReadAllBytes();
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

		[JsonProperty("minecraft:entity_type")]
		public RegistryBase Entities { get; set; }

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
