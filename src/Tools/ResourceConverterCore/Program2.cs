using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Alex.API.Graphics.Textures;
using Alex.Graphics.Models.Entity;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.ResourcePackLib.Json.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog.Fluent;
using static Alex.Graphics.Models.Entity.EntityModelRenderer;
using ModelBone = Alex.Graphics.Models.Entity.EntityModelRenderer.ModelBone;
using Path = System.IO.Path;

namespace ResourceConverterCore
{
	static class Log
	{
		public static void Error(string message)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"[ERROR] {message}");
			Reset();
		}

        public static void Warn(string message)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine($"[WARN] {message}");
			Reset();
		}

        public static void Info(string message)
        {
	        Console.ForegroundColor = ConsoleColor.Gray;
	        Console.WriteLine($"[INFO] {message}");
	        Reset();
        }

        private static void Reset()
		{
			Console.ForegroundColor = ConsoleColor.White;
        }
	}

	class Program2
	{
		static void Main(string[] args)
		{
            if (args.Length != 1)
			{
				Log.Warn("No path was specified!");
				return;
			}

			var arg = args[0];
			DirectoryInfo dirInfo = new DirectoryInfo(arg);
			if (!dirInfo.Exists)
			{
				Log.Warn("Specified directory just not exist!");
				return;
			}

			RealConverter c = new RealConverter(dirInfo);

			Stopwatch sw = Stopwatch.StartNew();

			c.Run();

			foreach (var model in c.EntityModels)
			{
				//model.Value.
			}

			sw.Stop();

			Log.Info($"Time Elapsed: {sw.Elapsed}");
			Log.Info("Press ENTER to exit.");
			Console.ReadLine();
		}
	}

	internal class RealConverter
	{
		private Dictionary<string, EntityModel> _processedModels = new Dictionary<string, EntityModel>();
		public IReadOnlyDictionary<string, EntityModel> EntityModels => _processedModels;
        public IReadOnlyDictionary<string, Bitmap> Textures { get; private set; } = new ConcurrentDictionary<string, Bitmap>();
        public IReadOnlyDictionary<string, TextureInfoJson> TextureJsons { get; private set; } = new ConcurrentDictionary<string, TextureInfoJson>();

		public IReadOnlyDictionary<string, EntityDefinition> EntityDefinitions { get; private set; } = new ConcurrentDictionary<string, EntityDefinition>();

		private readonly DirectoryInfo WorkingDir;

		public RealConverter(DirectoryInfo workingDirectorg)
		{
			WorkingDir = workingDirectorg;
		}

		private bool EntitysLoaded { get; set; } = false;

		public bool TryGetTexture(string name, out Bitmap texture)
		{
			return Textures.TryGetValue(NormalisePath(name), out texture);
		}

		public bool TryGetTextureJson(string name, out TextureInfoJson textureJson)
		{
			return TextureJsons.TryGetValue(NormalisePath(name), out textureJson);
		}

		private string NormalisePath(string path)
		{
			return path.Replace('\\', '/').ToLowerInvariant();
		}

        public void Run()
		{
			DirectoryInfo entityDirectory = null;
            DirectoryInfo entityDirectory2 = null;
			DirectoryInfo entityDefinitionsDir = null;
			FileInfo mobsFile = null;
			
			Dictionary<string, FileInfo> entityGeometry = new Dictionary<string, FileInfo>();
			foreach (var dir in WorkingDir.EnumerateDirectories())
			{
				if (entityDirectory2 == null && dir.Name.Equals("entity"))
				{
					entityDirectory2 = dir;
					foreach (var file in dir.EnumerateFiles())
					{
						if (!entityGeometry.TryAdd(file.Name, file))
						{
							Log.Warn($"Failed to add to entity geo dictionary (0)! {file.Name}");
						}
                    }
					continue;
				}

                if (entityDefinitionsDir == null && dir.Name.Equals("definitions"))
				{
					foreach (var d in dir.EnumerateDirectories())
					{
						if (d.Name.Equals("entity"))
						{
							entityDefinitionsDir = d;

							foreach (var file in d.EnumerateFiles())
							{
								if (!entityGeometry.TryAdd(file.Name, file))
								{
									Log.Warn($"Failed to add to entity geo dictionary (1)! {file.Name}");
								}
                            }

                            break;
						}
					}
				}
				
				if (dir.Name.Equals("models"))
				{
					if (entityDirectory == null)
					{
						foreach (var d in dir.EnumerateDirectories())
						{
							if (d.Name.Equals("entity"))
							{
								entityDirectory = dir;

								foreach (var file in d.EnumerateFiles())
								{
									if (!entityGeometry.TryAdd(file.Name, file))
									{
										Log.Warn($"Failed to add to entity geo dictionary (2)! {file.Name}");
									}
								}

                                break;
							}
						}
					}

					if (mobsFile == null)
					{
						foreach (var file in dir.EnumerateFiles())
						{
                            if (file.Name.Equals("mobs.json"))
							{
								mobsFile = file;
								break;
							}
						}
					}
				}
				
				if (entityDirectory != null && mobsFile != null && entityDefinitionsDir != null && entityDirectory2 != null)
					break;
			}

			if (entityDirectory == null || !entityDirectory.Exists)
			{
				Log.Warn("Could not find entity folder!");
				return;
			}

			if (entityDefinitionsDir == null || !entityDefinitionsDir.Exists)
			{
				Log.Warn("Could not find entity definitions folder!");
				return;
			}

			if (mobsFile == null || !mobsFile.Exists)
			{
				Log.Warn("Could not find mob entity definitions! ('mobs.json')");
				return;
			}

			Dictionary<string, EntityDefinition> entityDefinitions = new Dictionary<string, EntityDefinition>();
            foreach (var def in entityDefinitionsDir.EnumerateFiles())
			{
				LoadEntityDefinition(def, entityDefinitions);
			}

			EntityDefinitions = entityDefinitions;

            var res = new Dictionary<string, EntityModel>();
			GetEntries(mobsFile, res);
			
            int missed1 = LoadMobs(res);

			res.Clear();

            foreach (var file in entityGeometry.Values)
            {
	            GetEntries(file, res);
            }

            int missed2 = LoadMobs(res);

           // foreach (var def in EntityModels)
           // {
			//	Log.Info($"{def.Key}");
            //}

            if (missed1 > 0 || missed2 > 0)
            {
	            Log.Warn($"Failed to process {missed1 + missed2} entity models");
            }

            Log.Info($"Processed {EntityModels.Count} entity models!");
			Log.Info($"Processed {EntityDefinitions.Count} entity definitions");
		}

		/*private void Cache(EntityModel model, Dictionary<string, EntityModelRenderer.ModelBone> modelBones)
		{
			foreach (var bone in model.Bones)
			{
				if (bone == null) continue;
				if (bone.NeverRender) continue;
				bool partOfHead = bone.Name.Contains("head");

				if (bone.Cubes != null)
				{
					List<EntityModelRenderer.ModelBoneCube> c = new List<EntityModelRenderer.ModelBoneCube>();
					ModelBone modelBone;
					foreach (var cube in bone.Cubes)
					{
						if (cube == null)
						{
							Console.WriteLine("Cube was null!");
							continue;
						}

						var size = cube.Size;
						var origin = cube.Origin;
						var pivot = bone.Pivot;
						var rotation = bone.Rotation;

						VertexPositionNormalTexture[] vertices;
						Alex.Graphics.Models.Model.Cube built = new Alex.Graphics.Models.Model.Cube(size, new Vector2(16, 16));
						built.Mirrored = bone.Mirror;
						built.BuildCube(cube.Uv);

						vertices = built.Front.Concat(built.Back).Concat(built.Top).Concat(built.Bottom).Concat(built.Left)
							.Concat(built.Right).ToArray();
						
						var part = new ModelBoneCube(vertices, null, rotation, pivot, origin);

						part.Mirror = bone.Mirror;
						if (partOfHead)
						{
							part.ApplyHeadYaw = true;
							part.ApplyYaw = false;
						}
						else
						{
							part.ApplyPitch = false;
							part.ApplyYaw = true;
							part.ApplyHeadYaw = false;
						}

						c.Add(part);
					}

					modelBone = new ModelBone(c.ToArray());
					if (!modelBones.TryAdd(bone.Name, modelBone))
					{
						Console.WriteLine($"Failed to add bone! {model.Name}:{bone.Name}");
					}
				}
			}
		}*/

        private void LoadEntityDefinition(FileInfo entry, Dictionary<string, EntityDefinition> entityDefinitions)
		{
			using (var open = entry.OpenText())
			{
				var json = open.ReadToEnd();

				string fileName = Path.GetFileNameWithoutExtension(entry.Name);

				Dictionary<string, EntityDefinition> definitions = JsonConvert.DeserializeObject<Dictionary<string, EntityDefinition>>(json);
				foreach (var def in definitions)
				{
					def.Value.Filename = fileName;
					if (!entityDefinitions.ContainsKey(def.Key))
					{
						entityDefinitions.Add(def.Key, def.Value);
					}
				}
            }
		}

        private void GetEntries(FileInfo file, Dictionary<string, EntityModel> entries)
        {
            using (var open = file.OpenText())
	        {
		        var json = open.ReadToEnd();
		        JObject obj = JObject.Parse(json, new JsonLoadSettings());

		        foreach (var e in obj)
		        {
			        if (e.Key == "format_version") continue;
			        if (e.Key == "minecraft:client_entity") continue;
			        //if (e.Key.Contains("zombie")) Console.WriteLine(e.Key);
			        entries.TryAdd(e.Key, e.Value.ToObject<EntityModel>(new JsonSerializer()
			        {
				        Converters = { new Vector3Converter(), new Vector2Converter() }
			        }));
		        }
	        }
        }

        private Dictionary<string, TValue> OrderByLength<TValue>(Dictionary<string, TValue> dictionary)
        {
			return dictionary.OrderBy(obj => obj.Key.Length).ToDictionary(obj => obj.Key, obj => obj.Value);
        }

        private Dictionary<string, TValue> OrderByChild<TValue>(Dictionary<string, TValue> dictionary)
        {
	        return dictionary.OrderBy(obj => obj.Key.Contains(":")).ToDictionary(obj => obj.Key, obj => obj.Value);
        }

        private int LoadMobs(Dictionary<string, EntityModel> entries)
        {
	        int c = 0;
		
			List<string> laterStages = new List<string>();
			Dictionary<string, EntityModel> orderedDict = new Dictionary<string, EntityModel>();
			Dictionary<string, EntityModel> failedToProcess = new Dictionary<string, EntityModel>();

            foreach (var (key, value) in entries)
            {
	            if (!key.Contains(":"))
	            {
		            if (!orderedDict.TryAdd(key, value))
		            {
						Log.Warn($"Failed to add to dictionary! {key}");
		            }
	            }
	            else
	            {
		            if (!laterStages.Contains(key))
			            laterStages.Add(key);
	            }
            }

            orderedDict = OrderByLength(orderedDict);


            foreach (var late in laterStages.ToArray())
            {
	            var split = late.Split(':');
	            string parent = split[1];
                string kid = split[0];

                if (orderedDict.TryGetValue(parent, out EntityModel _))
	            {
		            if (orderedDict.TryAdd(late, entries[late]))
		            {
			            laterStages.Remove(late);
                    }
		            else
		            {
						Log.Warn($"Could not add to ordered dictionary!");
		            }
	            }
	            else
	            {
		            Log.Warn($"Unresolved entity: {late}");
	            }
            }

            orderedDict = OrderByChild(orderedDict);

            foreach (var (key, value) in orderedDict)
            {
	            value.Name = key;

	            if (_processedModels.ContainsKey(key))
		            continue;

	            ProcessEntityModel(value, entries, failedToProcess, false);
            }

            var retryCopy = new Dictionary<string, EntityModel>(failedToProcess.ToArray());

			int fix = 0;
            foreach (var e in retryCopy)
            {
	            if (ProcessEntityModel(e.Value, entries, failedToProcess, true))
	            {
		            fix++;
	            }
            }

            c = retryCopy.Count;

            return failedToProcess.Count - fix;
        }

		private bool ProcessEntityModel(EntityModel model, Dictionary<string, EntityModel> models,
			Dictionary<string, EntityModel> failedToProcess, bool isRetry = false)
		{
			string modelName = model.Name;
			if (model.Name.Contains(":")) //This model inherits from another model.
			{
				string[] split = model.Name.Split(':');
				string parent = split[1];

				if (!_processedModels.TryGetValue(parent, out var parentModel))
				{
					if (!isRetry)
					{
						failedToProcess.TryAdd(modelName, model);

						Log.Warn($"No parent model for {modelName}");
					}

					return false;
				}
				
				modelName = split[0];

				if (model.Bones == null)
				{
					model.Bones = new EntityModelBone[0];
				}

				if (parentModel == null)
				{
                    Log.Warn($"Pass 1 fail... {modelName}");
					return false;
				}

				if (parentModel.Bones == null || parentModel.Bones.Length == 0)
				{
					Log.Warn($"Parent models contains no bones! {modelName}");
					return false;
				}

				Dictionary<string, EntityModelBone> parentBones =
					parentModel.Bones.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Name))
						.ToDictionary(x => x.Name, e => e);

				Dictionary<string, EntityModelBone> bones =
					model.Bones.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Name))
						.ToDictionary(x => x.Name, e => e);

				foreach (var bone in parentBones)
				{
					var parentBone = bone.Value;
					if (bones.TryGetValue(bone.Key, out EntityModelBone val))
					{
						if (!val.Reset)
						{
							if (val.Cubes != null)
							{
								val.Cubes = val.Cubes.Concat(parentBone.Cubes).ToArray();
							}
							else
							{
								val.Cubes = parentBone.Cubes;
							}

							//val.Cubes.Concat(parentBone.Cubes);
						}


						bones[bone.Key] = val;
					}
					else
					{
						bones.Add(bone.Key, parentBone);
					}
				}

				model.Bones = bones.Values.ToArray();
			}

			return _processedModels.TryAdd(modelName, model);
		}

		private void LoadTexture(FileInfo entry)
		{/*
			//var dir = entry.
			//var stream = new StreamReader(Wo);
			string json;
			using (var stream = entry.OpenText())
			{
				json = stream.ReadToEnd();
			}

			Dictionary<string, Bitmap> textures = new Dictionary<string, Bitmap>();
			Dictionary<string, TextureInfoJson> textureJsons = new Dictionary<string, TextureInfoJson>();

			string[] defs = JsonConvert.DeserializeObject<string[]>(json);
			foreach (string def in defs)
			{
				if (textures.ContainsKey(def))
					continue;

				var e = _archive.GetEntry(def + ".png");
				if (e != null && e.IsFile)
				{
					Bitmap bmp = new Bitmap(_archive.GetInputStream(e));
					textures.Add(NormalisePath(def), bmp);
				}

				e = _archive.GetEntry(def + ".json");
				if (e != null && e.IsFile)
				{
					using (var eStream = _archive.GetInputStream(e))
					using (var sr = new StreamReader(eStream))
					{
						var textureJson = sr.ReadToEnd();
						var textureInfo = MCJsonConvert.DeserializeObject<TextureInfoJson>(textureJson);
						textureJsons.Add(NormalisePath(def), textureInfo);
					}
				}
			}

			Textures = textures;
			TextureJsons = textureJsons;
			Log.Info($"Loaded {textures.Count} textures and {textureJsons.Count} texture definitions");*/
		}

		private string a = @"
	public class @NAME@ : Entity
	{

		public @NAME@(int entityTypeId, World level, INetworkProvider network) : base(entityTypeId, level, network)
		{		public Dictionary<string, string> Textures;
		public Dictionary<string, string> Geometry;
			Width = Length = 0.6;
			Height = 1.80;
		}

		public Mob(EntityType mobTypes, World level, INetworkProvider network) : this((int)mobTypes, level, network)
		{
		}

		public override void OnTick()
		{
			base.OnTick();
		}
	}";
	}

	public class EntityDefinition
	{
		[JsonIgnore]
		public string Filename { get; set; } = string.Empty;

		public Dictionary<string, string> Textures;
		public Dictionary<string, string> Geometry;
	}


}

