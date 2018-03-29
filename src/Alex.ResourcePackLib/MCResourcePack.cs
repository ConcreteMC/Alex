using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using Alex.API.World;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Microsoft.Xna.Framework;
using NLog;
using Color = Microsoft.Xna.Framework.Color;

namespace Alex.ResourcePackLib
{
	public class McResourcePack : IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(McResourcePack));
		public ResourcePackInfo Info { get; private set; }

		public IReadOnlyDictionary<string, BlockStateResource> BlockStates => _blockStates;
		public IReadOnlyDictionary<string, BlockModel> BlockModels => _blockModels;

		private ZipArchive _archive;
		private readonly Dictionary<string, BlockStateResource> _blockStates = new Dictionary<string, BlockStateResource>();
		private readonly Dictionary<string, BlockModel> _blockModels = new Dictionary<string, BlockModel>();
		private readonly Dictionary<string, Bitmap> _textureCache = new Dictionary<string, Bitmap>();
		private Color[] FoliageColors { get; set; } = null;
		private int _foliageWidth = 256;
		private int _foliageHeight = 256;

		private Color[] GrassColors { get; set; } = null;
		private int _grassHeight = 256;
		private int _grassWidth = 256;

		public McResourcePack(byte[] resourcePackData) : this(new ZipArchive(new MemoryStream(resourcePackData), ZipArchiveMode.Read, false))
		{

		}

		public McResourcePack(ZipArchive archive)
		{
			_archive = archive;
			Load();
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

		public bool TryGetBitmap(string resource, out Bitmap texture)
		{
			try
			{
				var entry = _archive.GetEntry(resource);
				if (entry == null)
				{
					texture = default(Bitmap);
					return false;
				}
				using(var e = entry.Open()){
					Bitmap bmp = new Bitmap(e);
					texture = bmp;
				}
				return true;
			}
			catch(Exception exception)
			{
				Log.Error("Oh oh!", exception);
				texture = default(Bitmap);
				return false;
			}
		} 

		private void Load()
		{
			LoadMeta();
			LoadBlockModels();
			LoadBlockStates();
			LoadColormap();
		}

		private void LoadColormap()
		{
			const string colormapPath = "assets/minecraft/textures/colormap";
			var foliageEntry = _archive.GetEntry($"{colormapPath}/foliage.png");
			var grassEntry = _archive.GetEntry($"{colormapPath}/grass.png");

			if (foliageEntry != null)
			{
				var foliageColors = new LockBitmap(new Bitmap(foliageEntry.Open()));
				foliageColors.LockBits();
				FoliageColors = foliageColors.GetColorArray();
				foliageColors.UnlockBits();

				_foliageHeight = foliageColors.Height;
				_foliageWidth = foliageColors.Width;
			}

			if (grassEntry != null)
			{
				var grassColors = new LockBitmap(new Bitmap(grassEntry.Open()));
				grassColors.LockBits();
				GrassColors = grassColors.GetColorArray();
				grassColors.UnlockBits();

				_grassWidth = grassColors.Width;
				_grassHeight = grassColors.Height;
			}
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
			string nameSpace = entry.FullName.Split('/')[1];
			string name = Path.GetFileNameWithoutExtension(entry.FullName);
			using (var r = new StreamReader(entry.Open()))
			{
				var blockModel = MCJsonConvert.DeserializeObject<BlockModel>(r.ReadToEnd());
				blockModel.Name = name;
				blockModel.Namespace = nameSpace;

				blockModel = ProcessBlockModel(blockModel);
				_blockModels[$"{nameSpace}:{name}"] = blockModel;

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
				Log.Debug("Failed to load Block Model: File Not Found (" + "assets/minecraft/models/block/" + modelName + ".json)");
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

		private BlockStateResource LoadBlockState(ZipArchiveEntry entry)
		{
			try
			{
				string name = Path.GetFileNameWithoutExtension(entry.FullName);
				string nameSpace = entry.FullName.Split('/')[1];

				using (var r = new StreamReader(entry.Open()))
				{
					var json = r.ReadToEnd();

					var blockState = MCJsonConvert.DeserializeObject<BlockStateResource>(json);
					blockState.Name = name;
					blockState.Namespace = nameSpace;
					_blockStates[$"{nameSpace}:{name}"] = ProcessBlockState(blockState);

					return blockState;
				}
			}
			catch (Exception ex)
			{
				Log.Warn($"Could not load {entry.Name}!", ex);
				return null;
			}
		}

		public bool TryGetBlockState(string modelName, out BlockStateResource stateResource)
		{
			if (_blockStates.TryGetValue(modelName, out stateResource))
				return true;

			stateResource = null;

			var modelFile =
				_archive.Entries.FirstOrDefault(e => e.FullName.Equals($"assets/minecraft/blockstates/{modelName}.json"));

			if (modelFile == null)
			{
				Log.Debug("Failed to load BlockStateResource: File Not Found (" + "assets/minecraft/blockstates/" + modelName + ".json)");
				return false;
			}

			stateResource = LoadBlockState(modelFile);
			return true;
		}

		public bool TryGetStream(string filePath, out Stream stream)
		{
			stream = null;
			var textureFile =
				_archive.Entries.FirstOrDefault(e => e.FullName.Replace('/', '\\').Equals(filePath.Replace('/', '\\'), StringComparison.InvariantCultureIgnoreCase));
			if (textureFile == null) return false;

			using (var fileStream = textureFile.Open())
			{
				stream = new MemoryStream();
				fileStream.CopyTo(stream);
			}
			return true;
		}

		public bool TryGetJson<TValue>(string filePath, out TValue value)
		{
			if (TryGetJson(filePath, out var json))
			{
				value = MCJsonConvert.DeserializeObject<TValue>(json);
				return true;
			}

			value = default(TValue);
			return false;
		}

		public bool TryGetJson(string filePath, out string json)
		{
			if (TryGetStream(filePath + ".json", out var stream))
			{
				using (var sr = new StreamReader(stream))
				{
					json = sr.ReadToEnd();
				}

				return true;
			}

			json = null;
			return false;
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

		private BlockStateResource ProcessBlockState(BlockStateResource blockStateResource)
		{
			if (blockStateResource.Parts.Length > 0)
			{
				foreach (var part in blockStateResource.Parts)
				{
					foreach (var sVariant in part.Apply)
					{
						if (!TryGetBlockModel("block/" + sVariant.ModelName, out BlockModel model))
						{
							Log.Debug($"Could not get multipart blockmodel! Variant: {blockStateResource} Model: {sVariant.ModelName}");
							continue;
						}

						sVariant.Model = model;
						//part.Apply = model;
						//var apply = MCJsonConvert.DeserializeObject<BlockStateModel>(part.ApplyModel);
						//part.Apply = apply;
					}
				}
			}
			else
			{
				foreach (var variant in blockStateResource.Variants)
				{
					foreach (var sVariant in variant.Value)
					{
						if (!TryGetBlockModel("block/" + sVariant.ModelName, out BlockModel model))
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

		public void Dispose()
		{
			_archive?.Dispose();
		}
	}

	internal class LockBitmap
	{
		Bitmap _source = null;
		IntPtr _iptr = IntPtr.Zero;
		BitmapData _bitmapData = null;

		public byte[] Pixels { get; set; }
		public int Depth { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }

		public LockBitmap(Bitmap source)
		{
			this._source = source;
		}

		/// <summary>
		/// Lock bitmap data
		/// </summary>
		public void LockBits()
		{
			try
			{
				// Get width and height of bitmap
				Width = _source.Width;
				Height = _source.Height;

				// get total locked pixels count
				int pixelCount = Width * Height;

				// Create rectangle to lock
				System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, Width, Height);

				// get source bitmap pixel format size
				Depth = System.Drawing.Bitmap.GetPixelFormatSize(_source.PixelFormat);

				// Check if bpp (Bits Per Pixel) is 8, 24, or 32
				if (Depth != 8 && Depth != 24 && Depth != 32)
				{
					throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
				}

				// Lock bitmap and return bitmap data
				_bitmapData = _source.LockBits(rect, ImageLockMode.ReadWrite,
											 _source.PixelFormat);

				// create byte array to copy pixel values
				int step = Depth / 8;
				Pixels = new byte[pixelCount * step];
				_iptr = _bitmapData.Scan0;

				// Copy data from pointer to array
				Marshal.Copy(_iptr, Pixels, 0, Pixels.Length);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Unlock bitmap data
		/// </summary>
		public void UnlockBits()
		{
			try
			{
				// Copy data from byte array to pointer
				Marshal.Copy(Pixels, 0, _iptr, Pixels.Length);

				// Unlock bitmap data
				_source.UnlockBits(_bitmapData);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Get the color of the specified pixel
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public Color GetPixel(int x, int y)
		{
			Color clr = Color.White;

			// Get color components count
			int cCount = Depth / 8;

			// Get start index of the specified pixel
			int i = ((y * Width) + x) * cCount;

			if (i > Pixels.Length - cCount)
				throw new IndexOutOfRangeException();

			if (Depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
			{
				byte b = Pixels[i];
				byte g = Pixels[i + 1];
				byte r = Pixels[i + 2];
				byte a = Pixels[i + 3]; // a
				clr = new Color(r,g,b,a);
			}
			if (Depth == 24) // For 24 bpp get Red, Green and Blue
			{
				byte b = Pixels[i];
				byte g = Pixels[i + 1];
				byte r = Pixels[i + 2];
				clr =  new Color(r, g, b);
			}
			if (Depth == 8)
			// For 8 bpp get color value (Red, Green and Blue values are the same)
			{
				byte c = Pixels[i];
				clr = new Color(c, c, c);
			}
			return clr;
		}

		/// <summary>
		/// Set the color of the specified pixel
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="color"></param>
		public void SetPixel(int x, int y, Color color)
		{
			// Get color components count
			int cCount = Depth / 8;

			// Get start index of the specified pixel
			int i = ((y * Width) + x) * cCount;

			if (Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
			{
				Pixels[i] = color.B;
				Pixels[i + 1] = color.G;
				Pixels[i + 2] = color.R;
				Pixels[i + 3] = color.A;
			}
			if (Depth == 24) // For 24 bpp set Red, Green and Blue
			{
				Pixels[i] = color.B;
				Pixels[i + 1] = color.G;
				Pixels[i + 2] = color.R;
			}
			if (Depth == 8)
			// For 8 bpp set color value (Red, Green and Blue values are the same)
			{
				Pixels[i] = color.B;
			}
		}

		public Color[] GetColorArray()
		{
			Color[] colors = new Color[Width * Height];
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					var indx = Width * y + x;
					colors[indx] = GetPixel(x, y);
				}
			}

			return colors;
		}
	}
}
