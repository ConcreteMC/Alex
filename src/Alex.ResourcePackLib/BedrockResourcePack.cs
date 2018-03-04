using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using Alex.ResourcePackLib.Json.Models;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib
{
	public class BedrockResourcePack : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(BedrockResourcePack));

		public IReadOnlyDictionary<string, EntityModel> EntityModels { get; private set; }
		public IReadOnlyDictionary<string, Bitmap> EntityTextures { get; private set; }
		private ZipFile _archive;
		public BedrockResourcePack(ZipFile archive)
		{
			_archive = archive;
			
			Load();
		}

		public BedrockResourcePack(byte[] resourceData) : this(new ZipFile(new MemoryStream(resourceData))
		{
			IsStreamOwner = true
		})
		{

		}

		private void Load()
		{
			foreach (ZipEntry entry in _archive)
			{
				if (entry.IsDirectory)
				{
					CheckDirectory(entry);
					continue;
				}

				if (entry.IsFile)
				{
					CheckFile(entry);
				}
			}
		}

		private bool EntitysLoaded { get; set; } = false;
		private void CheckFile(ZipEntry entry)
		{
			if (entry.Name.EndsWith("mobs.json") && !EntitysLoaded)
			{
				LoadMobs(entry);
			}
			else if (entry.Name.EndsWith("textures_list.json"))
			{
				LoadTexture(entry);
			}
		}

		private void CheckDirectory(ZipEntry entry)
		{
			
		}

		private void LoadTexture(ZipEntry entry)
		{
			var stream = new StreamReader(_archive.GetInputStream(entry));
			var json = stream.ReadToEnd();

			Dictionary<string, Bitmap> textures = new Dictionary<string, Bitmap>();
			string[] definitions = JsonConvert.DeserializeObject<string[]>(json);
			foreach (string def in definitions)
			{
				if (textures.ContainsKey(def))
					continue;

				if (def.StartsWith("textures/entity/"))
				{
					var e = _archive.GetEntry(def + ".png");
					if (e != null && e.IsFile)
					{
						Bitmap bmp = new Bitmap(_archive.GetInputStream(e));
						textures.Add(def, bmp);
					}
				}
			}

			EntityTextures = textures;
			Log.Info($"Loaded {textures.Count} entity textures");
		}

		private void LoadMobs(ZipEntry entry)
		{
			var stream = new StreamReader(_archive.GetInputStream(entry));
			var json = stream.ReadToEnd();

			Dictionary<string, EntityModel> entries = JsonConvert.DeserializeObject<Dictionary<string, EntityModel>>(json);
			Dictionary<string, EntityModel> processedModels = new Dictionary<string, EntityModel>();
			foreach (var e in entries)
			{
				e.Value.Name = e.Key;
			}

			foreach (var e in entries)
			{
				if (processedModels.ContainsKey(e.Key))
					continue;

				ProcessModel(e.Value, entries, processedModels);
			}

			EntityModels = processedModels;

			Log.Info($"Imported {processedModels.Count} entity models");
		}

		private void ProcessModel(EntityModel model, Dictionary<string, EntityModel> models, Dictionary<string, EntityModel> processedModels)
		{
			string modelName = model.Name;
			if (model.Name.Contains(":")) //This model inherits from another model.
			{
				string[] split = model.Name.Split(':');
				string parent = split[1];
				modelName = split[0];

				EntityModel parentModel;
				if (!processedModels.TryGetValue(parent, out parentModel))
				{
					if (models.TryGetValue(parent, out parentModel))
					{
						ProcessModel(parentModel, models, processedModels);
						parentModel = processedModels[parent];
					}
					else
					{
						Log.Warn($"Failed to find parent model (Stage 1)! {model.Name}");
						return;
					}
				}

				if (model.Bones == null)
				{
					model.Bones = new EntityModelBone[0];
				}

				if (parentModel == null)
				{
					Log.Warn($"Failed to find parent model (Stage 2)! {model.Name}");
					return;
				}

				if (parentModel.Bones == null || parentModel.Bones.Length == 0)
				{
					Log.Warn($"Parent models contains no bones! {model.Name}");
					return;
				}

				Dictionary<string, EntityModelBone> parentBones =
					parentModel.Bones.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Name)).ToDictionary(x => x.Name, e => e);

				Dictionary<string, EntityModelBone> bones =
					model.Bones.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Name)).ToDictionary(x => x.Name, e => e);

				int inheritedBones = 0;
				foreach (var bone in parentBones)
				{
					if (!bones.ContainsKey(bone.Key))
					{
						bones.Add(bone.Key, bone.Value);
						inheritedBones++;
					}
				}

				model.Bones = bones.Values.ToArray();
				Log.Info($"Processed {modelName} inherited {inheritedBones} bones from {parent}");
			}

			processedModels.Add(modelName, model);
		}

		public void Dispose()
		{
			_archive?.Close();
		}
	}
}