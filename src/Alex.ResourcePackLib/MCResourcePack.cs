using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Utils;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Alex.ResourcePackLib.Json.Models.Items;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using Color = Microsoft.Xna.Framework.Color;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace Alex.ResourcePackLib
{
	public class McResourcePack : IDisposable
	{
		public delegate void McResourcePackPreloadCallback(IFont font);
		
		private const string BitmapFontCharacters = "\u00c0\u00c1\u00c2\u00c8\u00ca\u00cb\u00cd\u00d3\u00d4\u00d5\u00da\u00df\u00e3\u00f5\u011f\u0130\u0131\u0152\u0153\u015e\u015f\u0174\u0175\u017e\u0207\u0000\u0000\u0000\u0000\u0000\u0000\u0000 !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\u0000\u00c7\u00fc\u00e9\u00e2\u00e4\u00e0\u00e5\u00e7\u00ea\u00eb\u00e8\u00ef\u00ee\u00ec\u00c4\u00c5\u00c9\u00e6\u00c6\u00f4\u00f6\u00f2\u00fb\u00f9\u00ff\u00d6\u00dc\u00f8\u00a3\u00d8\u00d7\u0192\u00e1\u00ed\u00f3\u00fa\u00f1\u00d1\u00aa\u00ba\u00bf\u00ae\u00ac\u00bd\u00bc\u00a1\u00ab\u00bb\u2591\u2592\u2593\u2502\u2524\u2561\u2562\u2556\u2555\u2563\u2551\u2557\u255d\u255c\u255b\u2510\u2514\u2534\u252c\u251c\u2500\u253c\u255e\u255f\u255a\u2554\u2569\u2566\u2560\u2550\u256c\u2567\u2568\u2564\u2565\u2559\u2558\u2552\u2553\u256b\u256a\u2518\u250c\u2588\u2584\u258c\u2590\u2580\u03b1\u03b2\u0393\u03c0\u03a3\u03c3\u03bc\u03c4\u03a6\u0398\u03a9\u03b4\u221e\u2205\u2208\u2229\u2261\u00b1\u2265\u2264\u2320\u2321\u00f7\u2248\u00b0\u2219\u00b7\u221a\u207f\u00b2\u25a0\u0000";
		private const RegexOptions RegexOpts = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;

		private static readonly Regex IsLanguageResource = new Regex(@"^assets\/(?'namespace'.*)\/lang\/(?'filename'.*)\.(?'filetype'json|lang)$", RegexOpts);
		private static readonly Regex IsFontTextureResource = new Regex(@"^assets\/(?'namespace'.*)\/textures\/font\/(?'filename'.*)\.png$", RegexOpts);
		private static readonly Regex IsTextureResource     = new Regex(@"^assets\/(?'namespace'.*)\/textures\/(?'filename'.*)\.png$", RegexOpts);
		private static readonly Regex IsModelRegex          = new Regex(@"^assets\/(?'namespace'.*)\/models\/(?'filename'.*)\.json$", RegexOpts);
		private static readonly Regex IsBlockStateRegex     = new Regex(@"^assets\/(?'namespace'.*)\/blockstates\/(?'filename'.*)\.json$", RegexOpts);
		private static readonly Regex IsGlyphSizes          = new Regex(@"^assets\/(?'namespace'.*)\/font\/glyph_sizes.bin$", RegexOpts);

		private readonly Dictionary<string, BlockStateResource> _blockStates   = new Dictionary<string, BlockStateResource>();
		private readonly Dictionary<string, BlockModel>         _blockModels   = new Dictionary<string, BlockModel>();
		private readonly Dictionary<string, ResourcePackItem>   _itemModels    = new Dictionary<string, ResourcePackItem>();
		//private readonly Dictionary<string, Texture2D>          _textureCache  = new Dictionary<string, Texture2D>();
		private readonly Dictionary<string, Bitmap>             _bitmapCache   = new Dictionary<string, Bitmap>();
		private readonly Dictionary<string, LanguageResource>	_languageCache = new Dictionary<string, LanguageResource>();

		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(McResourcePack));

		public IReadOnlyDictionary<string, BlockStateResource> BlockStates       => _blockStates;
		public IReadOnlyDictionary<string, BlockModel>         BlockModels       => _blockModels;
		public IReadOnlyDictionary<string, ResourcePackItem>   ItemModels        => _itemModels;
		public IReadOnlyDictionary<string, Bitmap>             TexturesAsBitmaps => _bitmapCache;
		//public IReadOnlyDictionary<string, Texture2D>          Textures          => _textureCache;
		public IReadOnlyDictionary<string, LanguageResource>   Languages		 => _languageCache;
		
		public ResourcePackInfo Info { get; private set; }

		public IFont Font { get; private set; }
		
		public bool IsPreLoaded { get; private set; }
		public bool IsLoaded { get; private set; }
		
		private McResourcePackPreloadCallback PreloadCallback { get; } = null;
		private GraphicsDevice Graphics { get; }

		private byte[] GlyphWidth = null;
		
		private Color[] FoliageColors { get; set; } = null;
		private int _foliageWidth = 256;
		private int _foliageHeight = 256;

		private Color[] GrassColors { get; set; } = null;
		private int _grassHeight = 256;
		private int _grassWidth = 256;


		public McResourcePack(byte[] resourcePackData, GraphicsDevice graphicsDevice) : this(new ZipArchive(new MemoryStream(resourcePackData), ZipArchiveMode.Read, false), graphicsDevice, null)
		{

		}

		public McResourcePack(ZipArchive archive, GraphicsDevice graphicsDevice, McResourcePackPreloadCallback preloadCallback)
		{
			PreloadCallback = preloadCallback;
			Graphics = graphicsDevice;
			//_archive = archive;
			Load(archive);
		}


		private void Preload(ZipArchive archive)
		{
			if (IsPreLoaded) return;

			LoadMeta(archive);

			// First load the font, then we can render the splash screen!
			LoadFont(archive);

			IsPreLoaded = true;

			PreloadCallback?.Invoke(Font);
		}

		private void Load(ZipArchive archive)
		{
			if (!IsPreLoaded)
			{
				Preload(archive);
			}

			if (IsLoaded) return;

			Dictionary<string, BlockModel> models = new Dictionary<string, BlockModel>();
			Dictionary<string, ResourcePackItem> items = new Dictionary<string, ResourcePackItem>();

			foreach (var entry in archive.Entries)
			{
				var textureMatchs = IsTextureResource.Match(entry.FullName);
				if (textureMatchs.Success)
				{
					LoadTexture(entry, textureMatchs);
					continue;
				}

				var languageMatchs = IsLanguageResource.Match(entry.FullName);
				if (languageMatchs.Success)
				{
					LoadLocalization(entry, languageMatchs);
					continue;
				}

				var modelMatch = IsModelRegex.Match(entry.FullName);
				if (modelMatch.Success)
				{
					var fileName = modelMatch.Groups["filename"].Value;
					if (fileName.StartsWith("block"))
					{
						var model = LoadBlockModel(entry, modelMatch);
                        if (model != null)
						    models.Add($"{model.Namespace}:{model.Name}", model);
					}
					else if (fileName.StartsWith("item"))
					{
						var item = LoadItemModel(entry, modelMatch);
                        if (item != null)
						    items.Add($"{item.Namespace}:{item.Name}", item);
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

			foreach (var blockModel in models)
			{
				if (!_blockModels.ContainsKey(blockModel.Key))
					ProcessBlockModel(blockModel.Value, ref models);
			}

			foreach (var itemModel in items)
			{
				if (!_itemModels.ContainsKey(itemModel.Key))
					ProcessItem(itemModel.Value, ref items);
			}

			foreach (var blockState in _blockStates.ToArray())
			{
				_blockStates[blockState.Key] = ProcessBlockState(blockState.Value);
			}

			LoadColormap();

			IsLoaded = true;
		}


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

		#region BitmapFont
		
		private void LoadFont(ZipArchive archive)
		{
			List<ZipArchiveEntry> asciiFontEnties = new List<ZipArchiveEntry>();

			foreach (var entry in archive.Entries)
			{
				var fontTextureMatch = IsFontTextureResource.Match(entry.FullName);
				if (fontTextureMatch.Success)
				{
					if (fontTextureMatch.Groups["filename"].Value == "ascii")
					{
						asciiFontEnties.Add(entry);
						
						break;
					}
				}
			}

			foreach (var entry in asciiFontEnties)
			{
				LoadBitmapFont(entry);
			}
		}

		private void LoadBitmapFont(ZipArchiveEntry entry)
		{
			var match = IsTextureResource.Match(entry.FullName);
			
			var fontBitmap = LoadBitmap(entry, match);
			LoadTexture(entry, match);

			Font = new BitmapFont(Graphics, fontBitmap, 16, BitmapFontCharacters.ToCharArray().ToList());
		}
		
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

		#endregion

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
		
		private void LoadTexture(ZipArchiveEntry entry, Match match)
		{
			var textureName = match.Groups["filename"].Value;
			if (!TryGetBitmap(textureName, out var bmp))
			{
				bmp = LoadBitmap(entry, match);
			}
			
		//	_textureCache[match.Groups["filename"].Value] = TextureUtils.ImageToTexture2D(Graphics, bmp);
		}

		public bool TryGetTexture(string textureName, out Texture2D texture)
		{
		//	if (_textureCache.TryGetValue(textureName, out texture))
		//		return true;

			if (TryGetBitmap(textureName, out Bitmap bmp))
			{
				texture = TextureUtils.BitmapToTexture2D(Graphics, bmp);
				return true;
			}

			texture = null;
			return false;
		}

		#endregion
		
		#region Items
		private ResourcePackItem LoadItemModel(ZipArchiveEntry entry, Match match)
		{
			string name = match.Groups["filename"].Value;
			string nameSpace = match.Groups["namespace"].Value;

			using (var r = new StreamReader(entry.Open()))
			{
				var blockModel = MCJsonConvert.DeserializeObject<ResourcePackItem>(r.ReadToEnd());
				blockModel.Name = name;//name.Replace("item/", "");
				blockModel.Namespace = nameSpace;
				if (blockModel.ParentName != null)
				{
				//	blockModel.ParentName = blockModel.ParentName.Replace("item/", "");
				}

				return blockModel;
			}

		}

		private ResourcePackItem ProcessItem(ResourcePackItem model, ref Dictionary<string, ResourcePackItem> models)
		{
			string key = $"{model.Namespace}:{model.Name}";
			if (!string.IsNullOrWhiteSpace(model.ParentName) && !model.ParentName.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				if (!model.ParentName.StartsWith("builtin/", StringComparison.InvariantCultureIgnoreCase))
				{
					string parentKey = $"{model.Namespace}:{model.ParentName}";

					ResourcePackItem parent;
					if (!_itemModels.TryGetValue(parentKey, out parent))
					{
						if (models.TryGetValue(parentKey, out parent))
						{
							parent = ProcessItem(parent, ref models);
						}
					}

					if (parent != null)
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

						foreach (var kvp in parent.Display)
						{
							if (!model.Display.ContainsKey(kvp.Key))
							{
								model.Display.Add(kvp.Key, kvp.Value);
							}
						}
					}
				}
			}

			_itemModels.Add(key, model);

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
            try
            {
                string name = match.Groups["filename"].Value;
                string nameSpace = match.Groups["namespace"].Value;

                using (var r = new StreamReader(entry.Open()))
                {
                    var blockModel = MCJsonConvert.DeserializeObject<BlockModel>(r.ReadToEnd());
                    blockModel.Name = name; // name.Replace("block/", "");
                    blockModel.Namespace = nameSpace;
                    if (blockModel.ParentName != null)
                    {
                        //blockModel.ParentName = blockModel.ParentName.Replace("block/", "");
                    }

                    //blockModel = ProcessBlockModel(blockModel);
                    //_blockModels[$"{nameSpace}:{name}"] = blockModel;
                    return blockModel;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading blockmodel.");

                return null;
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

					foreach (var (k, value) in parent.Textures)
					{
						if (!model.Textures.ContainsKey(k))
						{
							model.Textures.Add(k, value);
						}
					}

					foreach (var (name, value) in parent.Display)
					{
						model.Display[name] = value;
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

		#region Localization

		private void LoadLocalization(ZipArchiveEntry entry, Match match)
		{
			string name = match.Groups["filename"].Value;
			string nameSpace = match.Groups["namespace"].Value;

			bool isJson = match.Groups["filetype"].Value.Equals("json", StringComparison.InvariantCultureIgnoreCase);

			using (var r = new StreamReader(entry.Open()))
			{
				var text = r.ReadToEnd();
				LanguageResource lang;

				if (isJson)
				{
					lang = MCJsonConvert.DeserializeObject<LanguageResource>(text);
				}
				else
				{
					lang = LanguageResource.ParseLangFile(text);
				}

				lang.CultureName = name;
				lang.Namespace   = nameSpace;

				_languageCache[$"{nameSpace}:{name}"] = lang;
			}
		}

		#endregion
		
		public Color GetGrassColor(float temp, float rain, int elevation)
		{
			if (GrassColors == null) return new Color(94, 157, 52);

			temp = MathHelper.Clamp(temp - elevation * 0.00166667f, 0f, 1f);
			rain = MathHelper.Clamp(rain, 0f, 1f) * temp;

			int x = (int)Math.Floor(MathHelper.Clamp(_grassWidth - (_grassWidth * temp), 0, _grassWidth));
			int y = (int)Math.Floor(MathHelper.Clamp(_grassHeight - (_grassHeight * rain), 0, _grassHeight));

			var indx = _grassWidth * y + x;

			if (indx < 0) indx                      = 0;
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

			if (indx < 0) indx                        = 0;
			if (indx > FoliageColors.Length - 1) indx = FoliageColors.Length - 1;

			var result = FoliageColors[indx];

			return new Color(result.R, result.G, result.B);
		}


		public void Dispose()
		{
			//_archive?.Dispose();
		}
	}
}
