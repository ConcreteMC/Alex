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
using Alex.Blocks;
using Alex.Blocks.Mapping;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Common.Graphics.GpuResources;
using Alex.Common.Resources;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Entities;
using Alex.Entities.BlockEntities;
using Alex.Gamestates;
using Alex.Graphics;
using Alex.Graphics.Effect;
using Alex.Graphics.Models.Blocks;
using Alex.Gui;
using Alex.Items;
using Alex.Networking.Java;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Abstraction;
using Alex.ResourcePackLib.Generic;
using Alex.ResourcePackLib.IO;
using Alex.ResourcePackLib.IO.Abstract;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.ResourcePackLib.Json.Textures;
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
using ResourceLocation = Alex.Common.Resources.ResourceLocation;
using Task = System.Threading.Tasks.Task;

namespace Alex
{
	public class ResourceManager : ITextureProvider
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ResourceManager));

		private LinkedList<McResourcePack> ActiveResourcePacks { get; } = new LinkedList<McResourcePack>();
		private LinkedList<BedrockResourcePack> ActiveBedrockResourcePacks { get; } = new LinkedList<BedrockResourcePack>();
		public IReadOnlyCollection<BedrockResourcePack> ActiveBedrockResources => ActiveBedrockResourcePacks;
		public BedrockResourcePack BedrockResourcePack => ActiveBedrockResourcePacks.First?.Value;
		public  Registries                 Registries          { get; private set; }
		public  AtlasGenerator             BlockAtlas               { get; private set; }
		public  AtlasGenerator             ItemAtlas               { get; private set; }
		
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
		
		public List<MCPack>        SkinPacks               { get; } = new List<MCPack>();
		public BlockModelRegistry  BlockModelRegistry  { get; private set; }
		public EntityModelRegistry EntityModelRegistry { get; private set; }
		
		
		public static ManagedTexture2D NethergamesLogo { get; private set; }
		
		/*private readonly Dictionary<ResourceLocation, ResourcePackModelBase>   _models    = new Dictionary<ResourceLocation, ResourcePackModelBase>();
		public IReadOnlyDictionary<ResourceLocation, ResourcePackModelBase> BlockModels =>
			_models.Where(x => x.Value.Type == ModelType.Block)
			   .ToDictionary(x => x.Key, x => x.Value);
		
		public IReadOnlyDictionary<ResourceLocation, ResourcePackModelBase>   ItemModels        => _models.Where(x => x.Value.Type == ModelType.Item)
		   .ToDictionary(x => x.Key, x => x.Value);*/
		
		public ResourceManager(IServiceProvider serviceProvider)
		{
			BlockAtlas = new AtlasGenerator("atlases/blocks/");
			ItemAtlas = new AtlasGenerator("atlases/items/");
			
			Storage = serviceProvider.GetService<IStorageSystem>();

			Options = serviceProvider.GetService<IOptionsProvider>();
			RegistryManager = serviceProvider.GetService<IRegistryManager>();
			Alex = serviceProvider.GetService<Alex>();
			ContentManager = serviceProvider.GetService<ContentManager>();
			
			AssetsUtil = new MCJavaAssetsUtil(Storage); //ContentManager.Load<byte[]>();
			BedrockAssetUtil = new MCBedrockAssetUtils(Storage);
		}

		private ResourcePackModelBase ProcessModel(ResourceLocation resourceLocation,
			ResourcePackModelBase model,
			ref Dictionary<ResourceLocation, ResourcePackModelBase> models)
		{
			if (model.ParentName != null && !model.ParentName.Equals(resourceLocation))
			{
				ResourceLocation parentKey = model.ParentName;
				
				ResourcePackModelBase parent;

				if (BlockModelRegistry.TryGet(parentKey, out var rawParent))
				{
					parent = rawParent.Value;
					//else if (TryGetBlockModel(parentKey, out parent)){}
				}
				else if (models.TryGetValue(parentKey, out parent))
				{
					parent = ProcessModel(parentKey, parent, ref models);
				}

				if (parent != null)
				{
					model.UpdateValuesFromParent(parent);
					//model.Parent = parent;
				}
			}

			var entry = new BlockModelEntry(model);
			BlockModelRegistry.Set(resourceLocation, () => entry);

			return model;
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

		public bool TryLoadResourcePackInfo(string file, out ResourcePackManifest[] manifests)
		{
			manifests = default;
			if (!File.Exists(file))
			{
				return false;
			}
			
			using(FileStream stream = File.OpenRead(file))
			using (var archive = new ZipFileSystem(stream, Path.GetFileNameWithoutExtension(file)))
			{
				manifests = ResourcePackLib.ResourcePack.GetManifests(archive).ToArray();

				if (manifests != null)
				{
					for (int i = 0; i < manifests.Length; i++)
					{
						if (manifests[i] != null && string.IsNullOrWhiteSpace(manifests[i].Name))
						{
							manifests[i].Name = archive.Name;
						}
					}
				}
			}

			if (manifests == null)
			{
				return false;
			}

			return true;
		}

		internal void ReloadTextures(IProgressReceiver progress)
		{
			ProcessEntityModels(progress);
			ProcessBedrockResourcePacks(progress);
			
			progress?.UpdateProgress(0, $"Loading UI textures...");
			Alex.GuiRenderer.LoadResourcePackTextures(this, progress);
		}
		
		internal bool Remove(ResourcePack resourcePack)
		{
			if (resourcePack.Info.Type == ResourcePackType.Bedrock)
			{
				if (ActiveBedrockResourcePacks.Remove((BedrockResourcePack) resourcePack))
				{
					return true;
				}
			}
			
			if (resourcePack.Info.Type == ResourcePackType.Java)
			{
				if (ActiveResourcePacks.Remove((McResourcePack) resourcePack))
				{
					return true;
				}
			}

			return false;
		}
		
		internal IEnumerable<BedrockResourcePack> LoadBedrockTexturePack(IFilesystem fs, IProgressReceiver progress = null, string contentKey = null)
		{
			//var fs = new ZipFileSystem(new MemoryStream(data), "servertextures");

			foreach (var resourcePack in LoadResourcePack(progress, fs, null))
			{
				if (resourcePack.Info.Type == ResourcePackType.Bedrock)
				{
					var pack = (BedrockResourcePack) resourcePack;
					pack.ContentKey = contentKey;
					
					ActiveBedrockResourcePacks.AddLast(pack);

					yield return pack;
				}
			}
		}

		private IEnumerable<ResourcePack> LoadResourcePack(IProgressReceiver progressReceiver,
			IFilesystem fs,
			McResourcePack.McResourcePackPreloadCallback preloadCallback = null)
		{
			Stopwatch sw = Stopwatch.StartNew();

			//try
			//{
			foreach (var manifest in ResourcePackLib.ResourcePack.GetManifests(fs))
			{
				if (manifest == null)
					continue;
				
				if (manifest.Type == ResourcePackType.Java)
				{
					McResourcePack resourcePack = new McResourcePack(
						fs, preloadCallback, manifest,
						(percentage, file) => { progressReceiver?.UpdateProgress(percentage, null, file); });

					sw.Stop();

					//Log.Debug($"Loaded {resourcePack.BlockModels.Count} block models from resourcepack");
					//	Log.Debug($"Loaded {resourcePack.ItemModels.Count} item models from resourcepack");
					Log.Info($"Loading resourcepack \"{(string.IsNullOrWhiteSpace(manifest.Name) ? fs.Name : manifest.Name)}\" took: {sw.ElapsedMilliseconds}ms");

					sw.Restart();
					yield return resourcePack;
				}
				else if (manifest.Type == ResourcePackType.Bedrock)
				{
					BedrockResourcePack brp = new BedrockResourcePack(
						fs, manifest,
						(percentage, file) => { progressReceiver?.UpdateProgress(percentage, null, file); });

					sw.Stop();

				//	Log.Debug($"Loaded {brp.EntityModels.Count} entity models from resourcepack");
					Log.Info($"Loading resourcepack \"{manifest.Name}\" took: {sw.ElapsedMilliseconds}ms");

					sw.Restart();
					yield return brp;
				}
			}
			//}
			//	catch (Exception ex)
			//	{
			//		Log.Warn(ex, $"Failed to load resourcepack ({fs.Name})");
			//	}


		}

		private static string ResolveTexture(ResourcePackModelBase var, string texture)
		{
			if (texture[0] != '#')
				return texture;

			var original = texture;
			var modified = texture.Substring(1);
			if (var.Textures.TryGetValue(modified, out texture))
			{
				if (texture[0] == '#')
				{
					if (!var.Textures.TryGetValue(texture.Substring(1), out texture))
					{
						//texture = "no_texture";
						return original;
					}
				}
			}

			return texture;
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

        public bool CheckResources(GraphicsDevice device, IProgressReceiver progressReceiver, McResourcePack.McResourcePackPreloadCallback preloadCallback)
        {
	        LoadHWID();
	        
	        PreloadCallback = preloadCallback;
	        
	        Log.Info($"Loading registries...");
	        progressReceiver?.UpdateProgress(0, "Loading registries...");
	        Registries = JsonConvert.DeserializeObject<Registries>(ReadStringResource("Alex.Resources.registries.json"));
	        ParticlePacket.RegistryLookup = particleId =>
		      Registries.Particles.Entries.FirstOrDefault(
			        x => x.Value.ProtocolId == particleId).Key; 
	        
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

			List<ResourcePack> resourcePacks = new List<ResourcePack>();

			progressReceiver?.UpdateProgress(0, "Loading vanilla resources...");

			foreach (var vanilla in LoadResourcePack(
				progressReceiver, new DiskFileSystem(defaultResources), preloadCallback))
			{
				resourcePacks.Add(vanilla);
			}

			// Log.Info($"Loading bedrock resources...");
			
			progressReceiver?.UpdateProgress(0, "Loading bedrock resources...");
			foreach (var vanilla in LoadResourcePack(progressReceiver, new DiskFileSystem(defaultBedrock)))
			{
				resourcePacks.Add(vanilla);
			}

			foreach (var vanilla in LoadResourcePack(
				progressReceiver,
				new ZipFileSystem(new MemoryStream(ReadResource("Alex.Resources.AlexDefault.zip")), "Alex")))
			{
				resourcePacks.Add(vanilla);
			}

			foreach (var pack in resourcePacks)
			{
				if (pack.Info.Type == ResourcePackType.Java)
				{
					if (pack.Info != null && string.IsNullOrWhiteSpace(pack.Info.Name))
						pack.Info.Name = "Vanilla";

					ActiveResourcePacks.AddLast((McResourcePack) pack);
				}
				else if (pack.Info.Type == ResourcePackType.Bedrock)
				{
					if (pack.Info != null && string.IsNullOrWhiteSpace(pack.Info.Name))
						pack.Info.Name = "Vanilla Bedrock";

					ActiveBedrockResourcePacks.AddLast((BedrockResourcePack) pack);
				}
			}
			//Log.Info($"Loading known entity data...");

			Storage.TryGetDirectory(Path.Combine("assets", "resourcepacks"), out DirectoryInfo root);
            ResourcePackDirectory = root;

            LoadRegistries(progressReceiver);
            
	        LoadResourcePacks(
		        device, progressReceiver, Options.AlexOptions.ResourceOptions.LoadedResourcesPacks.Value);

	        ProcessResources(device, progressReceiver);
	        
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
            NethergamesLogo = TextureUtils.BitmapToTexture2D(this, Alex.GraphicsDevice, Image.Load(data));
	            
            return true;
		}

        private void ProcessEntityModels(IProgressReceiver progress)
        {
	        Dictionary<string, EntityModel> entityModels = new Dictionary<string, EntityModel>();
	        var activeBedrockPacks = ActiveBedrockResourcePacks.ToArray();

	        for (int index = 0; index < activeBedrockPacks.Length; index++)
	        {
		        var resourcePack = activeBedrockPacks[index];

		        foreach (var model in resourcePack.EntityModels)
		        {
			        entityModels[model.Key] = model.Value;
		        }
	        }

	        entityModels = BedrockResourcePack.ProcessEntityModels(entityModels);
	        foreach(var model in entityModels)
	        {
		        EntityModelRegistry.Set(model.Key, () => new EntityModelEntry(model.Value));
	        }
        }

        private void ProcessBedrockResourcePacks(IProgressReceiver progress)
        {
	        var activeBedrockPacks = ActiveBedrockResourcePacks.ToArray();

	        for (int index = 0; index < activeBedrockPacks.Length; index++)
	        {
		        var resourcePack = activeBedrockPacks[index];
		        ProcessBedrockResources(progress, Alex.GraphicsDevice, resourcePack);
		        
		        //LoadEntityModels(resourcePack, progress);
		        
	        }
        }
        
        private void ProcessResources(GraphicsDevice device, IProgressReceiver progress)
        {
	        Dictionary<ResourceLocation, ResourcePackModelBase> models =
		        new Dictionary<ResourceLocation, ResourcePackModelBase>();
	        var  active  = ActiveResourcePacks.ToArray();

	        Stopwatch modelTimer = Stopwatch.StartNew();
	        foreach (var resourcePack in active)
	        {
		        foreach (var model in resourcePack.Models)
		        {
			        models[model.Key] = model.Value;
		        }
	        }

	        int modelsProcessed = 0;
	        foreach (var model in models.OrderBy(
		        x =>
		        {
			        if (x.Value.ParentName == null)
				        return 0;

			        return 1;
		        }))
	        {
		        progress?.UpdateProgress(modelsProcessed, models.Count, model.Key.ToString());
		        ProcessModel(model.Key, model.Value, ref models);

		        modelsProcessed++;
	        }
	        
	        foreach (var m in models.Where(x => x.Value.Type == ModelType.Block)
		       .OrderByDescending(x => x.Value.ParentName != null))
	        {
		        var model = m.Value;

		        if (model.Elements != null)
		        {
			        for (var index = 0; index < model.Elements.Length; index++)
			        {
				        var element = model.Elements[index];

				        if (element.Faces != null)
				        {
					        foreach (var face in element.Faces.ToArray())
					        {
						        if (face.Value != null && !string.IsNullOrWhiteSpace(face.Value.Texture))
						        {
							        var result = ResolveTexture(model, face.Value.Texture);

							        if (!string.IsNullOrWhiteSpace(result))
							        {
								        model.Elements[index].Faces[face.Key].Texture = result;
							        }
						        }
					        }
				        }
			        }
		        }
	        }
	        
	        Log.Debug($"Imported {BlockModelRegistry.Count} models from resourcepacks in {modelTimer.ElapsedMilliseconds}ms!");

	        foreach (var resourcePack in active)
	        {
		        Alex.GuiRenderer.LoadLanguages(resourcePack, progress);
	        }
	        
	        Stopwatch sw = Stopwatch.StartNew();
	        MeasureProfiler.StartCollectingData();
	        var imported = BlockFactory.LoadBlockstates(RegistryManager, this, true, false, progress);
	        MeasureProfiler.SaveData();
            
	        MeasureProfiler.StopCollectingData();
            
	        Log.Info($"Imported {imported} blockstates from resourcepack in {sw.ElapsedMilliseconds}ms!");

	        var textures = new Dictionary<ResourceLocation, AtlasGenerator.ImageEntry>();

	        progress?.UpdateProgress(0, $"Loading block textures...");

	        GetTextures(
		        models, textures, progress, ModelType.Block);
	        
	        FindAndAddTexture(new ResourceLocation(ResourceLocation.DefaultNamespace, "block/water_flow"), textures);
	        FindAndAddTexture(new ResourceLocation(ResourceLocation.DefaultNamespace, "block/water_still"), textures);
	        
	        FindAndAddTexture(new ResourceLocation(ResourceLocation.DefaultNamespace, "block/lava_flow"), textures);
	        FindAndAddTexture(new ResourceLocation(ResourceLocation.DefaultNamespace, "block/lava_still"), textures);
	        
	        //if (TryGetBitmap())
	        //textures.Add(, new AtlasGenerator.ImageEntry());
	        
	        BlockAtlas.LoadResources(device, textures, this, progress, true);
	        
	        textures.Clear();

	        progress?.UpdateProgress(0, $"Loading item textures...");

	        GetTextures(
		        models, textures, progress, ModelType.Item);
	        
	        ItemAtlas.LoadResources(device, textures, this, progress, true);

	        ProcessEntityModels(progress);

	        ProcessBedrockResourcePacks(progress);

	        progress?.UpdateProgress(0, $"Loading UI textures...");
	        Alex.GuiRenderer.LoadResourcePackTextures(this, progress);

	        progress?.UpdateProgress(50, "Loading language...");
	        if (!Alex.GuiRenderer.SetLanguage(Options.AlexOptions.MiscelaneousOptions.Language))
	        {
		        string language = CultureInfo.InstalledUICulture.Name;

		        if (!Alex.GuiRenderer.SetLanguage(language))
		        {
			        language = "en_uk";
			        Alex.GuiRenderer.SetLanguage(language);
		        }

		        Options.AlexOptions.MiscelaneousOptions.Language.Value = language;
	        }
	        
	        progress?.UpdateProgress(100, "Loading language...");
	        
	        var f = ActiveResourcePacks.LastOrDefault(x => x.FontBitmap != null);
	        if (f != null)
	        {
		        PreloadCallback?.Invoke(f.FontBitmap, McResourcePack.BitmapFontCharacters.ToList());
	        }
        }

        private void FindAndAddTexture(ResourceLocation search,
	        Dictionary<ResourceLocation, AtlasGenerator.ImageEntry> textures)
        {
	        if (TryGetBitmap(search, out var texture))
	        {
		        TextureMeta meta = null;

		        TryGetTextureMeta(search, out meta);
		        if (textures.ContainsKey(search))
		        {
			        if (meta != null)
				        textures[search].Meta = meta;
						      

			        if (texture != null)
				        textures[search].Image = texture;

			        //textures[texture.Key] = entry;
		        }
		        else
		        {
			        textures.Add(search, new AtlasGenerator.ImageEntry(texture, meta));
		        }
	        }
	        
	       // if (TryGetBitmap(search, out var bmp))
	       // {
		   //     textures.TryAdd(search, new AtlasGenerator.ImageEntry(bmp, ));
	       // }
        }
        
        private void GetTextures(IDictionary<ResourceLocation, ResourcePackModelBase> models,
	        Dictionary<ResourceLocation, AtlasGenerator.ImageEntry> textures,
	        IProgressReceiver progress, ModelType type)
        {
	        int counter = 0;
	        foreach (var itemModel in models.Where(x => x.Value.Type == type))
	        {
		        progress.UpdateProgress(counter, models.Count, null, itemModel.Key.ToString());
		        foreach (var path in itemModel.Value.Textures)
		        {
			        // if (!path.Value.Contains(Selector, StringComparison.InvariantCultureIgnoreCase))
			        //    continue;

			        var p = new ResourceLocation(path.Value);

			        FindAndAddTexture(p, textures);
		        }

		        counter++;
	        }
        }

        private void ProcessBedrockResources(IProgressReceiver progress, GraphicsDevice device, BedrockResourcePack resourcePack)
        {
	        int audioCount = Alex.AudioEngine.Initialize(resourcePack, progress);
	        Log.Debug($"Imported {audioCount} sounds from \"{resourcePack.Info.Name}\"...");
	        
	        int modelCount = EntityFactory.LoadEntityDefinitions(resourcePack, this, device, true, progress);
	        Log.Debug($"Imported {modelCount} entity models from \"{resourcePack.Info.Name}\"...");

	        int particleCount = Alex.ParticleManager.Load(resourcePack, progress);
	        Log.Debug($"Imported {particleCount} particles from \"{resourcePack.Info.Name}\"...");
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
				        SkinPacks.Add(pack);
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

	        //Java Reset
	        {
		        if (ActiveResourcePacks.First != null)
		        {
			        var first = ActiveResourcePacks.First.Value;
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
				        BlockAtlas.Reset();
				        ItemAtlas.Reset();
			        }
		        }
	        }
	        
	        //Bedrock Reset
	        /*{
		        if (ActiveBedrockResourcePacks.First != null)
		        {
			        var first = ActiveBedrockResourcePacks.First.Value;
			        var before = ActiveBedrockResourcePacks.ToArray();

			        foreach (var item in before.Skip(1))
			        {
				        item.Dispose();
			        }

			        ActiveBedrockResourcePacks.Clear();
			        ActiveBedrockResourcePacks.AddFirst(first);
		        }
	        }*/

	        foreach (string file in resourcePacks)
	        {
		        try
		        {
			        string resourcePackPath = Path.Combine(ResourcePackDirectory.FullName, file);

			        if (File.Exists(resourcePackPath))
			        {
				        foreach (var pack in LoadResourcePack(
					        progress,
					        new ZipFileSystem(
						        new FileStream(resourcePackPath, FileMode.Open),
						        Path.GetFileNameWithoutExtension(resourcePackPath)), null))
				        {

					        if (pack.Info.Type == ResourcePackType.Bedrock)
					        {
						        if (pack is BedrockResourcePack bedrockPack)
						        {
							        ActiveBedrockResourcePacks.AddLast(bedrockPack);
						        }
					        }
					        else if (pack.Info.Type == ResourcePackType.Java)
					        {
						        if (pack is McResourcePack javaPack)
						        {
							        if (pack.Info != null && string.IsNullOrWhiteSpace(pack.Info.Name))
							        {
								        pack.Info.Name = Path.GetFileNameWithoutExtension(file);
							        }

							        ActiveResourcePacks.AddLast(javaPack);
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

		/// <inheritdoc />
		public bool TryGetTextureMeta(ResourceLocation location, out TextureMeta meta)
		{
			meta = null;
			foreach (var resourcePack in ActiveResourcePacks.Reverse())
			{
				if (resourcePack.TryGetTextureMeta(location, out var f))
				{
					meta = f;
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
				if (resourcePack.TryGetBitmap(location, out var f))
				{
					bitmap = f;
					return true;
				}
			}

			return false;
		}
		
		//TryGetEntityDefinition
		public bool TryGetEntityDefinition(ResourceLocation location, out EntityDescription entityDef, out BedrockResourcePack source)
		{
			entityDef = null;
			source = null;
			foreach (var resourcePack in ActiveBedrockResourcePacks.Reverse())
			{
				if (resourcePack.EntityDefinitions.TryGetValue(location, out var f))
				{
					entityDef = f;
					source = resourcePack;
					return true;
				}
			}

			return false;
		}

		public bool TryGetEntityModel(string location, out EntityModel entityDef)
		{
			if (EntityModelRegistry.TryGet(location, out var entry))
			{
				entityDef = entry.Value;
				return true;
			}
			entityDef = null;

			return false;
			foreach (var resourcePack in ActiveBedrockResourcePacks.Reverse())
			{
				if (resourcePack.EntityModels.TryGetValue(location, out var f))
				{
					entityDef = f;
					return true;
				}
			}

			/*foreach (var resourcePack in SkinPacks)
			{
				foreach (var module in resourcePack.Modules.Where(x => x is MCSkinPack).Cast<MCSkinPack>())
				{
					if (module.EntityModels.TryGetValue(location, out var f))
					{
						entityDef = f;
						return true;
					}
				}
			}*/
			
			return false;
		}
		
		public bool TryGetBlockState(ResourceLocation location, out BlockStateResource resource)
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

		/*public bool TryGetItemModel(ResourceLocation key, out ResourcePackModelBase model)
		{
			if (ItemModels.TryGetValue(key, out model))
				return true;
			
			return false;
			
			/*model = null;
			foreach (var resourcePack in ActiveResourcePacks.Reverse())
			{
				if (resourcePack.TryGetItemModel(key, out model))
				{
					return true;
				}
			}

			return false;*
		}*/

		/*public bool TryGetBlockModel(ResourceLocation key, out ResourcePackModelBase model)
		{
			if (BlockModels.TryGetValue(key, out model))
				return true;

			var m = BlockModels.FirstOrDefault(x => x.Key.ToString().EndsWith(key.Path, StringComparison.OrdinalIgnoreCase))
			   .Value;

			if (m != null)
			{
				model = m;
				return true;
			}
			
			model = null;
			return false;
			/*model = null;
			foreach (var resourcePack in ActiveResourcePacks.Reverse())
			{
				if (resourcePack.TryGetBlockModel(key, out model))
				{
					return true;
				}
			}

			return false;*
		}*/
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
		
		[JsonProperty("minecraft:sound_event")]
		public RegistryBase Sounds { get; set; }
		
		[JsonProperty("minecraft:particle_type")]
		public RegistryBase Particles { get; set; }
		
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
