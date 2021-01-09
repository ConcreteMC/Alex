using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using Alex.API.Utils;
using Alex.ResourcePackLib.Generic;
using Alex.ResourcePackLib.IO;
using Alex.ResourcePackLib.IO.Abstract;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Bedrock;
using NLog;
using NLog.Fluent;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace Alex.ResourcePackLib
{
    public class ResourcePack
    {
	    public delegate void LoadProgress(int percentage, string file);
	    
	    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
	    
	    public LoadProgress         ProgressReporter { get; set; } = null;
	    public ResourcePackManifest Info             { get; protected set; }
		protected ResourcePack()
	    {

	    }

		public static ResourcePackManifest GetManifest(IFilesystem archive)
		{
			//if (archive.GetEntry("pack.mcmeta") != null)

			var entry = archive.GetEntry("pack.mcmeta");

			if (entry != null)
			{
				ResourcePackInfo info;

				using (TextReader reader = new StreamReader(entry.Open()))
				{
					ResourcePackInfoWrapper wrap =
						MCJsonConvert.DeserializeObject<ResourcePackInfoWrapper>(reader.ReadToEnd());

					info = wrap.pack;
				}

				try
				{
					var imgEntry = archive.GetEntry("pack.png");

					if (imgEntry != null && imgEntry.Length > 0)
					{
						// Bitmap bmp = new Bitmap(imgEntry.Open());
						using (var stream = imgEntry.Open())
						{
							//var data = stream.ReadToSpan(entry.Length);
							
							var bmp  = Image.Load(stream).CloneAs<Rgba32>();

							return new ResourcePackManifest(bmp, "", info.Description, ResourcePackType.Java);
						}
					}
				}
				catch (Exception ex)
				{
					Log.Warn(ex, $"Could not read resourcepack logo: {archive.ToString()}");
				}

				return new ResourcePackManifest("", info.Description, ResourcePackType.Java);
			}

			entry = archive.GetEntry("manifest.json");

			if (entry != null)
			{
				//Load bedrock meta
				McPackManifest info;

				using (TextReader reader = new StreamReader(entry.Open()))
				{
					info =
						MCJsonConvert.DeserializeObject<McPackManifest>(reader.ReadToEnd());

					//info = new ResourcePackInfo() {Description = wrap.Header.Description};
					//info = wrap.pack;
				}

				try
				{
					var imgEntry = archive.GetEntry("pack_icon.png");

					if (imgEntry != null && imgEntry.Length > 0)
					{
						// Bitmap bmp = new Bitmap(imgEntry.Open());
						using (var stream = imgEntry.Open())
						{
							//var data = stream.ReadToSpan(entry.Length);
							
							var bmp = Image.Load(stream).CloneAs<Rgba32>();

							return new ResourcePackManifest(bmp, info.Header.Name, info.Header.Description, ResourcePackType.Bedrock);
						}
					}
				}
				catch (Exception ex)
				{
					Log.Warn(ex, $"Could not read resourcepack logo: {archive.ToString()}");
				}

				return new ResourcePackManifest(info.Header.Name, info.Header.Description, ResourcePackType.Bedrock);
			}


			return null;
		}
    }

	public class InvalidResourcePackException : Exception
	{

	}
}
