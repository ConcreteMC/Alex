using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ResourcePackLib.CoreRT.Json;
using ResourcePackLib.CoreRT.Json.BlockStates;
using ResourcePackLib.CoreRT.Json.Models;

namespace ResourcePackLib.CoreRT
{
	public class MCResourcePack : IDisposable
	{
		public ResourcePackInfo Info { get; private set; }

		public IReadOnlyDictionary<string, BlockState> BlockStates => _blockStates;
		public IReadOnlyDictionary<string, BlockModel> BlockModels => _blockModels;

		private ZipArchive _archive;
		private readonly Dictionary<string, BlockState> _blockStates = new Dictionary<string, BlockState>();
		private readonly Dictionary<string, BlockModel> _blockModels = new Dictionary<string, BlockModel>();
		private readonly Dictionary<string, Bitmap> _textureCache = new Dictionary<string, Bitmap>();

		public MCResourcePack(byte[] resourcePackData) : this(new ZipArchive(new MemoryStream(resourcePackData), ZipArchiveMode.Read, false))
		{

		}

		public MCResourcePack(ZipArchive archive)
		{
			_archive = archive;
			Load();
		}

		private void Load()
		{
			LoadMeta();
			LoadBlockModels();
			LoadBlockStates();
		}

		private void LoadMeta()
		{
			ResourcePackInfo info;

			var entry = _archive.GetEntry("pack.mcmeta");
			if (entry == null)
			{
				info = new ResourcePackInfo();
			}
			else
			{
				using (TextReader reader = new StreamReader(entry.Open()))
				{
					ResourcePackInfoWrapper wrap = MCJsonConvert.DeserializeObject<ResourcePackInfoWrapper>(reader.ReadToEnd());
					info = wrap.pack;
				}
			}


			var imgEntry = _archive.GetEntry("pack.png");
			if (imgEntry != null)
			{
				Bitmap bmp = new Bitmap(imgEntry.Open());
				info.Logo = bmp;
			}
		}

		private void LoadBlockModels()
		{
			var jsonFiles = _archive.Entries
				.Where(e => e.FullName.StartsWith("assets/minecraft/models/block/") && e.FullName.EndsWith(".json")).ToArray();

			foreach (var jsonFile in jsonFiles)
			{
				LoadBlockModel(jsonFile);
			}
		}

		private BlockModel LoadBlockModel(ZipArchiveEntry entry)
		{
			string name = Path.GetFileNameWithoutExtension(entry.FullName);
			using (var r = new StreamReader(entry.Open()))
			{
				var blockModel = MCJsonConvert.DeserializeObject<BlockModel>(r.ReadToEnd());
				blockModel.Name = name;

				blockModel = ProcessBlockModel(blockModel);
				_blockModels[name] = blockModel;

				return blockModel;
			}
		}

		public bool TryGetBlockModel(string modelName, out BlockModel model)
		{
			if (modelName.StartsWith("block/"))
				modelName = modelName.Replace("block/", "");

			//		if (_blockModels.TryGetValue(modelName, out model))
			//		return true;

			model = null;

			var modelFile =
				_archive.Entries.FirstOrDefault(e => e.FullName.Equals("assets/minecraft/models/block/" + modelName + ".json"));

			if (modelFile == null)
			{
				Debug.WriteLine("Failed to load Block Model: File Not Found (" + "assets/minecraft/models/block/" + modelName + ".json)");
				return false;
			}

			model = LoadBlockModel(modelFile);
			return true;
		}

		private void LoadBlockStates()
		{
			var jsonFiles = _archive.Entries
				.Where(e => e.FullName.StartsWith("assets/minecraft/blockstates/") && e.FullName.EndsWith(".json")).ToArray();

			foreach (var jsonFile in jsonFiles)
			{
				LoadBlockState(jsonFile);
			}
		}

		private BlockState LoadBlockState(ZipArchiveEntry entry)
		{
			using (var r = new StreamReader(entry.Open()))
			{
				var json = r.ReadToEnd();

				var blockState = MCJsonConvert.DeserializeObject<BlockState>(json);
				_blockStates[entry.Name.Replace(".json", "")] = ProcessBlockState(blockState);

				return blockState;
			}
		}

		public bool TryGetBlockState(string modelName, out BlockState state)
		{
			if (_blockStates.TryGetValue(modelName, out state))
				return true;

			state = null;

			var modelFile =
				_archive.Entries.FirstOrDefault(e => e.FullName.Equals($"assets/minecraft/blockstates/{modelName}.json"));

			if (modelFile == null)
			{
				Debug.WriteLine("Failed to load BlockState: File Not Found (" + "assets/minecraft/blockstates/" + modelName + ".json)");
				return false;
			}

			state = LoadBlockState(modelFile);
			return true;
		}

		public bool TryGetTexture(BlockModel model, string textureName, out Bitmap texture)
		{
			while (textureName.StartsWith("#"))
			{
				if (!model.Textures.TryGetValue(textureName.TrimStart('#'), out textureName))
				{
					texture = null;
					return false;
				}
			}

			if (_textureCache.TryGetValue(textureName, out texture))
				return true;

			var textureFile =
				_archive.Entries.FirstOrDefault(e => e.FullName.Equals("assets/minecraft/textures/" + textureName + ".png"));
			if (textureFile == null) return false;

			using (var s = textureFile.Open())
			{
				var img = new Bitmap(s);

				_textureCache[textureName] = img;

				texture = img;
				return true;
			}
		}

		private BlockModel ProcessBlockModel(BlockModel model)
		{
			if (!string.IsNullOrWhiteSpace(model.ParentName) && !model.ParentName.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				BlockModel parent;
				if (TryGetBlockModel(model.ParentName, out parent))
				{
					model.Parent = parent;

					if (model.Elements.Length == 0 && parent.Elements.Length > 0)
					{
						model.Elements = (BlockModelElement[]) parent.Elements.Clone();
					}

					foreach (var kvp in parent.Textures)
					{
						if (!model.Textures.ContainsKey(kvp.Key))
						{
							model.Textures.Add(kvp.Key, kvp.Value);
						}
					}
				}
			}

			return model;
		}

		private BlockState ProcessBlockState(BlockState blockState)
		{
			foreach (var variant in blockState.Variants)
			{
				foreach (var sVariant in variant.Value)
				{
					if (!TryGetBlockModel("block/" + sVariant.ModelName, out BlockModel model))
					{
						Debug.WriteLine($"Could not get blockmodel for variant! Variant: {variant.Key} Model: {sVariant.ModelName}");
						continue;
					}

					sVariant.Model = model;
				}
			}

			return blockState;
		}

		public void Dispose()
		{
			_archive?.Dispose();
		}
	}
}
