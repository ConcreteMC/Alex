using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Resources;
using Alex.API.Utils;
using Alex.ResourcePackLib.Abstraction;
using Alex.ResourcePackLib.Generic;
using Alex.ResourcePackLib.IO;
using Alex.ResourcePackLib.IO.Abstract;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Fonts;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.ResourcePackLib.Json.Sound;
using Alex.ResourcePackLib.Json.Textures;
using Microsoft.Xna.Framework;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Color = Microsoft.Xna.Framework.Color;
using Image = SixLabors.ImageSharp.Image;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace Alex.ResourcePackLib
{
	public class McResourcePack : ResourcePack, ITextureProvider, IBlockStateResourceProvider, IDisposable
	{
		public delegate void McResourcePackPreloadCallback(Image<Rgba32> fontBitmap, List<char> bitmapFontCharacters);

		public const string BitmapFontCharacters = "\u00c0\u00c1\u00c2\u00c8\u00ca\u00cb\u00cd\u00d3\u00d4\u00d5\u00da\u00df\u00e3\u00f5\u011f\u0130\u0131\u0152\u0153\u015e\u015f\u0174\u0175\u017e\u0207\u0000\u0000\u0000\u0000\u0000\u0000\u0000 !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\u0000\u00c7\u00fc\u00e9\u00e2\u00e4\u00e0\u00e5\u00e7\u00ea\u00eb\u00e8\u00ef\u00ee\u00ec\u00c4\u00c5\u00c9\u00e6\u00c6\u00f4\u00f6\u00f2\u00fb\u00f9\u00ff\u00d6\u00dc\u00f8\u00a3\u00d8\u00d7\u0192\u00e1\u00ed\u00f3\u00fa\u00f1\u00d1\u00aa\u00ba\u00bf\u00ae\u00ac\u00bd\u00bc\u00a1\u00ab\u00bb\u2591\u2592\u2593\u2502\u2524\u2561\u2562\u2556\u2555\u2563\u2551\u2557\u255d\u255c\u255b\u2510\u2514\u2534\u252c\u251c\u2500\u253c\u255e\u255f\u255a\u2554\u2569\u2566\u2560\u2550\u256c\u2567\u2568\u2564\u2565\u2559\u2558\u2552\u2553\u256b\u256a\u2518\u250c\u2588\u2584\u258c\u2590\u2580\u03b1\u03b2\u0393\u03c0\u03a3\u03c3\u03bc\u03c4\u03a6\u0398\u03a9\u03b4\u221e\u2205\u2208\u2229\u2261\u00b1\u2265\u2264\u2320\u2321\u00f7\u2248\u00b0\u2219\u00b7\u221a\u207f\u00b2\u25a0\u0000";
		private const RegexOptions RegexOpts = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;

		private static readonly Regex IsLanguageResource    = new(@"^assets[\\\/]((?'namespace'.*)(?<!realms))[\\\/]lang[\\\/](?'filename'.*)[\\.](?'filetype'json|lang)$", RegexOpts);
		private static readonly Regex IsFontTextureResource = new(@"^assets[\\\/](?'namespace'.*)[\\\/]textures[\\\/]font[\\\/](?'filename'.*)\.png$", RegexOpts);
		private static readonly Regex IsTextureResource     = new(@"^assets[\\\/](?'namespace'.*)[\\\/]textures[\\\/](?'filename'.*)\.png$", RegexOpts);
		private static readonly Regex IsTextureMetaResource = new(@"^assets[\\\/](?'namespace'.*)[\\\/]textures[\\\/](?'filename'.*)\.png.mcmeta$", RegexOpts);
		private static readonly Regex IsModelRegex          = new(@"^assets[\\\/](?'namespace'.*)[\\\/]models[\\\/](?'filename'.*)\.json$", RegexOpts);
		private static readonly Regex IsBlockStateRegex     = new(@"^assets[\\\/](?'namespace'.*)[\\\/]blockstates[\\\/](?'filename'.*)\.json$", RegexOpts);
		private static readonly Regex IsGlyphSizes          = new(@"^assets[\\\/](?'namespace'.*)[\\\/]font[\\\/]glyph_sizes.bin$", RegexOpts);
		private static readonly Regex IsFontDefinition      = new(@"^assets[\\\/](?'namespace'.*)[\\\/]font[\\\/](?'filename'.*)\.json$", RegexOpts);
		private static readonly Regex IsParticle            = new(@"^assets[\\\/](?'namespace'.*)[\\\/]particles[\\\/](?'filename'.*)\.json$", RegexOpts);
		private static readonly Regex IsSoundDefinition     = new(@"^assets[\\\/](?'namespace'.*)[\\\/]sounds.json$", RegexOpts);

		private readonly Dictionary<ResourceLocation, Lazy<BlockStateResource>> _blockStates   = new();
		public Dictionary<ResourceLocation, ResourcePackModelBase>   Models    = new();
		private readonly Dictionary<ResourceLocation, Lazy<Image<Rgba32>>> _bitmapCache      = new();
		private readonly Dictionary<ResourceLocation, TextureMeta>         _textureMetaCache = new();
		private readonly Dictionary<string, LanguageResource>              _languageCache    = new(StringComparer.OrdinalIgnoreCase);

		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(McResourcePack));
		//public IReadOnlyDictionary<ResourceLocation, TextureMeta> TextureMetas => _textureMetaCache;
		public IReadOnlyDictionary<ResourceLocation, Lazy<Image<Rgba32>>>          Textures          => _bitmapCache;
		public IReadOnlyDictionary<string, LanguageResource>   Languages		 => _languageCache;
		
		//public new ResourcePackInfo Info { get; private set; }

		//public IFont Font { get; private set; }
	
		public bool IsPreLoaded { get; private set; }
		public bool IsLoaded { get; private set; }
		
		private McResourcePackPreloadCallback PreloadCallback  { get; }      = null;

		private byte[] GlyphWidth = null;
		
		private Color[] FoliageColors { get; set; } = null;
		private int _foliageWidth = 256;
		private int _foliageHeight = 256;

		private Color[] GrassColors { get; set; } = null;
		private int _grassHeight = 256;
		private int _grassWidth = 256;
		
		public Image<Rgba32> FontBitmap { get; private set; }

		private PngDecoder                           PngDecoder       { get; }
		public  IDictionary<string, SoundDefinition> SoundDefinitions { get; private set; }
		
		private IFilesystem Filesystem  { get; set; }
		public McResourcePack(IFilesystem archive, McResourcePackPreloadCallback preloadCallback, LoadProgress progressReporter = null)
		{
			Filesystem = archive;
			ProgressReporter = progressReporter;
			
			PngDecoder = new PngDecoder()
			{
				IgnoreMetadata = true
			};
			PreloadCallback = preloadCallback;
			//_archive = archive;
			Load(archive);
		}


		private void Preload(IFilesystem archive)
		{
			if (IsPreLoaded) return;

			LoadMeta(archive);

			// First load the font, then we can render the splash screen!
			LoadFont(archive);

			IsPreLoaded = true;

			//if (FontBitmap != null)
			//{
			//	PreloadCallback?.Invoke(FontBitmap, BitmapFontCharacters.ToCharArray().ToList());
			//}
		}

		private string SanitizeFilename(string filename)
		{
			return filename.Replace("\\", "/");
		}

		private void Load(IFilesystem archive)
		{
			if (!IsPreLoaded)
			{
				Preload(archive);
			}

			if (IsLoaded) return;

			//if (Info == null)
			//{
			//	Info = GetManifest(archive, ResourcePackType.Java);
			//}

			//Dictionary<ResourceLocation, ResourcePackModelBase> models = new Dictionary<ResourceLocation, ResourcePackModelBase>();

			var total = archive.Entries.Count;
			int count = 0;
			foreach (var entry in archive.Entries)
			{
				count++;
				
				ProgressReporter?.Invoke((int)(((double)count / (double)total) * 100D), entry.Name);

				if (IsParticle.IsMatch(entry.FullName))
				{
					continue;
				}
				
				if (IsSoundDefinition.IsMatch(entry.FullName))
				{
					using (var e = entry.Open())
					{
						SoundDefinitions = SoundDefinition.FromJson(Encoding.UTF8.GetString(e.ReadToEnd()));
					}

					continue;
				}
				
				var textureMatchs = IsTextureResource.Match(entry.FullName);
				if (textureMatchs.Success)
				{
					ProcessTexture(entry, textureMatchs);
					continue;
				}

				var textureMetaMatch = IsTextureMetaResource.Match(entry.FullName);
				if (textureMetaMatch.Success)
				{
					LoadTextureMeta(entry, textureMetaMatch);
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
					var resourceLocation = new ResourceLocation(modelMatch.Groups["namespace"].Value, SanitizeFilename(modelMatch.Groups["filename"].Value));

					var model = ReadModel(entry, resourceLocation);
					if (model != null)
						Models.Add(resourceLocation, model);

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

			LoadColormap();

			IsLoaded = true;
		}


		private void LoadMeta(IFilesystem archive)
		{
			Info = GetManifest(archive);
			/*ResourcePackInfo info;

			var entry = archive.GetEntry("pack.mcmeta");
			if (entry == null)
			{
				info = new ResourcePackInfo();
			}
			else
			{
				using (var stream = entry.Open())
				{
					ResourcePackInfoWrapper wrap =
						MCJsonConvert.DeserializeObject<ResourcePackInfoWrapper>(
							Encoding.UTF8.GetString(stream.ReadToSpan(entry.Length)));
					info = wrap.pack;
				}
			}

			Info = info;*/
		}

		private void LoadTextureMeta(IFile entry, Match match)
		{
			if (!TryGetTextureMeta(new ResourceLocation(match.Groups["namespace"].Value, SanitizeFilename(match.Groups["filename"].Value)), out var meta))
			{
				LoadBitmapMeta(entry, match);
			}
		}

		private void ProcessTexture(IFile entry, Match match)
		{
			try
			{
				LoadBitmap(entry, match);
			}
			catch (Exception ex)
			{
				Log.Warn(ex, $"Could not load texture from resourcepack: {entry.FullName}");
			}
		}

		private Lazy<Image<Rgba32>> LoadBitmap(IFile entry, Match match)
		{
			var resource = new ResourceLocation(match.Groups["namespace"].Value, SanitizeFilename(match.Groups["filename"].Value));

			if (_bitmapCache.TryGetValue(resource, out var result))
			{
				return result;
			}

			_bitmapCache[resource] = new Lazy<Image<Rgba32>>(
				() =>
				{
					Image<Rgba32> img;
					using (var s = entry.Open())
					{
						//img = new Bitmap(s);
						var data = s.ReadToSpan(entry.Length);
						return Image.Load<Rgba32>(data, PngDecoder);
					}
				});
			
			return _bitmapCache[resource];
		}
		
		#region BitmapFont

		private void LoadFontDefinition(IFile entry)
		{
			ReadOnlySpan<byte> content;
			using (var stream = entry.Open())
			{
				content = stream.ReadToEnd();
			}

			FontDefinitionFile file = MCJsonConvert.DeserializeObject<FontDefinitionFile>(Encoding.UTF8.GetString(content));

			if (file?.Providers == null)
				return;

		}
		
		private void LoadFont(IFilesystem archive)
		{
			List<IFile> asciiFontEnties = new List<IFile>();

			foreach (var entry in archive.Entries)
			{
				var fontDefinitionMatch = IsFontDefinition.Match(entry.FullName);

				if (fontDefinitionMatch.Success)
				{
					LoadFontDefinition(entry);
					continue;
				}
				
				var fontTextureMatch    = IsFontTextureResource.Match(entry.FullName);
				if (fontTextureMatch.Success)
				{
					if (fontTextureMatch.Groups["filename"].Value == "ascii")
					{
						asciiFontEnties.Add(entry);
						
						break;
					}
					
					continue;
				}
			}

			foreach (var entry in asciiFontEnties)
			{
				LoadBitmapFont(entry);
			}
		}

		private bool DidPreload { get; set; } = false;
		private void LoadBitmapFont(IFile entry)
		{
			var match = IsFontTextureResource.Match(entry.FullName);

			if (match.Success)
			{
				var fontBitmap = LoadBitmap(entry, match);
				//ProcessTexture(entry, match);

				FontBitmap = fontBitmap.Value;

				if (!DidPreload)
				{
					DidPreload = true;

					PreloadCallback?.Invoke(FontBitmap, BitmapFontCharacters.ToCharArray().ToList());
				}
			}

			//Log.Info($"Font pixelformat: {fontBitmap.PixelFormat} | RawFormat: {fontBitmap.RawFormat}");
			//Font = new BitmapFont(Graphics, fontBitmap, 16, BitmapFontCharacters.ToCharArray().ToList());
		}
		
		private void LoadGlyphSizes(IFile entry)
		{
			byte[] glyphWidth;// = new byte[65536];
			using (Stream stream = entry.Open())
			{
				glyphWidth = stream.ReadToSpan(entry.Length).ToArray();
				//int length = stream.Read(glyphWidth, 0, glyphWidth.Length);
				//Array.Resize(ref glyphWidth, length);
			}

			GlyphWidth = glyphWidth;
		}

		#endregion

		#region Bitmap
		
		private void LoadColormap()
		{
			if (TryGetBitmap("colormap/foliage", out Image<Rgba32> foliage))
			{
				FoliageColors = GetColorArray(foliage);

				_foliageHeight = foliage.Height;
				_foliageWidth  = foliage.Width;
			}

			if (TryGetBitmap("colormap/grass", out Image<Rgba32> grass))
			{
				GrassColors = GetColorArray(grass);

				_grassWidth  = grass.Width;
				_grassHeight = grass.Height;
			}
		}
		
		private Color[] GetColorArray(Image<Rgba32> image)
		{
			var cloned = image;
			if (cloned.TryGetSinglePixelSpan(out var pixelSpan))
			{
				return pixelSpan.ToArray().Select(x => new Color(x.Rgba)).ToArray();
			}

			return null;
			
			Color[] colors = new Color[cloned.Width * cloned.Height];
			for (int x = 0; x < cloned.Width; x++)
			{
				for (int y = 0; y < cloned.Height; y++)
				{
					var indx = cloned.Width * y + x;
					colors[indx] = new Color(cloned[x, y].Rgba);
				}
			}

			return colors;
		}

		public bool TryGetBitmap(ResourceLocation textureName, out Image<Rgba32> bitmap)
		{
			if (_bitmapCache.TryGetValue(textureName, out var val))
			{
				bitmap = val.Value.Clone();
				return true;
			}

			bitmap = null;
			return false;
		}
		
		private void LoadBitmapMeta(IFile entry, Match match)
		{
			TextureMeta meta;
			using (var stream = entry.Open())
			{
				//using (StreamReader sr = new StreamReader(stream))
				{
					//string content = sr.ReadToEnd();
					meta = TextureMeta.FromJson(Encoding.UTF8.GetString(stream.ReadToSpan(entry.Length)));
				}
			}

			_textureMetaCache[new ResourceLocation(match.Groups["namespace"].Value, SanitizeFilename(match.Groups["filename"].Value))] = meta;
		}

		public bool TryGetTextureMeta(ResourceLocation textureName, out TextureMeta meta)
		{
			if (_textureMetaCache.TryGetValue(textureName, out meta))
				return true;
			
			meta = null;
			return false;
		}

		#endregion

		#region Block States

		private void LoadBlockState(IFile entry, Match match)
		{
			try
			{
				string name      = match.Groups["filename"].Value;
				string nameSpace = match.Groups["namespace"].Value;
				string key = $"{nameSpace}:{name}";

				_blockStates[key] = new Lazy<BlockStateResource>(
					() =>
					{
						using (var stream = entry.Open())
						{
							var json = Encoding.UTF8.GetString(stream.ReadToSpan(entry.Length));

							var blockState = MCJsonConvert.DeserializeObject<BlockStateResource>(json);
							blockState.Name = name;
							blockState.Namespace = nameSpace;

							return blockState;
						}
					});
			}
			catch (Exception ex)
			{
				Log.Warn(ex, $"Could not load {entry.Name}!");
				//	return null;
			}
		}
		
		public bool TryGetBlockState(ResourceLocation modelName, out BlockStateResource stateResource)
		{
			if (_blockStates.TryGetValue(modelName, out var lazy))
			{
				stateResource = lazy.Value;
				return true;
			}

			stateResource = null;
			return false;
		}
		
		#endregion

		#region Models
		
		private ResourcePackModelBase ReadModel(IFile entry, ResourceLocation location)
		{
			try
			{
				using (var stream = entry.Open())
				{
					var model = MCJsonConvert.DeserializeObject<ResourcePackModelBase>(Encoding.UTF8.GetString(stream.ReadToSpan(entry.Length)));
					model.Type = location.Path.Contains("block/", StringComparison.OrdinalIgnoreCase) ?
						ModelType.Block : ModelType.Item;

					return model;
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error loading model: ({location})");

				return null;
			}
		}
		
		#endregion

		#region Localization

		private void LoadLocalization(IFile entry, Match match)
		{
			string name = match.Groups["filename"].Value;
			string nameSpace = match.Groups["namespace"].Value;

			bool isJson = match.Groups["filetype"].Value.Equals("json", StringComparison.OrdinalIgnoreCase);

			using (var stream = entry.Open())
			{
				var text = Encoding.UTF8.GetString(stream.ReadToSpan(entry.Length));
				LanguageResource lang;

				if (isJson)
				{
					var lng = MCJsonConvert.DeserializeObject<IDictionary<string, string>>(text);

					if (!lng.TryGetValue("language.code", out var cultureCode)
					|| !lng.TryGetValue("language.name", out var cultureName)
					|| !lng.TryGetValue("language.region", out var cultureRegion))
					{
						return;
					}
					lang = new LanguageResource(lng)
					{
						CultureCode = cultureCode,
						CultureName = cultureName,
						CultureRegion = cultureRegion
					};
				/*	if (lang.CultureCode == null)
					{
						if (lang.TryGetValue("language.code", out var code))
							lang.CultureCode = code;
					}*/
				}
				else
				{
					lang = LanguageResource.ParseLangFile(text);
				}

				lang.CultureName = name;
				//lang.Namespace   = nameSpace;

				_languageCache[$"{nameSpace}:{name}"] = lang;
			}
		}

		#endregion
		
		public bool TryGetGrassColor(float temp, float rain, int elevation, out Color color)
		{
			color = new Color(94, 157, 52);
			if (GrassColors == null) return false;

			temp = MathHelper.Clamp(temp - elevation * 0.00166667f, 0f, 1f);
			rain = MathHelper.Clamp(rain, 0f, 1f) * temp;

			int x = (int)Math.Floor(MathHelper.Clamp(_grassWidth - (_grassWidth * temp), 0, _grassWidth));
			int y = (int)Math.Floor(MathHelper.Clamp(_grassHeight - (_grassHeight * rain), 0, _grassHeight));

			var indx = _grassWidth * y + x;

			if (indx < 0) indx                      = 0;
			if (indx > GrassColors.Length - 1) indx = GrassColors.Length - 1;
			
			var result = GrassColors[indx];

			color = new Color(result.R, result.G, result.B);
			
			return true;
		}

		public bool TryGetFoliageColor(float temp, float rain, int elevation, out Color color)
		{
			color = new Color(94, 157, 52);
			if (FoliageColors == null) return false;
			temp = MathHelper.Clamp(temp - elevation * 0.00166667f, 0f, 1f);
			rain = MathHelper.Clamp(rain, 0f, 1f) * temp;

			int x = (int)Math.Floor(MathHelper.Clamp(_foliageWidth - (_foliageWidth * temp), 0, _foliageWidth));
			int y = (int)Math.Floor(MathHelper.Clamp(_foliageHeight - (_foliageHeight * rain), 0, _foliageHeight));

			var indx = _foliageWidth * y + x;

			if (indx < 0) indx                        = 0;
			if (indx > FoliageColors.Length - 1) indx = FoliageColors.Length - 1;

			var result = FoliageColors[indx];

			color = new Color(result.R, result.G, result.B);
			return true;
		}


		public void Dispose()
		{
			Filesystem?.Dispose();
			Filesystem = null;
			//_archive?.Dispose();
		}
	}
}
