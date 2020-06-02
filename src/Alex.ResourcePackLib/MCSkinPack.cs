using System.Collections.Generic;
using System.IO.Compression;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Bedrock;
using Alex.ResourcePackLib.Json.Models.Entities;
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

		internal virtual void Load()
		{
			
		}
	}
	
	public class MCSkinPack : MCPackModule
	{
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
		internal override void Load()
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
						img = Image.Load<Rgba32>(s, PngDecoder);
					}
					
					LoadedSkin loaded = new LoadedSkin(skin.LocalizationName, model, img);
					skins.Add(loaded);
					//skin.
				}
			}

			Skins = skins.ToArray();
		}
	}

	public class LoadedSkin
	{
		public string Name { get; }
		public EntityModel Model { get; }
		public Image<Rgba32> Texture { get; }

		public LoadedSkin(string name, EntityModel model, Image<Rgba32> texture)
		{
			Name = name;
			Model = model;
			Texture = texture;
		}
	}
}