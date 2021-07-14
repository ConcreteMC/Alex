using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Alex.Common.Resources;
using Alex.Common.Utils;
using Alex.MoLang.Parser.Exceptions;
using Alex.ResourcePackLib.Abstraction;
using Alex.ResourcePackLib.Generic;
using Alex.ResourcePackLib.IO.Abstract;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Bedrock;
using Alex.ResourcePackLib.Json.Bedrock.Attachables;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Bedrock.Particles;
using Alex.ResourcePackLib.Json.Bedrock.Sound;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Converters.Bedrock;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.ResourcePackLib.Json.Textures;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using Animation = Alex.ResourcePackLib.Json.Bedrock.Entity.Animation;
using Image = SixLabors.ImageSharp.Image;

namespace Alex.ResourcePackLib
{
	public class BedrockResourcePack : ResourcePack, ITextureProvider, IAnimationProvider, IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockResourcePack));

		private ConcurrentDictionary<ResourceLocation,Func<Image<Rgba32>>> _bitmaps = new ConcurrentDictionary<ResourceLocation, Func<Image<Rgba32>>>();
        public IReadOnlyDictionary<ResourceLocation, Func<Image<Rgba32>>> Textures => _bitmaps;
		public IReadOnlyDictionary<ResourceLocation, EntityDescription> EntityDefinitions { get; private set; } = new ConcurrentDictionary<ResourceLocation, EntityDescription>();
		public IReadOnlyDictionary<string, AttachableDefinition> Attachables { get; private set; } = new ConcurrentDictionary<string, AttachableDefinition>();
		public IReadOnlyDictionary<string, RenderController> RenderControllers { get; private set; } = new ConcurrentDictionary<string, RenderController>();
		public IReadOnlyDictionary<string, AnimationController> AnimationControllers { get; private set; } = new ConcurrentDictionary<string, AnimationController>();
		public IReadOnlyDictionary<string, Animation> Animations { get; private set; } = new ConcurrentDictionary<string, Animation>();

		public IReadOnlyDictionary<string, ParticleDefinition> Particles { get; private set; } =
			new ConcurrentDictionary<string, ParticleDefinition>();

		public IReadOnlyDictionary<string, JToken> GlobalUiVariables { get; private set; } =
			new Dictionary<string, JToken>();
		
		public SoundDefinitionFormat SoundDefinitions { get; private set; } = null;

		public SoundBindingsCollection SoundBindings { get; private set; } = null;
		
		private readonly IFilesystem _archive;

		public string ContentKey { get; set; } = null;
		public BedrockResourcePack(IFilesystem archive, ResourcePackManifest manifest, ResourcePack.LoadProgress progressReporter = null)
		{
			Info = manifest;
			_archive = archive;

			//Info = GetManifest(archive);
			Load(progressReporter);
		}

		private string NormalisePath(string path)
		{
			return path.Replace('\\', '/').ToLowerInvariant();
		}
		
		private const RegexOptions RegexOpts = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
		private static readonly Regex IsEntityDefinition     = new Regex(@"^entity[\\\/](?'filename'.*)\.json$", RegexOpts);
		private static readonly Regex IsEntityModel    = new Regex(@"^models[\\\/]entity[\\\/](?'filename'.*)\.json$", RegexOpts);
		private static readonly Regex IsRenderController     = new Regex(@"^render_controllers[\\\/](?'filename'.*)\.json$", RegexOpts);
		private static readonly Regex IsAnimationController     = new Regex(@"^animation_controllers[\\\/](?'filename'.*)\.json$", RegexOpts);
		private static readonly Regex IsAnimation = new Regex(@"^animations[\\\/](?'filename'.*)\.json$", RegexOpts);
		private static readonly Regex IsSoundDefinition    = new Regex(@"^sounds[\\\/]sound_definitions\.json$", RegexOpts);
		private static readonly Regex IsSoundBindings    = new Regex(@"^sounds\.json$", RegexOpts);
		private static readonly Regex IsFontFile    = new Regex(@"^font[\\\/](?'filename'.*)\.png$", RegexOpts);
		private static readonly Regex IsParticleFile    = new Regex(@"^particles[\\\/](?'filename'.*)\.json$", RegexOpts);
		private static readonly Regex IsAttachableFile    = new Regex(@"^attachables[\\\/](?'filename'.*)\.json$", RegexOpts);
		
		private static readonly Regex IsUiTexture    = new Regex(@"^textures[\\\/]ui[\\\/](?'filename'.*)\.png", RegexOpts);
		private static readonly Regex IsUiDefinition    = new Regex(@"^ui[\\\/](?'filename'.*)\.json", RegexOpts);
		private void Load(ResourcePack.LoadProgress progressReporter)
		{
			Dictionary<ResourceLocation, EntityDescription> entityDefinitions = new Dictionary<ResourceLocation, EntityDescription>();
			Dictionary<string, EntityModel> entityModels = new Dictionary<string, EntityModel>(StringComparer.Ordinal);
			Dictionary<string, RenderController> renderControllers = new Dictionary<string, RenderController>(StringComparer.Ordinal);

			Dictionary<string, AnimationController>
				animationControllers = new Dictionary<string, AnimationController>(StringComparer.Ordinal);

			Dictionary<string, ParticleDefinition> particleDefinitions = new Dictionary<string, ParticleDefinition>(StringComparer.Ordinal);

			Dictionary<string, AttachableDefinition> attachableDefinitions =
				new Dictionary<string, AttachableDefinition>(StringComparer.Ordinal);
			Dictionary<string, Animation>
				animations = new Dictionary<string, Animation>(StringComparer.Ordinal);
			
			TryAddBitmap("textures/ui/title");
			
			if (TryLoadMobModels(entityModels))
			{
				//Log.Info($"Loaded mobs.json: {entityModels.Count}");
			}

			var entries = _archive.Entries.ToArray();
			var total   = entries.Length;
			int count   = 0;
			
			foreach (var entry in entries)
			{
				try
				{
					count++;
					progressReporter?.Invoke((int) (((double) count / (double) total) * 100D), entry.FullName);

					if (entry.Name.Equals("_global_variables.json"))
					{
						try
						{
							using (var stream = entry.OpenEncoded(ContentKey))
							{
								var json = Encoding.UTF8.GetString(stream.ReadToSpan(entry.Length));

								Dictionary<string, JToken> globalVariables =
									MCJsonConvert.DeserializeObject<Dictionary<string, JToken>>(json);

								GlobalUiVariables = new ReadOnlyDictionary<string, JToken>(globalVariables);
							}
						}
						catch (Exception ex)
						{
							Log.Warn(ex, $"Could not load global variables.");
						}

						continue;
					}

					if (IsSoundBindings.IsMatch(entry.FullName))
					{
						ProcessSoundBindings(entry);
						continue;
					}
					
					if (IsUiTexture.IsMatch(entry.FullName))
					{
						ProcessTexture(entry);
						continue;
					}
					
					if (IsEntityDefinition.IsMatch(entry.FullName))
					{
						LoadEntityDefinition(entry, entityDefinitions);

						continue;
					}

					if (IsEntityModel.IsMatch(entry.FullName))
					{
						LoadEntityModel(entry, entityModels);

						continue;
					}

					if (IsSoundDefinition.IsMatch(entry.FullName))
					{
						ProcessSounds(progressReporter, entry);

						continue;
					}

					if (IsFontFile.IsMatch(entry.FullName))
					{
						ProcessFontFile(progressReporter, entry);

						continue;
					}

					if (IsRenderController.IsMatch(entry.FullName))
					{
						ProcessRenderController(entry, renderControllers);

						continue;
					}

					if (IsAnimationController.IsMatch(entry.FullName))
					{
						ProcessAnimationController(entry, animationControllers);

						continue;
					}

					if (IsAnimation.IsMatch(entry.FullName))
					{
						ProcessAnimation(entry, animations);

						continue;
					}

					if (IsParticleFile.IsMatch(entry.FullName))
					{
						ProcessParticle(entry, particleDefinitions);

						continue;
					}

					if (IsAttachableFile.IsMatch(entry.FullName))
					{
						ProcessAttachable(entry, attachableDefinitions);
						continue;
					}
				}
				catch (Exception ex)
				{
					Log.Warn(ex, $"Could not process file in resource pack: '{entry.FullName}' continuing anyways...");
				}
			}

			EntityModels = entityModels;

			//Log.Info($"Processed {EntityModels.Count} entity models");
		
			EntityDefinitions = entityDefinitions;
			RenderControllers = renderControllers;
			AnimationControllers = animationControllers;
			Animations = animations;
			Particles = particleDefinitions;
			Attachables = attachableDefinitions;
			// Log.Info($"Processed {EntityDefinitions.Count} entity definitions");
		}

		private void ProcessSoundBindings(IFile entry)
		{
			try
			{
				string json = entry.ReadAsEncodedString(ContentKey);
				SoundBindings = MCJsonConvert.DeserializeObject<SoundBindingsCollection>(json);
			}
			catch (Exception ex)
			{
				Log.Warn(ex, $"Failed to load soundbindings from file \"{entry.FullName}\"");
			}
		}

		private void ProcessTexture(IFile entry)
		{
			
		}

		private void ProcessAttachable(IFile entry, Dictionary<string, AttachableDefinition> attachableDefinitions)
		{
			try
			{
				string json = entry.ReadAsEncodedString(ContentKey);

				var versionedResource = MCJsonConvert.DeserializeObject<VersionedResource<AttachableDefinition>>(
					json, new VersionedResourceConverter<AttachableDefinition>("minecraft:attachable", true, (def) => def.Description.Identifier));

				foreach (var controller in versionedResource.Values)
				{
					if (attachableDefinitions.TryAdd(controller.Key, controller.Value))
					{
						if (controller.Value?.Description?.Textures != null)
						{
							foreach (var kv in controller.Value.Description.Textures)
							{
								var texture = kv.Value;

								if (texture != null && !_bitmaps.ContainsKey(texture))
								{
									TryAddBitmap(texture);
								}
							}
						}
					};
				}
			}
			catch (MoLangParserException ex)
			{
				Log.Warn(ex, $"Failed to load attachable from file \"{entry.FullName}\"");
			}
		}

		private void ProcessParticle(IFile entry, Dictionary<string, ParticleDefinition> particleDefinitions)
		{
			try
			{
				string json = entry.ReadAsEncodedString(ContentKey);

				var versionedResource = MCJsonConvert.DeserializeObject<VersionedResource<ParticleDefinition>>(
					json, new VersionedResourceConverter<ParticleDefinition>("particle_effect", true, (def) => def.Description.Identifier));

				foreach (var controller in versionedResource.Values)
				{
					if (particleDefinitions.TryAdd(controller.Key, controller.Value))
					{
						string texture = controller.Value?.Description?.BasicRenderParameters?.Texture;

						if (texture != null && !_bitmaps.ContainsKey(controller.Value.Description.BasicRenderParameters.Texture))
						{
							TryAddBitmap(texture);
						}
					};
				}
			}
			catch (MoLangParserException ex)
			{
				Log.Warn(ex, $"Failed to load particle from file \"{entry.FullName}\"");
			}
		}
		
		private void ProcessAnimation(IFile entry, Dictionary<string, Animation> animations)
		{
			try
			{
				string json = entry.ReadAsEncodedString(ContentKey);

				var versionedResource = MCJsonConvert.DeserializeObject<VersionedResource<Animation>>(
					json, new VersionedResourceConverter<Animation>("animations"));

				foreach (var controller in versionedResource.Values)
				{
					if (controller.Value.AnimationTimeUpdate == null)
						controller.Value.AnimationTimeUpdate = Animation.DefaultTimeUpdate;
					
					animations.TryAdd(controller.Key, controller.Value);
				}
			}
			catch (MoLangParserException ex)
			{
				Log.Warn(ex, $"Failed to load animation from file \"{entry.FullName}\"");
			}
		}
		
		private void ProcessAnimationController(IFile entry, Dictionary<string, AnimationController> animationControllers)
		{
			string json = entry.ReadAsEncodedString(ContentKey);

			var versionedResource = MCJsonConvert.DeserializeObject<VersionedResource<AnimationController>>(
				json, new VersionedResourceConverter<AnimationController>("animation_controllers"));

			foreach (var controller in versionedResource.Values)
			{
				animationControllers.TryAdd(controller.Key, controller.Value);
			}
		}
		
		private void ProcessRenderController(IFile entry, Dictionary<string, RenderController> renderControllers)
		{
			string json = entry.ReadAsEncodedString(ContentKey);

			var versionedResource = MCJsonConvert.DeserializeObject<VersionedResource<RenderController>>(
				json, new VersionedResourceConverter<RenderController>("render_controllers"));

			foreach (var controller in versionedResource.Values)
			{
				renderControllers.TryAdd(controller.Key, controller.Value);
			}
		}

		public IReadOnlyDictionary<string, EntityModel> EntityModels { get; private set; }
		public static Dictionary<string, EntityModel> ProcessEntityModels(Dictionary<string, EntityModel> models, Func<string, EntityModel> lookup = null)
		{
			Dictionary<string, EntityModel> final = new Dictionary<string, EntityModel>();
			Queue<KeyValuePair<string, EntityModel>> workQueue = new Queue<KeyValuePair<string, EntityModel>>();

			foreach (var model in models.OrderBy(x => x.Key.Count(k => k == ':')))
			{
				workQueue.Enqueue(model);
			}

			//var item = workQueue.First;

			//while (item.Next != null)
			while(workQueue.TryDequeue(out var item))
			{
				try
				{
					var model = item.Value;

					EntityModel result = null;

					if (!item.Key.Contains(":"))
					{
						final.TryAdd(item.Key, model);
						continue;
					}

					var    split         = item.Key.Split(':').Reverse().ToArray();
						//string sb            = "";
					bool   wasInterupted = false;

					for (int i = 0; i < split.Length; i++)
					{
						//	if (i == 0)
						//		break;
						EntityModel requiredEntityModel;
						if (i == split.Length - 1) //Last item
						{
							requiredEntityModel = model;
						}
						else
						{
							var key = split[i];
							
							if (!final.TryGetValue(key, out requiredEntityModel))
							{
								if (lookup != null)
								{
									requiredEntityModel = lookup.Invoke(key);

									if (requiredEntityModel == null)
									{
										wasInterupted = true;
										workQueue.Enqueue(item);

										break;
									}
								}
								else
								{
									//Log.Warn($"Could not find entity model: {key} of {item.Key}");
									wasInterupted = true;
									workQueue.Enqueue(item);

									break;
								}
							}
						}

						if (i == 0)
						{
							result = requiredEntityModel.Clone();
						}
						else
						{
							result += requiredEntityModel;
						}

						/*if (i > 0)
						{
							result += requiredEntityModel;
						}
						else
						{
							result = requiredEntityModel;
						}*/

						//final.TryAdd(split[i], result);
					}

					//models.TryAdd(sb, result);
					if (!wasInterupted && result != null)
					{
						result.Description = model.Description.Clone();

						if (!final.TryAdd(split[^1], result))
						{
							Log.Warn($"Failed to add {split[0]}");
						}

						//final[split[0]] = result;
					}
				}
				finally
				{
					//item = item.Next;
					//item.Previous = null;
				//	workQueue.Remove(item.Previous);
				}
			}

			return final;
		}

		private void LoadEntityModel(IFile entry, Dictionary<string, EntityModel> models)
		{
			try
			{
				string json = entry.ReadAsEncodedString(ContentKey);

				LoadEntityModel(json, models);
			}
			catch (Exception ex)
			{
				Log.Warn(ex, $"Failed to load entity model from file: {entry.FullName}");
			}
		}

		public static void LoadEntityModel(string json, Dictionary<string, EntityModel> models)
		{
			
				var d = MCJsonConvert.DeserializeObject<MobsModelDefinition>(json);

				//if (decoded == null)
				if (d != null)
				{
					foreach (var item in d)
					{
						if (item.Value.Description?.Identifier == null)
						{
							Log.Warn($"Missing identifier for {item.Key}");

							//return;
						}

						if (!models.TryAdd(item.Key, item.Value))
						{
							Log.Warn($"Duplicate geometry model: {item.Key}");

							//	return;
						}
					}
				}
			}
		
		private bool TryLoadMobModels(Dictionary<string, EntityModel> models)
		{
			var mobsJson = _archive.GetEntry("models/mobs.json");

			if (mobsJson == null)
				return false;

			LoadEntityModel(mobsJson, models);

			return true;
		}

		private void ProcessSounds(ResourcePack.LoadProgress progress, IFile entry)
		{
			string json = entry.ReadAsEncodedString(ContentKey);

			SoundDefinitions = SoundDefinitionFormat.FromJson(json);
		}

		private enum DefFormat
		{
			unknown,
			v18,
			v110
		}

		private void LoadEntityDefinition(IFile entry,
			Dictionary<ResourceLocation, EntityDescription> entityDefinitions)
		{

			string json = entry.ReadAsEncodedString(ContentKey);

			//string fileName = Path.GetFileNameWithoutExtension(entry.Name);

			Dictionary<ResourceLocation, EntityDescription> definitions =
				new Dictionary<ResourceLocation, EntityDescription>();

			JObject obj = JObject.Parse(json, new JsonLoadSettings());

			DefFormat format = DefFormat.unknown;

			if (obj.TryGetValue("format_version", out var ftv))
			{
				if (ftv.Type == JTokenType.String)
				{
					switch (ftv.Value<string>())
					{
						case "1.10.0":
							format = DefFormat.v110;

							break;

						case "1.8.0":
							format = DefFormat.v18;

							break;
					}
				}
			}

			foreach (var e in obj)
			{
				if (e.Key == "format_version") continue;

				if (e.Key == "minecraft:client_entity" && e.Value != null)
				{
					EntityDescription desc = null;
					var clientEntity = (JObject) e.Value;

					if (clientEntity.TryGetValue("description", out var descriptionToken))
					{
						if (descriptionToken.Type == JTokenType.Object)
						{
							desc = descriptionToken.ToObject<EntityDescription>(MCJsonConvert.Serializer);
						}
					}

					/*desc = e.Value.ToObject<EntityDescriptionWrapper>(JsonSerializer.Create(new JsonSerializerSettings()
					{
						Converters = MCJsonConvert.DefaultSettings.Converters,
						MissingMemberHandling = MissingMemberHandling.Ignore,
						NullValueHandling = NullValueHandling.Ignore,
					}));*/

					if (desc != null)
					{
						if (!definitions.TryAdd(desc.Identifier, desc))
						{
							Log.Warn($"Duplicate definition: {desc.Identifier}");
						}
					}
				}
			}

			foreach (var def in definitions)
			{
				//def.Value.Filename = fileName;
				if (def.Value != null && def.Value.Textures != null)
				{
					//if (entityDefinitions.TryAdd(def.Key, def.Value))
					{
						//entityDefinitions.Add(def.Key, def.Value);
						try
						{
							foreach (var texture in def.Value.Textures)
							{
								if (_bitmaps.ContainsKey(texture.Value))
								{
									//Log.Warn($"Duplicate bitmap: {texture.Value}");
									continue;
								}

								TryAddBitmap(texture.Value);
							}
						}
						catch (Exception ex)
						{
							Log.Warn($"Could not load texture! {ex}");
						}

						entityDefinitions[def.Key] = def.Value;
					}

					//	else
					{
						//	Log.Warn($"Tried loading duplicate entity: {def.Key}");
					}
				}
			}

			TryAddBitmap("textures/entity/chest/double_normal");
		}

		private void ProcessFontFile(ResourcePack.LoadProgress progress, IFile entry)
		{
			//TODO: Process server sent custom font glyph files (glyph_E1 starts at E100 in unicode, so \uE100)
			return;
			var image = TryLoad(entry.FullName);

			if (image != null)
			{
				
			}
		}

		private bool TryAddBitmap(string path)
		{
			if (_bitmaps.ContainsKey(path))
				return false;
			
			if (_archive.GetEntry(path + ".tga") != null)
			{
				return _bitmaps.TryAdd(path, new Func<Image<Rgba32>>(
					() =>
					{
						return TryLoad(path + ".tga");
					}));
			}
			else if (_archive.GetEntry(path + ".png") != null)
			{
				return _bitmaps.TryAdd(path, new Func<Image<Rgba32>>(
					() =>
					{
						return TryLoad(path + ".png");
					}));
			}

			return false;
		}
		
		private Image<Rgba32> TryLoad(string file)
		{
			var entry = _archive.GetEntry(file);
			if (entry != null)
			{
				Image<Rgba32> bmp = null;
				using (var fs = entry.OpenEncoded(ContentKey))
				{
					if (file.EndsWith(".tga"))
					{
						bmp = Image.Load(fs, new TgaDecoder()).CloneAs<Rgba32>();
					}
					else
					{
						bmp = Image.Load<Rgba32>(fs);
					}

					//	bmp = new Image(fs);
				}

				return bmp;
			}

			return null;
		}

		public bool TryGetBitmap(ResourceLocation textureName, out Image<Rgba32> bitmap)
		{
			if (Textures.TryGetValue(new ResourceLocation(textureName.Namespace, NormalisePath(textureName.Path)), out var t))
			{
				bitmap = t();

				return bitmap != null;
			}

			bitmap = null;
			return false;
		}

		/// <inheritdoc />
		public bool TryGetTextureMeta(ResourceLocation textureName, out TextureMeta meta)
		{
			meta = null;
			return false;
		}

		public void Dispose()
		{
			_archive?.Dispose();
		}

		/// <inheritdoc />
		public bool TryGetAnimationController(string key, out AnimationController animationController)
		{
			return AnimationControllers.TryGetValue(key, out animationController);
		}

		/// <inheritdoc />
		public bool TryGetAnimation(string key, out Animation animation)
		{
			return Animations.TryGetValue(key, out animation);
		}
	}
}