using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Alex.API;
using Alex.API.Utils;
using Alex.API.World;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Alex.ResourcePackLib.Json.Models.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using Color = Microsoft.Xna.Framework.Color;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Alex.ResourcePackLib
{
	public class McResourcePack : IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(McResourcePack));
		public ResourcePackInfo Info { get; private set; }

		public IReadOnlyDictionary<string, BlockStateResource> BlockStates => _blockStates;
		public IReadOnlyDictionary<string, BlockModel> BlockModels => _blockModels;
		public IReadOnlyDictionary<string, ResourcePackItem> ItemModels => _itemModels;
		public IReadOnlyDictionary<string, Bitmap> TexturesAsBitmaps => _bitmapCache;
		public IReadOnlyDictionary<string, Texture2D> Textures => _textureCache;

	//	private ZipArchive _archive;
		private readonly Dictionary<string, BlockStateResource> _blockStates = new Dictionary<string, BlockStateResource>();
		private readonly Dictionary<string, BlockModel> _blockModels = new Dictionary<string, BlockModel>();
		private readonly Dictionary<string, ResourcePackItem> _itemModels = new Dictionary<string, ResourcePackItem>();
		private readonly Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();
		private readonly Dictionary<string, Bitmap> _bitmapCache = new Dictionary<string, Bitmap>();

		private Color[] FoliageColors { get; set; } = null;
		private int _foliageWidth = 256;
		private int _foliageHeight = 256;

		private Color[] GrassColors { get; set; } = null;
		private int _grassHeight = 256;
		private int _grassWidth = 256;

		public McResourcePack(byte[] resourcePackData, GraphicsDevice graphicsDevice) : this(new ZipArchive(new MemoryStream(resourcePackData), ZipArchiveMode.Read, false), graphicsDevice, null)
		{

		}

		private Action<IFontRenderer> ReportRenderer = null;
		public McResourcePack(ZipArchive archive, GraphicsDevice graphicsDevice, Action<IFontRenderer> reportFont)
		{
			ReportRenderer = reportFont;
			//_archive = archive;
			Load(archive, graphicsDevice);
		}

		public Color GetGrassColor(float temp, float rain, int elevation)
		{
			if (GrassColors == null) return new Color(94, 157, 52);

			temp = MathHelper.Clamp(temp - elevation * 0.00166667f, 0f, 1f);
			rain = MathHelper.Clamp(rain, 0f, 1f) * temp;

			int x = (int)Math.Floor(MathHelper.Clamp(_grassWidth - (_grassWidth * temp), 0, _grassWidth));
			int y = (int)Math.Floor(MathHelper.Clamp(_grassHeight - (_grassHeight * rain), 0, _grassHeight));

			var indx = _grassWidth * y + x;

			if (indx < 0) indx = 0;
			if (indx > GrassColors.Length - 1) indx = GrassColors.Length - 1;
			
			var result = GrassColors[indx];

			return new Color(result.R, result.G, result.B);
		}

		public Color GetFoliageColor(float temp, float rain, int elevation)
		{
			if (FoliageColors == null) return new Color(94, 157, 52);
			temp = MathHelper.Clamp(temp - elevation * 0.00166667f, 0f, 1f);
			rain = MathHelper.Clamp(rain, 0f, 1f) * temp;

			int x = (int)Math.Floor(MathHelper.Clamp(_foliageWidth - (_foliageWidth * temp), 0, _foliageWidth));
			int y = (int)Math.Floor(MathHelper.Clamp(_foliageHeight - (_foliageHeight * rain), 0, _foliageHeight));

			var indx = _foliageWidth * y + x;

			if (indx < 0) indx = 0;
			if (indx > FoliageColors.Length - 1) indx = FoliageColors.Length - 1;

			var result = FoliageColors[indx];

			return new Color(result.R, result.G, result.B);
		}

		private static readonly Regex IsTextureResource = new Regex(@"assets\/(?'namespace'.*)\/textures\/(?'filename'.*)\.png$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex IsModelRegex = new Regex(@"assets\/(?'namespace'.*)\/models\/(?'filename'.*)\.json$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex IsBlockStateRegex = new Regex(@"assets\/(?'namespace'.*)\/blockstates\/(?'filename'.*)\.json$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex IsGlyphSizes = new Regex(@"assets\/(?'namespace'.*)\/font\/glyph_sizes.bin$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		
		private void Load(ZipArchive archive, GraphicsDevice graphics)
		{
			LoadMeta(archive);

			Dictionary<string, BlockModel> models = new Dictionary<string, BlockModel>();
			foreach (var entry in archive.Entries)
			{
				var textureMatchs = IsTextureResource.Match(entry.FullName);
				if (textureMatchs.Success)
				{
					// TODO: HACK
					//if (textureMatchs.Groups["filename"].Value.StartsWith("colormap/"))
					//{
					//	LoadBitmap(entry, textureMatchs);
					//}

					LoadTexture(graphics, entry, textureMatchs);
					continue;
				}

				var modelMatch = IsModelRegex.Match(entry.FullName);
				if (modelMatch.Success)
				{
					var fileName = modelMatch.Groups["filename"].Value;
					if (fileName.StartsWith("block"))
					{
						var model = LoadBlockModel(entry, modelMatch);
						models.Add($"{model.Namespace}:{model.Name}", model);
					}
					else if (fileName.StartsWith("item"))
					{
						LoadItemModel(entry, modelMatch);
					}

					continue;
				}

				var blockStateMatch = IsBlockStateRegex.Match(entry.FullName);
				if (blockStateMatch.Success)
				{
					LoadBlockState(entry, blockStateMatch);
					continue;
				}

				var glyphSizeMatch = IsGlyphSizes.Match(entry.FullName);
				if (glyphSizeMatch.Success)
				{
					LoadGlyphSizes(entry);
					continue;
				}
			}

			AsciiFont = new FontRenderer(true, this, GlyphWidth);
			ReportRenderer?.Invoke(AsciiFont);

			foreach (var blockModel in models)
			{
				if (!_blockModels.ContainsKey(blockModel.Key))
					ProcessBlockModel(blockModel.Value, ref models);
			}

			foreach (var itemModel in _itemModels.ToArray())
			{
				_itemModels[itemModel.Key] = ProcessItem(itemModel.Value);
			}

			foreach (var blockState in _blockStates.ToArray())
			{
				_blockStates[blockState.Key] = ProcessBlockState(blockState.Value);
			}

			LoadColormap();
		}

		private byte[] GlyphWidth = null;

		private void LoadMeta(ZipArchive archive)
		{
			ResourcePackInfo info;

			var entry = archive.GetEntry("pack.mcmeta");
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

			Info = info;
		}

		public FontRenderer AsciiFont { get; private set; } = null;

		private void LoadGlyphSizes(ZipArchiveEntry entry)
		{
			byte[] glyphWidth = new byte[65536];
			using (Stream stream = entry.Open())
			{
				int length = stream.Read(glyphWidth, 0, glyphWidth.Length);
				Array.Resize(ref glyphWidth, length);
			}

			GlyphWidth = glyphWidth;
		}

		#region Bitmap
		
		private void LoadColormap()
		{
			if (TryGetBitmap("colormap/foliage", out Bitmap foliage))
			{
				var foliageColors = new LockBitmap(foliage);
				foliageColors.LockBits();
				FoliageColors = foliageColors.GetColorArray();
				foliageColors.UnlockBits();

				_foliageHeight = foliageColors.Height;
				_foliageWidth  = foliageColors.Width;
			}

			if (TryGetBitmap("colormap/grass", out Bitmap grass))
			{
				var grassColors = new LockBitmap(grass);
				grassColors.LockBits();
				GrassColors = grassColors.GetColorArray();
				grassColors.UnlockBits();

				_grassWidth  = grassColors.Width;
				_grassHeight = grassColors.Height;
			}
		}

		private Bitmap LoadBitmap(ZipArchiveEntry entry, Match match)
		{
			Bitmap img;
			using (var s = entry.Open())
			{
				img = new Bitmap(s);
			}

			_bitmapCache[match.Groups["filename"].Value] = img;
			return img;
		}

		public bool TryGetBitmap(string textureName, out Bitmap bitmap)
		{
			if (_bitmapCache.TryGetValue(textureName, out bitmap))
				return true;
			
			bitmap = null;
			return false;
		}

		#endregion

		#region Texture2D
		
		private void LoadTexture(GraphicsDevice graphics, ZipArchiveEntry entry, Match match)
		{
			var textureName = match.Groups["filename"].Value;
			if (!TryGetBitmap(textureName, out var bmp))
			{
				bmp = LoadBitmap(entry, match);
			}
			
			_textureCache[match.Groups["filename"].Value] = TextureUtils.ImageToTexture2D(graphics, bmp);
		}

		public bool TryGetTexture(BlockModel model, string textureName, out Texture2D texture)
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

			texture = null;
			return false;
		}

		public bool TryGetTexture(string textureName, out Texture2D texture)
		{
			if (_textureCache.TryGetValue(textureName, out texture))
				return true;

			if (TryGetBitmap(textureName, out Bitmap bmp))
			{

			}

			texture = null;
			return false;
		}

		#endregion
		
		#region Items
		private void LoadItemModel(ZipArchiveEntry entry, Match match)
		{
			string name = match.Groups["filename"].Value;
			string nameSpace = match.Groups["namespace"].Value;

			using (var r = new StreamReader(entry.Open()))
			{
				var blockModel = MCJsonConvert.DeserializeObject<ResourcePackItem>(r.ReadToEnd());
				blockModel.Name = name;
				blockModel.Namespace = nameSpace;

				//blockModel = ProcessItem(blockModel);
				_itemModels[$"{nameSpace}:{name}"] = blockModel;
			}

		}

		private ResourcePackItem ProcessItem(ResourcePackItem model)
		{
			if (!string.IsNullOrWhiteSpace(model.Parent) && !model.Parent.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				
			}

			return model;
		}
		

		#endregion

		#region Block States
		
		private const string DefaultNamespace = "minecraft";
		
		private BlockStateResource ProcessBlockState(BlockStateResource blockStateResource)
		{
			if (blockStateResource.Parts.Length > 0)
			{
				foreach (var part in blockStateResource.Parts)
				{
					foreach (var sVariant in part.Apply)
					{
						if (!TryGetBlockModel(sVariant.ModelName, out BlockModel model))
						{
							Log.Debug($"Could not get multipart blockmodel! Variant: {blockStateResource} Model: {sVariant.ModelName}");
							continue;
						}

						sVariant.Model = model;
					}
				}
			}
			else
			{
				foreach (var variant in blockStateResource.Variants)
				{
					foreach (var sVariant in variant.Value)
					{
						if (!TryGetBlockModel(sVariant.ModelName, out BlockModel model))
						{
							Log.Debug($"Could not get blockmodel for variant! Variant: {variant.Key} Model: {sVariant.ModelName}");
							continue;
						}

						sVariant.Model = model;
					}
				}
			}

			return blockStateResource;
		}
		
		private void LoadBlockState(ZipArchiveEntry entry, Match match)
		{
			try
			{
				string name      = match.Groups["filename"].Value;
				string nameSpace = match.Groups["namespace"].Value;

				using (var r = new StreamReader(entry.Open()))
				{
					var json = r.ReadToEnd();

					var blockState = MCJsonConvert.DeserializeObject<BlockStateResource>(json);
					blockState.Name      = name;
					blockState.Namespace = nameSpace;

					
					_blockStates[$"{nameSpace}:{name}"] = blockState;

					//return blockState;
				}
			}
			catch (Exception ex)
			{
				Log.Warn($"Could not load {entry.Name}!", ex);
				//	return null;
			}
		}
		
		public bool TryGetBlockState(string modelName, out BlockStateResource stateResource)
		{
			if (_blockStates.TryGetValue(modelName, out stateResource))
				return true;

			stateResource = null;
			return false;
		}
		
		#endregion

		#region Block Models

		private BlockModel LoadBlockModel(ZipArchiveEntry entry, Match match)
		{
			string name      = match.Groups["filename"].Value;
			string nameSpace = match.Groups["namespace"].Value;

			using (var r = new StreamReader(entry.Open()))
			{
				var blockModel = MCJsonConvert.DeserializeObject<BlockModel>(r.ReadToEnd());
				blockModel.Name      = name.Replace("block/", "");
				blockModel.Namespace = nameSpace;
				if (blockModel.ParentName != null)
				{
					blockModel.ParentName = blockModel.ParentName.Replace("block/", "");
				}
				//blockModel = ProcessBlockModel(blockModel);
				//_blockModels[$"{nameSpace}:{name}"] = blockModel;
				return blockModel;
			}
		}

		private BlockModel ProcessBlockModel(BlockModel model, ref Dictionary<string, BlockModel> models)
		{
			string key = $"{model.Namespace}:{model.Name}";
			if (!string.IsNullOrWhiteSpace(model.ParentName) && !model.ParentName.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				string parentKey = $"{model.Namespace}:{model.ParentName}";

				BlockModel parent;
				if (!_blockModels.TryGetValue(parentKey, out parent))
				{
					if (models.TryGetValue(parentKey, out parent))
					{
						parent = ProcessBlockModel(parent, ref models);
					}
				}

				if (parent != null)
				{
					model.Parent = parent;

					if (model.Elements.Length == 0 && parent.Elements.Length > 0)
					{
						model.Elements = (BlockModelElement[])parent.Elements.Clone();
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

			_blockModels.Add(key, model);

			return model;
		}
		
		public bool TryGetBlockModel(string modelName, out BlockModel model)
		{
			string @namespace = DefaultNamespace;
			var    split      = modelName.Split(':');
			if (split.Length > 1)
			{
				@namespace = split[0];
				modelName  = split[1];
			}

			return TryGetBlockModel(@namespace, modelName, out model);
		}

		public bool TryGetBlockModel(string @namespace, string modelName, out BlockModel model)
		{
			string fullName = $"{@namespace}:{modelName}";

			if (_blockModels.TryGetValue(fullName, out model))
				return true;

			var m = _blockModels.FirstOrDefault(x => x.Value.Name.EndsWith(modelName, StringComparison.InvariantCultureIgnoreCase))
			                    .Value;

			if (m != null)
			{
				model = m;
				return true;
			}

			model = null;
			return false;
		}
		
		#endregion

		public void Dispose()
		{
			//_archive?.Dispose();
		}
	}
}
