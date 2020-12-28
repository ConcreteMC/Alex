using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Alex.API.Resources;
using Alex.API.Utils;
using Alex.ResourcePackLib.IO.Abstract;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Bedrock.Sound;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Models.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace Alex.ResourcePackLib
{
	public class BedrockResourcePack : IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockResourcePack));

		private ConcurrentDictionary<string, Image<Rgba32>> _bitmaps = new ConcurrentDictionary<string, Image<Rgba32>>();
        public IReadOnlyDictionary<string, Image<Rgba32>> Textures => _bitmaps;
		public IReadOnlyDictionary<ResourceLocation, EntityDescription> EntityDefinitions { get; private set; } = new ConcurrentDictionary<ResourceLocation, EntityDescription>();
		public SoundDefinitionFormat SoundDefinitions { get; private set; } = null;
		
		private readonly IFilesystem _archive;

		public BedrockResourcePack(IFilesystem archive, ResourcePack.LoadProgress progressReporter = null)
		{
			_archive = archive;

			Load(progressReporter);
		}

		public Stream GetStream(string path)
		{
			var entry = _archive.GetEntry(path);

			if (entry == null)
			{
				throw new FileNotFoundException();
			}
			
			return entry.Open();
		}
		
		public bool TryGetTexture(string name, out Image<Rgba32> texture)
		{
			return Textures.TryGetValue(NormalisePath(name), out texture);
		}

		private string NormalisePath(string path)
		{
			return path.Replace('\\', '/').ToLowerInvariant();
		}
		
		private const RegexOptions RegexOpts = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
		private static readonly Regex IsEntityDefinition     = new Regex(@"^entity[\\\/](?'filename'.*)\.json$", RegexOpts);
		private static readonly Regex IsEntityModel    = new Regex(@"^models[\\\/]entity[\\\/](?'filename'.*)\.json$", RegexOpts);
		private static readonly Regex IsSoundDefinition    = new Regex(@"^sounds[\\\/]sound_definitions\.json$", RegexOpts);
		private void Load(ResourcePack.LoadProgress progressReporter)
		{
			Dictionary<ResourceLocation, EntityDescription> entityDefinitions = new Dictionary<ResourceLocation, EntityDescription>();
			Dictionary<string, EntityModel> entityModels = new Dictionary<string, EntityModel>();

			if (TryLoadMobModels(entityModels))
			{
				//Log.Info($"Loaded mobs.json: {entityModels.Count}");
			}

			var entries = _archive.Entries.ToArray();
			var total   = entries.Length;
			int count   = 0;
			
			foreach (var entry in entries)
			{
				count++;
				progressReporter?.Invoke((int)(((double)count / (double)total) * 100D), entry.FullName);
				
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
			}
			EntityModels = ProcessEntityModels(entityModels);

			Log.Info($"Processed {EntityModels.Count} entity models");
		
			EntityDefinitions = entityDefinitions;
            Log.Info($"Processed {EntityDefinitions.Count} entity definitions");
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
		
		private static void LoadEntityModel(IFile entry, Dictionary<string, EntityModel> models)
		{
			string json;
			using (var stream = entry.Open())
			{
				json = Encoding.UTF8.GetString(stream.ReadToEnd());
			}

			LoadEntityModel(json, models);
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
			using (var fs = entry.Open())
			{
				var fileContents = fs.ReadToEnd();
				var json         = Encoding.UTF8.GetString(fileContents);

				SoundDefinitions = SoundDefinitionFormat.FromJson(json);
			}
		}

		private enum DefFormat
		{
			unknown,
			v18,
			v110
		}
		private void LoadEntityDefinition(IFile entry, Dictionary<ResourceLocation, EntityDescription> entityDefinitions)
		{
			using (var stream = entry.Open())
			{
				var json = Encoding.UTF8.GetString(stream.ReadToSpan(entry.Length));

				//string fileName = Path.GetFileNameWithoutExtension(entry.Name);

				Dictionary<ResourceLocation, EntityDescription> definitions = new Dictionary<ResourceLocation, EntityDescription>();
				
				JObject obj  = JObject.Parse(json, new JsonLoadSettings());

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
						var               clientEntity = (JObject) e.Value;
						
						if (clientEntity.TryGetValue("description", out var descriptionToken))
						{
							if (descriptionToken.Type == JTokenType.Object)
							{
								desc = descriptionToken.ToObject<EntityDescription>();
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

									var bmp = TryLoad(texture.Value + ".png");

									if (bmp == null)
										bmp = TryLoad(texture.Value + ".tga");

									if (bmp != null)
										_bitmaps.TryAdd(texture.Value, bmp);
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
			}
		}
		
		private Image<Rgba32> TryLoad(string file)
		{
			var entry = _archive.GetEntry(file);
			if (entry != null)
			{
				Image<Rgba32> bmp = null;
				using (var fs = entry.Open())
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

		public class EntityDefinition
		{
			[JsonIgnore] public string Filename { get; set; } = string.Empty;

			public Dictionary<string, string> Textures;
			public Dictionary<string, string> Geometry;
		}

		public void Dispose()
		{
			_archive?.Dispose();
		}
	}
}