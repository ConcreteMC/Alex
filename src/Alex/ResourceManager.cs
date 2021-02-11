using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Alex.API.Data.Options;
using Alex.API.Graphics;
using Alex.API.Resources;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Blocks;
using Alex.Blocks.Mapping;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Entities;
using Alex.Entities.BlockEntities;
using Alex.Gamestates;
using Alex.Graphics;
using Alex.Graphics.Effect;
using Alex.Graphics.Models.Blocks;
using Alex.Gui;
using Alex.Items;
using Alex.Networking.Java;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Generic;
using Alex.ResourcePackLib.IO;
using Alex.ResourcePackLib.IO.Abstract;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Utils.Assets;
using JetBrains.Profiler.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using DateTime = System.DateTime;
using ResourceLocation = Alex.API.Resources.ResourceLocation;
using Task = System.Threading.Tasks.Task;

namespace Alex
{
	public class ResourceManager
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ResourceManager));

		private LinkedList<McResourcePack> ActiveResourcePacks { get; } = new LinkedList<McResourcePack>();
		private LinkedList<BedrockResourcePack> ActiveBedrockResourcePacks { get; } = new LinkedList<BedrockResourcePack>();
		public BedrockResourcePack BedrockResourcePack => ActiveBedrockResourcePacks.First?.Value;
		public  Registries                 Registries          { get; private set; }
		public  AtlasGenerator             Atlas               { get; private set; }

		private IStorageSystem Storage { get; }
		private IOptionsProvider Options { get; }
		private IRegistryManager RegistryManager { get; }
		private Alex Alex { get; }
		private MCJavaAssetsUtil AssetsUtil { get; }
		private MCBedrockAssetUtils BedrockAssetUtil { get; }

		private ContentManager ContentManager { get; }
		
		public static Effect EntityEffect { get; set; }
		public static Effect BlockEffect { get; set; }
		public static Effect LightingEffect { get; set; }
		
		public List<MCPack>        Packs               { get; } = new List<MCPack>();
		public BlockModelRegistry  BlockModelRegistry  { get; private set; }
		public EntityModelRegistry EntityModelRegistry { get; private set; }
		
		public static PooledTexture2D NethergamesLogo { get; private set; }
		public ResourceManager(IServiceProvider serviceProvider)
		{
			Atlas = new AtlasGenerator();
			Storage = serviceProvider.GetService<IStorageSystem>();

			Options = serviceProvider.GetService<IOptionsProvider>();
			RegistryManager = serviceProvider.GetService<IRegistryManager>();
			Alex = serviceProvider.GetService<Alex>();
			ContentManager = serviceProvider.GetService<ContentManager>();
			
			AssetsUtil = new MCJavaAssetsUtil(Storage); //ContentManager.Load<byte[]>();
			BedrockAssetUtil = new MCBedrockAssetUtils(Storage);
		}

		private void ResourcePacksChanged(string[] oldvalue, string[] newvalue)
		{
			Log.Info($"Resource packs changed.");
			
			SplashScreen splashScreen = new SplashScreen();
			Alex.GameStateManager.SetActiveState(splashScreen);

			Task.Run(() =>
			{
				LoadResourcePacks(Alex.GraphicsDevice, splashScreen, newvalue);
				Alex.GameStateManager.Back();
			});
		}

		public bool TryLoadResourcePackInfo(string file, out ResourcePackManifest manifest)
		{
			manifest = default;
			if (!File.Exists(file))
			{
				return false;
			}
			
			using(FileStream stream = File.OpenRead(file))
			using (var archive = new ZipFileSystem(stream, Path.GetFileNameWithoutExtension(file)))
			{
				manifest = ResourcePackLib.ResourcePack.GetManifest(archive);

				if (manifest != null)
					manifest.Name = archive.Name;
			}

			if (manifest == null)
			{
				return false;
			}

			return true;
		}

		private ResourcePack LoadResourcePack(IProgressReceiver progressReceiver, IFilesystem fs, McResourcePack.McResourcePackPreloadCallback preloadCallback = null)
		{
			Stopwatch sw = Stopwatch.StartNew();
			
			Log.Info($"Loading resource pack ({fs.Name})...");

			try
			{
				var manifest = ResourcePackLib.ResourcePack.GetManifest(fs);

				if (manifest == null || manifest.Type == ResourcePackType.Java)
				{
					McResourcePack resourcePack = new McResourcePack(
						fs, preloadCallback, (percentage, file) =>
						{
							progressReceiver?.UpdateProgress(percentage, null, file);
						});
					
					sw.Stop();
			
					Log.Info($"Loaded {resourcePack.BlockModels.Count} block models from resourcepack");
					Log.Info($"Loaded {resourcePack.ItemModels.Count} item models from resourcepack");
					Log.Info($"Loading resourcepack took: {sw.ElapsedMilliseconds}ms");

					return resourcePack;
				}
				else if (manifest.Type == ResourcePackType.Bedrock)
				{
					BedrockResourcePack brp = new BedrockResourcePack(
						fs, (percentage, file) =>
						{
							progressReceiver?.UpdateProgress(percentage, null, file);
						});

					return brp;
				}
			}
			catch (Exception ex)
			{
				Log.Warn(ex, $"Failed to load.");
			}

			return null;
		}

		private void LoadModels(IProgressReceiver progressReceiver, McResourcePack resourcePack, bool replaceModels,
            bool reportMissingModels)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var modelRegistry = RegistryManager.GetRegistry<ResourcePackModelBase>();

            int imported = 0;
            if (modelRegistry is BlockModelRegistry bmr)
            {
	            imported = bmr.LoadResourcePack(progressReceiver, resourcePack, replaceModels);
            }

            Log.Info($"Imported {imported} block models from resourcepack in {sw.ElapsedMilliseconds}ms!");
            
            sw.Stop();
        }
		
        private bool CheckBedrockAssets(IProgressReceiver progressReceiver, out string bedrockResources)
        {
	        bedrockResources = null;
	        
			try
			{
				string targetPath = Path.Combine("assets", "bedrock");
				string bedrockPath;
				if (!BedrockAssetUtil.CheckUpdate(progressReceiver, out bedrockPath))
				{
					if (Storage.TryGetDirectory(targetPath, out var directoryInfo))
					{
						bedrockResources = directoryInfo.FullName;
						return true;
					}
				}
				
				if (string.IsNullOrWhiteSpace(bedrockPath) || !Storage.Exists(bedrockPath))
				{
					Log.Warn("Could not load any of the required Bedrock assets! Are you connected to the internet?");
					Log.Warn($"A manual fix is available, see: https://github.com/kennyvv/Alex/wiki/Bedrock-Assets");
					return false;
				}
				
				Storage.TryDeleteDirectory(targetPath);

				if (Storage.TryCreateDirectory(targetPath) && Storage.TryGetDirectory(targetPath, out var di))
				{
					using (ZipArchive zipArchive =
						new ZipArchive(Storage.OpenFileStream(bedrockPath, FileMode.Open)))
					{
						zipArchive.ExtractToDirectory(di.FullName);
					}

					bedrockResources = di.FullName;
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Could not check for latests assets! Are you connected to the internet?");
				return false;
			}
			
			return false;
		}

        private bool CheckJavaAssets(IProgressReceiver progressReceiver, out string javaResources)
        {
	        try
	        {
		        string assetDirectory    = Path.Combine("assets", "java");

		        string storedVersion;
		        AssetsUtil.TryGetStoredVersion(out storedVersion);

		        DirectoryInfo directoryInfo = null;
		        if (storedVersion == null || !storedVersion.Equals(JavaProtocol.VersionId) || !Storage.TryGetDirectory(assetDirectory, out directoryInfo))
		        {
			        Storage.TryDeleteDirectory(assetDirectory);
				        
			        var zipPath = AssetsUtil.EnsureTargetReleaseAsync(JavaProtocol.VersionId, progressReceiver)
					       .Result;

			        if (Storage.TryCreateDirectory(assetDirectory)
			            && Storage.TryGetDirectory(assetDirectory, out directoryInfo))
			        {
				        Log.Info($"Extracting resources....");
				        using (ZipArchive zipArchive = new ZipArchive(Storage.OpenFileStream(zipPath, FileMode.Open)))
				        {
					        zipArchive.ExtractToDirectory(directoryInfo.FullName, true);
				        }
			        }
		        }

		        if (directoryInfo != null)
		        {
			        javaResources = directoryInfo.FullName;

			        return true;
		        }

		        javaResources = null;
		        return false;
	        }
	        catch (Exception ex)
	        {
		        Log.Error(ex, $"Could not check for latests assets! Are you connected to the internet?");
		        javaResources = null;
		        //bedrockResources = null;
		        return false;
	        }

	        return true;
        }

        public string DeviceID { get; private set; } = null;
        private void LoadHWID()
        {
	        DeviceID = Guid.NewGuid().ToString();
        }
        
        public  DirectoryInfo                                SkinPackDirectory     { get; private set; } = null;
        public  DirectoryInfo                                ResourcePackDirectory { get; private set; } = null;
        private McResourcePack.McResourcePackPreloadCallback PreloadCallback       { get; set; }
        public  bool Asynchronous => true;//ActiveResourcePacks.All(x => x.Asynchronous);

        public bool CheckResources(GraphicsDevice device, IProgressReceiver progressReceiver, McResourcePack.McResourcePackPreloadCallback preloadCallback)
        {
	        LoadHWID();
	        
	        PreloadCallback = preloadCallback;
	        
	        Log.Info($"Loading registries...");
	        progressReceiver?.UpdateProgress(0, "Loading registries...");
	        Registries = JsonConvert.DeserializeObject<Registries>(ReadStringResource("Alex.Resources.registries.json"));
	        progressReceiver?.UpdateProgress(100, "Loading registries...");
	        
			string defaultResources;
			string defaultBedrock;

			if (!CheckJavaAssets(progressReceiver, out defaultResources))
			{
				return false;
			}

			if (!CheckBedrockAssets(progressReceiver, out defaultBedrock))
			{
				return false;
			}

			progressReceiver?.UpdateProgress(0, "Loading vanilla resources...");
			
	        var vanilla = LoadResourcePack(progressReceiver, new DiskFileSystem(defaultResources), preloadCallback);
	        if (vanilla.Info != null)
				vanilla.Info.Name = "Vanilla";
				
	        ActiveResourcePacks.AddFirst((McResourcePack)vanilla);

	        // Log.Info($"Loading bedrock resources...");
			
			progressReceiver?.UpdateProgress(0, "Loading bedrock resources...");

			var vanillaBedrock = LoadResourcePack(progressReceiver, new DiskFileSystem(defaultBedrock));
			ActiveBedrockResourcePacks.AddFirst((BedrockResourcePack) vanillaBedrock);
			Alex.AudioEngine.Initialize((BedrockResourcePack) vanillaBedrock);
			//Log.Info($"Loading known entity data...");

			Storage.TryGetDirectory(Path.Combine("assets", "resourcepacks"), out DirectoryInfo root);
            ResourcePackDirectory = root;

            LoadRegistries(progressReceiver);
            
	        LoadResourcePacks(
		        device, progressReceiver, Options.AlexOptions.ResourceOptions.LoadedResourcesPacks.Value);
	        
	        EntityFactory.Load(this, progressReceiver);

	        ItemFactory.Init(RegistryManager, this, progressReceiver);
			
	        BlockEntityFactory.LoadResources(device, this);
            
            if (Storage.TryGetDirectory(Path.Combine("assets", "bedrockpacks"), out DirectoryInfo info))
            {
	            SkinPackDirectory = info;
	            LoadBedrockPacks(progressReceiver, info);
            }
            else
            {
	            if (Storage.TryCreateDirectory(Path.Combine("assets", "bedrockpacks")))
	            {
		            if (Storage.TryGetDirectory(Path.Combine("assets", "bedrockpacks"), out var dirInfo))
		            {
			            SkinPackDirectory = dirInfo;
		            }
	            }
            }
            
            BlockMapper.Init(progressReceiver);
            
            Options.AlexOptions.ResourceOptions.LoadedResourcesPacks.Bind(ResourcePacksChanged);
            _hasInit = true;
            
            var data = ReadResource("Alex.Resources.nethergames.png");
            NethergamesLogo = TextureUtils.BitmapToTexture2D(Alex.GraphicsDevice, Image.Load(data));
	            
            return true;
		}

        public void LoadBedrockPacks(IProgressReceiver progressReceiver, DirectoryInfo directoryInfo)
        {
	        progressReceiver?.UpdateProgress(0, "Loading bedrock .MCPack files...");

	        var files = directoryInfo.EnumerateFiles("*.mcpack").ToArray();

	        for (var index = 0; index < files.Length; index++)
	        {
		        var file = files[index];
		        progressReceiver?.UpdateProgress(index, files.Length, "Loading bedrock .MCPack files...", file.Name);

		        try
		        {
			        using (var archive = new ZipFileSystem(file.Open(FileMode.Open, FileAccess.Read), file.Name))
			        {
				        MCPack pack = new MCPack(archive);
				        Packs.Add(pack);
			        }
		        }
		        catch (Exception ex)
		        {
			        Log.Warn(ex, $"Failed to load bedrock .MCPack file: {file.Name}: {ex}");
		        }
	        }
        }

        private bool _hasInit = false;
        private void LoadResourcePacks(GraphicsDevice device, IProgressReceiver progress, string[] resourcePacks)
        {
	        var countBefore = ActiveResourcePacks.Count;
	        
	        var first  = ActiveResourcePacks.First.Value;
	        var before = ActiveResourcePacks.ToArray();

	        foreach (var item in before.Skip(1))
	        {
		        item.Dispose();
	        }
	        
	        ActiveResourcePacks.Clear();

	        ActiveResourcePacks.AddFirst(first);

	        if (_hasInit)
	        {
		        PreloadCallback?.Invoke(first.FontBitmap, McResourcePack.BitmapFontCharacters.ToList());
		        Atlas.Reset();
	        }
	        
	        foreach (string file in resourcePacks)
	        {
		        try
		        {
			        string resourcePackPath = Path.Combine(ResourcePackDirectory.FullName, file);
			        if (File.Exists(resourcePackPath))
			        {
				        //Log.Info($"Loading resourcepack {file}...");

				        //using (FileStream stream = new FileStream(resourcePackPath, FileMode.Open))
				        {
					       // using (ZipFileSystem zipFileSystem = )
					        {
						        var pack = LoadResourcePack(progress, new ZipFileSystem(new FileStream(resourcePackPath, FileMode.Open), Path.GetFileNameWithoutExtension(resourcePackPath)), null);

						        if (pack is McResourcePack javaPack)
						        {
							        if (pack.Info != null && string.IsNullOrWhiteSpace(pack.Info.Name))
							        {
								        pack.Info.Name = Path.GetFileNameWithoutExtension(file);
							        }

							        ActiveResourcePacks.AddLast(javaPack);
						        }
						        else if (pack is BedrockResourcePack bedrockPack)
						        {
							        ActiveBedrockResourcePacks.AddLast(bedrockPack);
						        }
					        }
				        }
			        }
		        }
		        catch (Exception e)
		        {
			        Log.Warn(e, $"Could not load resourcepack {file}: {e.ToString()}");
		        }
	        }

	        var  active  = ActiveResourcePacks.ToArray();
	        bool isFirst = true;
	        foreach (var resourcePack in active)
	        {
		        Alex.GuiRenderer.LoadLanguages(resourcePack, progress);
		        
		        LoadModels(progress, resourcePack, !isFirst, isFirst);
		        if (isFirst)
		        { //Only load models for vanilla until above we fix blockstate replacement issues.
			        isFirst = false;
			       // break;
		        }
	        }
	        
	        Stopwatch sw = Stopwatch.StartNew();
	        MeasureProfiler.StartCollectingData();
	        var imported = BlockFactory.LoadBlockstates(RegistryManager, this, false, false, progress);
	        MeasureProfiler.SaveData();
            
	        MeasureProfiler.StopCollectingData();
            
	        Log.Info($"Imported {imported} blockstates from resourcepack in {sw.ElapsedMilliseconds}ms!");

	        var textures = new Dictionary<ResourceLocation, AtlasGenerator.ImageEntry>();
	        for (var index = 0; index < active.Length; index++)
	        {
		        var resourcePack = active[index];
		        
		        progress?.UpdateProgress(0, $"Loading textures: {resourcePack.Info?.Name ?? "Unknown"}");
	        
		        Atlas.LoadResourcePackOnTop(device,
			        textures,
			        resourcePack,
			        progress, index == active.Length - 1);
		        //LoadTextures(device, progress, resourcePack, ref textures, index == active.Length - 1);
	        }

	        foreach (var resourcePack in ActiveBedrockResourcePacks)
	        {
		        LoadEntityModels(resourcePack, progress);
		        int modelCount = EntityFactory.LoadModels(resourcePack, this, device, true, progress);

		        Log.Info($"Imported {modelCount} entity models...");
	        }

	        progress?.UpdateProgress(0, $"Loading UI textures...");
	        Alex.GuiRenderer.LoadResourcePackTextures(this, progress);

	        progress?.UpdateProgress(50, "Loading language...");
	        if (!Alex.GuiRenderer.SetLanguage(Options.AlexOptions.MiscelaneousOptions.Language.Value))
	        {
		        Alex.GuiRenderer.SetLanguage(CultureInfo.InstalledUICulture.Name);
	        }
	        
	        progress?.UpdateProgress(100, "Loading language...");
	        
	        var f = ActiveResourcePacks.LastOrDefault(x => x.FontBitmap != null);
	        if (f != null)
	        {
		        PreloadCallback?.Invoke(f.FontBitmap, McResourcePack.BitmapFontCharacters.ToList());
	        }
        }

        private void LoadEntityModels(BedrockResourcePack resourcePack, IProgressReceiver progressReceiver = null)
        {
	        Stopwatch sw            = Stopwatch.StartNew();
	        var       modelRegistry = EntityModelRegistry;

	        int imported = 0;
	        imported = modelRegistry.LoadResourcePack(progressReceiver, resourcePack, true);

	        Log.Info($"Imported {imported} block models from resourcepack in {sw.ElapsedMilliseconds}ms!");
            
	        sw.Stop();
        }
        
        private void LoadRegistries(IProgressReceiver progress)
        {
	        progress.UpdateProgress(0, "Loading block model registry...");
	        RegistryManager.AddRegistry<BlockModelEntry, ResourcePackModelBase>(BlockModelRegistry = new BlockModelRegistry());
	        
	        progress.UpdateProgress(0, "Loading entity model registry...");
	        RegistryManager.AddRegistry<EntityModelEntry, EntityModel>(EntityModelRegistry = new EntityModelRegistry());
	        
	        progress.UpdateProgress(0, "Loading blockstate registry...");
	        RegistryManager.AddRegistry(new RegistryBase<BlockState>("blockstate"));
	        
	        progress.UpdateProgress(50, "Loading block registry...");
	        RegistryManager.AddRegistry(new BlockRegistry());
        }
        
        public static string ReadStringResource(string resource, IProgressReceiver progressReceiver = null)
        {
	        return Encoding.UTF8.GetString(ReadResource(resource, progressReceiver));
        }

		public static byte[] ReadResource(string resource, IProgressReceiver progressReceiver = null)
		{
			progressReceiver?.UpdateProgress(0, null, $"Loading resource: {resource}");
			using (MemoryStream ms = new MemoryStream())
			{
				byte[] buffer = new byte[128];
				using (var reader = Assembly.GetEntryAssembly().GetManifestResourceStream(resource))
				{
					int count;

					while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
					{
						ms.Write(buffer, 0, count);
						progressReceiver?.UpdateProgress((int) ms.Length, (int) reader.Length, null, $"Loading resource: {resource}");
					}
				}

				return ms.ToArray();
			}

			//return fontStream.ReadAllBytes();
		}

		public bool TryGetBitmap(ResourceLocation location, out Image<Rgba32> bitmap)
		{
			bitmap = null;
			foreach (var resourcePack in ActiveResourcePacks.Reverse())
			{
				if (resourcePack.TryGetBitmap(location, out var f))
				{
					bitmap = f;
					return true;
				}
			}

			return false;
		}
		
		public bool TryGetBedrockBitmap(ResourceLocation location, out Image<Rgba32> bitmap)
		{
			bitmap = null;
			foreach (var resourcePack in ActiveBedrockResourcePacks.Reverse())
			{
				if (resourcePack.TryGetTexture(location, out var f))
				{
					bitmap = f;
					return true;
				}
			}

			return false;
		}

		public bool TryGetBlockState(string location, out BlockStateResource resource)
		{
			resource = null;
			foreach (var resourcePack in ActiveResourcePacks.Reverse())
			{
				if (resourcePack.TryGetBlockState(location, out resource))
				{
					return true;
				}
			}

			return false;
		}

		public Microsoft.Xna.Framework.Color GetGrassColor(float temp, float rain, int elevation)
		{
			foreach (var resourcePack in ActiveResourcePacks.Reverse())
			{
				if (resourcePack.TryGetGrassColor(temp, rain, elevation, out var color))
				{
					return color;
				}
			}

			return new Microsoft.Xna.Framework.Color(94, 157, 52);
		}
		
		public Microsoft.Xna.Framework.Color GetFoliageColor(float temp, float rain, int elevation)
		{
			foreach (var resourcePack in ActiveResourcePacks.Reverse())
			{
				if (resourcePack.TryGetFoliageColor(temp, rain, elevation, out var color))
				{
					return color;
				}
			}
			
			return new Microsoft.Xna.Framework.Color(94, 157, 52);
		}

		public bool TryGetItemModel(ResourceLocation key, out ResourcePackModelBase model)
		{
			model = null;
			foreach (var resourcePack in ActiveResourcePacks.Reverse())
			{
				if (resourcePack.ItemModels.TryGetValue(key, out model))
				{
					return true;
				}
			}

			return false;
		}

		public bool TryGetBlockModel(ResourceLocation key, out ResourcePackModelBase model)
		{
			model = null;
			foreach (var resourcePack in ActiveResourcePacks.Reverse())
			{
				if (resourcePack.BlockModels.TryGetValue(key, out model))
				{
					return true;
				}
			}

			return false;
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
