using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Alex.Blocks;
using Alex.Blocks.Mapping;
using Alex.Common.Graphics.Typography;
using Alex.Common.Resources;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Entities;
using Alex.Entities.BlockEntities;
using Alex.Gamestates;
using Alex.Graphics;
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
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.ResourcePackLib.Json.Textures;
using Alex.Utils.Assets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ResourceLocation = Alex.Interfaces.Resources.ResourceLocation;

namespace Alex
{
	public class FontsLoadedEventArgs : EventArgs
	{
		public BitmapFontSource[] FontSources { get; }

		public FontsLoadedEventArgs(BitmapFontSource[] fontSources)
		{
			FontSources = fontSources;
		}
	}
	public class ResourceManager : ITextureProvider
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ResourceManager));

		private LinkedList<ResourcePack> ActiveResourcePacks { get; } = new LinkedList<ResourcePack>();
		private IEnumerable<MCJavaResourcePack> ActiveJavaResourcePacks => ActiveResourcePacks.Where(x => x is MCJavaResourcePack).Cast<MCJavaResourcePack>();

		private IEnumerable<MCBedrockResourcePack> ActiveBedrockResourcePacks => ActiveResourcePacks
		   .Where(x => x is MCBedrockResourcePack).Cast<MCBedrockResourcePack>();

		public IEnumerable<MCBedrockResourcePack> ActiveBedrockResources => ActiveBedrockResourcePacks;
		public MCBedrockResourcePack BedrockResourcePack => ActiveBedrockResourcePacks.FirstOrDefault();
		public Registries Registries { get; private set; }
		public AtlasGenerator BlockAtlas { get; private set; }
		public AtlasGenerator ItemAtlas { get; private set; }

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

		public List<MCPack> SkinPacks { get; } = new List<MCPack>();
		public BlockModelRegistry BlockModelRegistry { get; private set; }
		public EntityModelRegistry EntityModelRegistry { get; private set; }


		public static Texture2D NethergamesLogo { get; private set; }

		public EventHandler OnResourcesReloaded;
		public EventHandler<FontsLoadedEventArgs> OnFontsLoaded;

		public ResourceManager(IStorageSystem storage,
			IOptionsProvider optionsProvider,
			IRegistryManager registryManager,
			Alex alex,
			ContentManager contentManager)
		{
			BlockAtlas = new AtlasGenerator("atlases/blocks/");
			ItemAtlas = new AtlasGenerator("atlases/items/");

			Storage = storage;
			Options = optionsProvider;
			RegistryManager = registryManager;
			Alex = alex;
			ContentManager = contentManager;

			AssetsUtil = new MCJavaAssetsUtil(Storage);
			BedrockAssetUtil = new MCBedrockAssetUtils(Storage);
		}

		private void ResourcePacksChanged(string[] oldvalue, string[] newvalue)
		{
			var splashScreen = new SplashScreen();
			Alex.GameStateManager.SetActiveState(splashScreen);

			ThreadPool.QueueUserWorkItem(
				o =>
				{
					try
					{
						ReloadPacks(splashScreen);
					}
					finally
					{
						
						Alex.GameStateManager.RemoveState(splashScreen);
						

						Alex.GameStateManager.Back();
					}
				});
			//GenerateTextureAtlases(Alex.GraphicsDevice, progress);
		}

		public void ReloadPacks(IProgressReceiver progressReceiver)
		{
			
			UnloadResourcePacks();

			if (!ProcessResourcePacks(Alex.GraphicsDevice, progressReceiver, PreloadCallback))
			{
				Log.Warn($"F!");
			}

		}

		public bool TryLoadResourcePackInfo(string file, out ResourcePackManifest[] manifests)
		{
			manifests = default;

			if (!File.Exists(file))
			{
				return false;
			}

			using (FileStream stream = File.OpenRead(file))
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

		private SemaphoreSlim _reloadSemaphore = new SemaphoreSlim(1, 1);
		internal void ReloadBedrockResources(IProgressReceiver progress, bool reset = false)
		{
			_reloadSemaphore.Wait();

			try
			{
				Alex.ParticleManager.Reset();
				EntityFactory.Reset();

				var activeBedrockPacks = ActiveBedrockResourcePacks.ToArray();

				if (reset)
				{
					/*foreach (var active in activeBedrockPacks.Skip(1))
					{
						Remove(active);
					}
					
					activeBedrockPacks = ActiveBedrockResourcePacks.ToArray();*/
				}
				
				ProcessEntityModels(progress, activeBedrockPacks);

				for (int index = 0; index < activeBedrockPacks.Length; index++)
				{
					var resourcePack = activeBedrockPacks[index];

					Log.Debug($"== Start Processing \"{resourcePack.Info.Name}\" ==");

					int audioCount = Alex.AudioEngine.Initialize(resourcePack, progress);
					Log.Debug($"Imported {audioCount} sounds from \"{resourcePack.Info.Name}\"...");

					int modelCount = EntityFactory.LoadEntityDefinitions(resourcePack, true, progress);
					Log.Debug($"Imported {modelCount} entity definitions from \"{resourcePack.Info.Name}\"...");

					int particleCount = Alex.ParticleManager.Load(resourcePack, progress);
					Log.Debug($"Imported {particleCount} particles from \"{resourcePack.Info.Name}\"...");

					Log.Debug($"== End Processing \"{resourcePack.Info.Name}\" ==\n");
				}

				ReloadFonts();
				
				progress?.UpdateProgress(0, $"Loading UI textures...");
				Alex.GuiRenderer.LoadResourcePackTextures(this, progress);
				Alex.GuiManager.Reinitialize();
				Alex.ServiceContainer.GetRequiredService<GuiPanoramaSkyBox>().LoadTextures(Alex.GuiRenderer);
			}
			finally
			{
				_reloadSemaphore.Release();
			}
			//  GenerateTextureAtlases(Alex.GraphicsDevice, progress);
		}

		private void ReloadFonts()
		{
			List<BitmapFontSource> fontSources = new List<BitmapFontSource>();

			foreach (var active in ActiveResourcePacks.Reverse())
			{
				if (active is IFontSourceProvider fontSourceProvider && fontSourceProvider.FontSources != null
				                                                     && fontSourceProvider.FontSources.Length > 0)
				{
					foreach (var source in fontSourceProvider.FontSources)
					{
					//	if (fontSources.Any(x => string.Equals(x.Name, source.Name, StringComparison.InvariantCultureIgnoreCase)))
					//		continue;

						fontSources.Add(source);
					}
				}
			}
			/*var f = ActiveResourcePacks.Reverse()
			   .Where(
					x => x is IFontSourceProvider fontSourceProvider && fontSourceProvider.FontSources != null
					                                                 && fontSourceProvider.FontSources.Length > 0)
			   .Cast<IFontSourceProvider>().SelectMany(x => x.FontSources).ToArray();*/

			//var f2 = ActiveBedrockResourcePacks.Where(x => x is IFontSourceProvider fontSourceProvider && fontSourceProvider.FontSources != null && fontSourceProvider.FontSources.Length > 0).Cast<IFontSourceProvider>().SelectMany(x => x.FontSources).ToArray();
			if (fontSources.Count > 0)
			{
				OnFontsLoaded?.Invoke(this, new FontsLoadedEventArgs(fontSources.ToArray()));
			}
		}

		internal bool Remove(ResourcePack resourcePack)
		{
			if (ActiveResourcePacks.Remove(resourcePack))
			{
				resourcePack?.Dispose();
				return true;
			}
			
			/*if (resourcePack.Info.Type == ResourcePackType.Bedrock)
			{
				if (ActiveBedrockResourcePacks.Remove((MCBedrockResourcePack)resourcePack))
				{
					return true;
				}
			}
			else if (resourcePack.Info.Type == ResourcePackType.Java)
			{
				if (ActiveJavaResourcePacks.Remove((MCJavaResourcePack)resourcePack))
				{
					return true;
				}
			}*/

			return false;
		}

		internal IEnumerable<MCBedrockResourcePack> LoadBedrockTexturePack(IFilesystem fs,
			IProgressReceiver progress = null,
			string contentKey = null)
		{
			foreach (var resourcePack in LoadResourcePack(progress, fs, null, contentKey))
			{
				if (resourcePack.Info.Type == ResourcePackType.Bedrock)
				{
					var pack = (MCBedrockResourcePack)resourcePack;

					ActiveResourcePacks.AddLast(pack);
					//ActiveBedrockResourcePacks.AddLast(pack);

					yield return pack;
				}
			}
		}

		private IEnumerable<ResourcePack> LoadResourcePack(IProgressReceiver progressReceiver,
			IFilesystem fs,
			McResourcePackPreloadCallback preloadCallback = null, string contentKey = null)
		{
			Stopwatch sw = Stopwatch.StartNew();

			foreach (var manifest in ResourcePackLib.ResourcePack.GetManifests(fs))
			{
				if (manifest == null)
					continue;

				if (manifest.Type == ResourcePackType.Java)
				{
					var resourcePack = new MCJavaResourcePack(
						fs, preloadCallback, manifest,
						(percentage, file) => { progressReceiver?.UpdateProgress(percentage, null, file); });

					sw.Stop();

					Log.Info(
						$"Loading java resourcepack \"{(string.IsNullOrWhiteSpace(manifest.Name) ? fs.Name : manifest.Name)}\" took: {sw.ElapsedMilliseconds}ms");

					sw.Restart();

					yield return resourcePack;
				}
				else if (manifest.Type == ResourcePackType.Bedrock)
				{
					var brp = new MCBedrockResourcePack(
						fs, manifest,
						(percentage, file) => { progressReceiver?.UpdateProgress(percentage, null, file); }, contentKey);

					sw.Stop();

					Log.Info($"Loading bedrock resourcepack \"{manifest.Name}\" took: {sw.ElapsedMilliseconds}ms");

					sw.Restart();

					yield return brp;
				}
			}
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

				if (Storage.TryGetDirectory(targetPath, out var targetDirInfo)
				    && targetDirInfo.GetFileSystemInfos().Length == 0)
				{
					Storage.TryDeleteDirectory(targetPath);
				}

				if (!BedrockAssetUtil.CheckUpdate(progressReceiver, targetPath, out bedrockPath))
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
					using (ZipArchive zipArchive = new ZipArchive(Storage.OpenFileStream(bedrockPath, FileMode.Open)))
					{
						var root = "bedrock-samples-main/resource_pack/";
						var resourcePackFiles = from currentEntry in zipArchive.Entries
												where currentEntry.FullName.StartsWith(root)
												where !String.IsNullOrWhiteSpace(currentEntry.Name)
												select currentEntry;

						foreach (ZipArchiveEntry entry in resourcePackFiles)
						{
							var path = Path.Combine(di.FullName, entry.FullName.Substring(root.Length));
							if (!Directory.Exists(path))
							{
								Storage.TryCreateDirectory(Path.GetDirectoryName(path));
							}
							entry.ExtractToFile(path);
						}
					}

					Storage.Delete(bedrockPath);

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
				string assetDirectory = Path.Combine("assets", "java");

				string storedVersion;
				AssetsUtil.TryGetStoredVersion(out storedVersion);

				DirectoryInfo directoryInfo = null;

				if (storedVersion == null || !storedVersion.Equals(JavaProtocol.VersionId)
				                          || !Storage.TryGetDirectory(assetDirectory, out directoryInfo))
				{
					Storage.TryDeleteDirectory(assetDirectory);

					var zipPath = AssetsUtil.EnsureTargetReleaseAsync(
						JavaProtocol.VersionId, progressReceiver, assetDirectory).Result;

					Storage.TryGetDirectory(assetDirectory, out directoryInfo);
					/*if (Storage.TryCreateDirectory(assetDirectory)
					    && Storage.TryGetDirectory(assetDirectory, out directoryInfo))
					{
					    Log.Info($"Extracting resources....");
					    using (ZipArchive zipArchive = new ZipArchive(Storage.OpenFileStream(zipPath, FileMode.Open)))
					    {
					        zipArchive.ExtractToDirectory(directoryInfo.FullName, true);
					    }
					}*/
				}

				if (directoryInfo != null)
				{
					var rootMatches = directoryInfo.GetFiles(".mcassetsroot", SearchOption.AllDirectories);

					if (rootMatches.Length > 0)
					{
						javaResources = rootMatches[0].Directory.FullName;

						return true;
					}

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
		}

		public string DeviceID { get; } = Guid.NewGuid().ToString();

		public DirectoryInfo SkinPackDirectory { get; private set; } = null;
		public DirectoryInfo ResourcePackDirectory { get; private set; } = null;
		private McResourcePackPreloadCallback PreloadCallback { get; set; }

		public bool CheckResources(GraphicsDevice device,
			IProgressReceiver progressReceiver,
			McResourcePackPreloadCallback preloadCallback)
		{
			PreloadCallback = preloadCallback;

			Log.Info($"Loading registries...");
			progressReceiver?.UpdateProgress(0, "Loading registries...");

			Registries =
				JsonConvert.DeserializeObject<Registries>(ReadStringResource("Alex.Resources.registries.json"));

			ParticlePacket.RegistryLookup = particleId =>
				Registries.Particles.Entries.FirstOrDefault(x => x.Value.ProtocolId == particleId).Key;

			progressReceiver?.UpdateProgress(100, "Loading registries...");
			LoadRegistries(progressReceiver);

			if (!ProcessResourcePacks(device, progressReceiver, preloadCallback))
			{
				return false;
			}

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
			NethergamesLogo = TextureUtils.BitmapToTexture2D(this, Alex.GraphicsDevice, Image.Load<Rgba32>(data));

			return true;
		}

		public void UnloadResourcePacks()
		{
			foreach (var active in ActiveResourcePacks.ToArray())
			{
				Remove(active);
				//active?.Dispose();
			}
		}
		
		private bool LoadVanillaResources(GraphicsDevice device, IProgressReceiver progressReceiver, McResourcePackPreloadCallback preloadCallback = null)
		{
			if (!CheckJavaAssets(progressReceiver, out var defaultResources))
			{
				return false;
			}

			if (!CheckBedrockAssets(progressReceiver, out var defaultBedrock))
			{
				return false;
			}
			
			List<ResourcePack> resourcePacks = new List<ResourcePack>();

			progressReceiver?.UpdateProgress(0, "Loading vanilla resources...");

			BlockAtlas.Reset();
			ItemAtlas.Reset();

			foreach (var vanilla in LoadResourcePack(
				         progressReceiver, new DiskFileSystem(defaultResources), preloadCallback))
			{
				resourcePacks.Add(vanilla);
			}

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

					//	ActiveJavaResourcePacks.AddLast((MCJavaResourcePack)pack);
					ActiveResourcePacks.AddLast(pack);
				}
				else if (pack.Info.Type == ResourcePackType.Bedrock)
				{
					if (pack.Info != null && string.IsNullOrWhiteSpace(pack.Info.Name))
						pack.Info.Name = "Vanilla Bedrock";

					//	ActiveBedrockResourcePacks.AddLast((MCBedrockResourcePack)pack);
					ActiveResourcePacks.AddLast(pack);
				}
			}

			return true;
		}

		private bool ProcessResourcePacks(GraphicsDevice device,
			IProgressReceiver progressReceiver,
			McResourcePackPreloadCallback preloadCallback)
		{
			bool wasInit = _hasInit;
			if (!LoadVanillaResources(device, progressReceiver, preloadCallback))
				return false;
			//h

			Storage.TryGetDirectory(Path.Combine("assets", "resourcepacks"), out DirectoryInfo root);
			ResourcePackDirectory = root;

			foreach (string file in Options.AlexOptions.ResourceOptions.LoadedResourcesPacks.Value)
			{
				try
				{
					string resourcePackPath = Path.Combine(ResourcePackDirectory.FullName, file);

					if (!File.Exists(resourcePackPath)) continue;

					foreach (var pack in LoadResourcePack(
						         progressReceiver,
						         new ZipFileSystem(
							         new FileStream(resourcePackPath, FileMode.Open),
							         Path.GetFileNameWithoutExtension(resourcePackPath))))
					{
						if (pack.Info.Type == ResourcePackType.Bedrock)
						{
							if (pack is MCBedrockResourcePack bedrockPack)
							{
								//ActiveBedrockResourcePacks.AddLast(bedrockPack);
								ActiveResourcePacks.AddLast(bedrockPack);
							}
						}
						else if (pack.Info.Type == ResourcePackType.Java)
						{
							if (pack is MCJavaResourcePack javaPack)
							{
								if (pack.Info != null && string.IsNullOrWhiteSpace(pack.Info.Name))
								{
									pack.Info.Name = Path.GetFileNameWithoutExtension(file);
								}

								//ActiveJavaResourcePacks.AddLast(javaPack);
								ActiveResourcePacks.AddLast(javaPack);
							}
						}
					}
				}
				catch (Exception e)
				{
					Log.Warn(e, $"Could not load resourcepack {file}: {e.ToString()}");
				}
			}

			foreach (var resourcePack in ActiveJavaResourcePacks)
				Alex.GuiRenderer.LoadLanguages(resourcePack, progressReceiver);

			progressReceiver?.UpdateProgress(50, "Loading language...");

			if (!Alex.GuiRenderer.SetLanguage(Options.AlexOptions.MiscelaneousOptions.Language))
			{
				string language = CultureInfo.InstalledUICulture.Name;

				if (!Alex.GuiRenderer.SetLanguage(language))
				{
					language = "en_us";
					Alex.GuiRenderer.SetLanguage(language);
				}

				Options.AlexOptions.MiscelaneousOptions.Language.Value = language;
			}

			ReloadBedrockResources(progressReceiver);

			progressReceiver?.UpdateProgress(0, "Processing block models...");
			ProcessBlockModels(progressReceiver, ActiveJavaResourcePacks);

			EntityFactory.Load(this, progressReceiver);

			progressReceiver?.UpdateProgress(0, "Generating texture atlasses...");
			GenerateTextureAtlases(device, progressReceiver);

			Stopwatch sw = Stopwatch.StartNew();
			var imported = BlockFactory.LoadBlockstates(RegistryManager, this, true, false, progressReceiver);

			Log.Info($"Imported {imported} blockstates from resourcepack in {sw.ElapsedMilliseconds}ms!");

			ItemFactory.Init(RegistryManager, this, progressReceiver);

			BlockEntityFactory.LoadResources(device, this);

			ReloadFonts();

			if (wasInit)
			{
				OnResourcesReloaded?.Invoke(this, EventArgs.Empty);
			}

			return true;
		}

		private void ProcessEntityModels(IProgressReceiver progress, MCBedrockResourcePack[] resourcePacks)
		{
			Dictionary<string, EntityModel> entityModels = new Dictionary<string, EntityModel>();

			for (int index = 0; index < resourcePacks.Length; index++)
			{
				var resourcePack = resourcePacks[index];

				foreach (var model in resourcePack.EntityModels)
				{
					entityModels[model.Key] = model.Value;
				}
			}

			entityModels = MCBedrockResourcePack.ProcessEntityModels(entityModels);

			foreach (var model in entityModels)
			{
				var key = new ResourceLocation(model.Key);

				EntityModelRegistry.Set(key, () => new EntityModelEntry(model.Value));
			}
		}

		private void ProcessBlockModels(IProgressReceiver progress, IEnumerable<MCJavaResourcePack> resourcePacks)
		{
			Dictionary<ResourceLocation, ResourcePackModelBase> models =
				new Dictionary<ResourceLocation, ResourcePackModelBase>();

			Stopwatch modelTimer = Stopwatch.StartNew();

			foreach (var resourcePack in resourcePacks)
			{
				foreach (var model in resourcePack.Models)
				{
					var value = model.Value;

					if (models.TryGetValue(model.Key, out var oldModel))
					{
						//     value.UpdateValuesFromParent(oldModel);
					}

					models[model.Key] = value;
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

			foreach (var m in models.Where(x => (x.Value.Type & ModelType.Block) != 0)
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

			Log.Debug(
				$"Imported {BlockModelRegistry.Count} models from resourcepacks in {modelTimer.ElapsedMilliseconds}ms!");
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
				}
				else if (models.TryGetValue(parentKey, out parent))
				{
					parent = ProcessModel(parentKey, parent, ref models);
				}

				if (parent != null)
				{
					model.UpdateValuesFromParent(parent);
				}
				else if (parentKey.Path.Equals("builtin/entity"))
				{
					model.Type = ModelType.Entity;
				}
			}

			var entry = new BlockModelEntry(model);
			BlockModelRegistry.Set(resourceLocation, () => entry);

			return model;
		}

		private void GenerateTextureAtlases(GraphicsDevice device, IProgressReceiver progress)
		{
			Dictionary<ResourceLocation, ResourcePackModelBase> models =
				new Dictionary<ResourceLocation, ResourcePackModelBase>();

			var active = ActiveJavaResourcePacks.ToArray();

			foreach (var resourcePack in active)
			{
				foreach (var model in resourcePack.Models)
				{
					models[model.Key] = model.Value;
				}
			}

			var textures = new Dictionary<ResourceLocation, AtlasGenerator.ImageEntry>();

			progress?.UpdateProgress(0, $"Loading block textures...");

			GetTextures(models, textures, progress, ModelType.Block);

			FindAndAddTexture(new ResourceLocation(ResourceLocation.DefaultNamespace, "block/water_flow"), textures);
			FindAndAddTexture(new ResourceLocation(ResourceLocation.DefaultNamespace, "block/water_still"), textures);

			FindAndAddTexture(new ResourceLocation(ResourceLocation.DefaultNamespace, "block/lava_flow"), textures);
			FindAndAddTexture(new ResourceLocation(ResourceLocation.DefaultNamespace, "block/lava_still"), textures);

			BlockAtlas.LoadResources(device, textures, this, progress, true);

			foreach (var texture in textures)
				texture.Value.Image?.Dispose();


			textures.Clear();

			progress?.UpdateProgress(0, $"Loading item textures...");

			GetTextures(models, textures, progress, ModelType.Item);

			ItemAtlas.LoadResources(device, textures, this, progress, true);

			foreach (var texture in textures)
				texture.Value.Image?.Dispose();
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
					{
						textures[search].Meta = meta;
					}

					if (texture != null)
					{
						textures[search].Image?.Dispose();
						textures[search].Image = texture;
					}
				}
				else
				{
					textures.Add(search, new AtlasGenerator.ImageEntry(texture, meta));
				}
			}
		}

		private void GetTextures(IReadOnlyDictionary<ResourceLocation, ResourcePackModelBase> models,
			Dictionary<ResourceLocation, AtlasGenerator.ImageEntry> textures,
			IProgressReceiver progress,
			ModelType type)
		{
			int counter = 0;

			foreach (var itemModel in models.Where(x => x.Value.Type == type))
			{
				progress.UpdateProgress(counter, models.Count, null, itemModel.Key.ToString());

				foreach (var path in itemModel.Value.Textures)
				{
					var p = new ResourceLocation(path.Value);

					FindAndAddTexture(p, textures);
				}

				counter++;
			}
		}

		private static Regex _bedrockSearchPattern = new Regex(
			@"$(?<=.(zip|mcpack))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public void LoadBedrockPacks(IProgressReceiver progressReceiver, DirectoryInfo directoryInfo)
		{
			progressReceiver?.UpdateProgress(0, "Loading bedrock .MCPack files...");

			var files = directoryInfo.EnumerateFiles().Where(x => _bedrockSearchPattern.IsMatch(x.Name)).ToArray();

			for (var index = 0; index < files.Length; index++)
			{
				var file = files[index];

				progressReceiver?.UpdateProgress(
					index, files.Length, "Loading bedrock resourcepack files...", file.Name);

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
					Log.Warn(ex, $"Failed to load bedrockpack file: {file.Name}: {ex}");
				}
			}
		}

		private bool _hasInit = false;

		private void LoadRegistries(IProgressReceiver progress)
		{
			progress.UpdateProgress(0, "Loading block model registry...");

			RegistryManager.AddRegistry<BlockModelEntry, ResourcePackModelBase>(
				BlockModelRegistry = new BlockModelRegistry());

			progress.UpdateProgress(0, "Loading entity model registry...");
			RegistryManager.AddRegistry<EntityModelEntry, EntityModel>(EntityModelRegistry = new EntityModelRegistry());

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

						progressReceiver?.UpdateProgress(
							(int)ms.Length, (int)reader.Length, null, $"Loading resource: {resource}");
					}
				}

				return ms.ToArray();
			}
		}

		public bool TryGetBitmap(ResourceLocation location, out Image<Rgba32> bitmap)
		{
			bitmap = null;

			foreach (var resourcePack in ActiveJavaResourcePacks.Reverse())
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

			foreach (var resourcePack in ActiveJavaResourcePacks.Reverse())
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
		public bool TryGetEntityDefinition(ResourceLocation location,
			out EntityDescription entityDef,
			out MCBedrockResourcePack source)
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

			return false;/*

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
			}#1#

			return false;*/
		}

		public bool TryGetBlockState(ResourceLocation location, out BlockStateResource resource)
		{
			resource = null;

			foreach (var resourcePack in ActiveJavaResourcePacks.Reverse())
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
			foreach (var resourcePack in ActiveJavaResourcePacks.Reverse())
			{
				if (resourcePack.TryGetGrassColor(temp, rain, elevation, out var color))
				{
					return new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
				}
			}

			return new Microsoft.Xna.Framework.Color(94, 157, 52);
		}

		public Microsoft.Xna.Framework.Color GetFoliageColor(float temp, float rain, int elevation)
		{
			foreach (var resourcePack in ActiveJavaResourcePacks.Reverse())
			{
				if (resourcePack.TryGetFoliageColor(temp, rain, elevation, out var color))
				{
					return new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
				}
			}

			return new Microsoft.Xna.Framework.Color(94, 157, 52);
		}
	}

	public class Registries
	{
		[JsonProperty("minecraft:fluid")] public RegistryBase Fluids { get; set; }

		[JsonProperty("minecraft:block")] public RegistryBase Blocks { get; set; }

		[JsonProperty("minecraft:item")] public RegistryBase Items { get; set; }

		[JsonProperty("minecraft:entity_type")]
		public RegistryBase Entities { get; set; }

		[JsonProperty("minecraft:sound_event")]
		public RegistryBase Sounds { get; set; }

		[JsonProperty("minecraft:particle_type")]
		public RegistryBase Particles { get; set; }

		[JsonProperty("minecraft:block_entity_type")]
		public RegistryBase BlockEntities { get; set; }

		public class RegistryBase
		{
			[JsonProperty("default")] public string Default { get; set; } = null;

			[JsonProperty("protocol_id")] public int ProtocolId { get; set; }

			[JsonProperty("entries")] public IReadOnlyDictionary<string, RegistryEntry> Entries { get; set; }
		}

		public class RegistryEntry
		{
			[JsonProperty("protocol_id")] public int ProtocolId { get; set; }
		}
	}
}