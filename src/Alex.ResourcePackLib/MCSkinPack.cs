using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Alex.API.Utils;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Bedrock;
using Alex.ResourcePackLib.Json.Models.Entities;
using NLog;
using NLog.Fluent;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Alex.ResourcePackLib
{
	public class MCPackModule
	{
		public virtual string Name
		{
			get
			{
				return Entry.Name;
			}
		}

		protected ZipArchiveEntry Entry { get; }
		protected MCPackModule(ZipArchiveEntry entry)
		{
			Entry = entry;
		}

		internal virtual bool Load()
		{
			return false;
		}
	}
	
	public class MCSkinPack : MCPackModule
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
		
		private static PngDecoder PngDecoder { get; } = new PngDecoder()
		{
			IgnoreMetadata = true
		};

		/// <inheritdoc />
		public override string Name {
			get
			{
				return Info?.LocalizationName ?? "Unknown";
			}
		} 
			

		public MCPackSkins Info { get; private set; }
		public LoadedSkin[] Skins { get; private set; }
		
		/// <inheritdoc />
		internal MCSkinPack(ZipArchiveEntry entry) : base(entry)
		{
			
		}
		
		/// <inheritdoc />
		internal override bool Load()
		{
			try
			{
				List<LoadedSkin> skins = new List<LoadedSkin>();

				using (var archive = new ZipArchive(Entry.Open(), ZipArchiveMode.Read))
				{
					var skinsEntry = archive.GetEntry("skins.json");
					Info = MCJsonConvert.DeserializeObject<MCPackSkins>(skinsEntry.ReadAsString());

					var geometryEntry = archive.GetEntry("geometry.json");

					Dictionary<string, EntityModel> models =
						MCJsonConvert.DeserializeObject<Dictionary<string, EntityModel>>(geometryEntry.ReadAsString());

					foreach (var skin in Info.Skins)
					{
						EntityModel model;

						if (!models.TryGetValue(skin.Geometry, out model))
							continue;

						var textureEntry = archive.GetEntry(skin.Texture);

						if (textureEntry == null)
							continue;

						Image<Rgba32> img;

						using (var s = textureEntry.Open())
						{
							//img = new Bitmap(s);
							img = Image.Load<Rgba32>(s.ReadToSpan(textureEntry.Length), PngDecoder);
						}

						LoadedSkin loaded = new LoadedSkin(skin.LocalizationName, model, img);
						skins.Add(loaded);
						//skin.
					}
				}

				Skins = skins.ToArray();

				return true;
			}
			catch (InvalidDataException ex)
			{
				Log.Debug(ex, $"Could not load module. IsFile={Entry.IsFile()} IsDirectory={Entry.IsDirectory()}");
			}

			return false;
		}
	}

	public class LoadedSkin
	{
		public string        Name    { get; }
		public EntityModel   Model   { get; }
		public Image<Rgba32> Texture { get; }

		public LoadedSkin(string name, EntityModel model, Image<Rgba32> texture)
		{
			Name = name;
			Model = model;
			Texture = texture;
		}
	}
}