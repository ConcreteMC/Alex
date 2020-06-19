using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using Alex.API.Utils;
using Alex.ResourcePackLib.Generic;
using Alex.ResourcePackLib.Json;
using NLog;
using NLog.Fluent;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace Alex.ResourcePackLib
{
    public class ResourcePack
    {
	    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
	    
		public ResourcePackType Type { get; private set; }
		public ResourcePackManifest Info { get; private set; }
		protected ResourcePack()
	    {

	    }

	    protected static ResourcePackManifest GetManifest(ZipArchive archive, ResourcePackType type)
	    {
		    if (type == ResourcePackType.Java)
		    {
			    var entry = archive.GetEntry("pack.mcmeta");
			    if (entry != null)
			    {
				    ResourcePackInfo info;
				    using (TextReader reader = new StreamReader(entry.Open()))
				    {
					    ResourcePackInfoWrapper wrap = MCJsonConvert.DeserializeObject<ResourcePackInfoWrapper>(reader.ReadToEnd());
					    info = wrap.pack;
				    }

				    try
				    {
					    var imgEntry = archive.GetEntry("pack.png");
					    if (imgEntry != null)
					    {
						    // Bitmap bmp = new Bitmap(imgEntry.Open());
						    using (var stream = imgEntry.Open())
						    {
							    var bmp = Image.Load<Rgba32>(stream.ReadToSpan(entry.Length));
							    return new ResourcePackManifest(bmp, "", info.Description);
						    }
					    }
				    }
				    catch (Exception ex)
				    {
					    Log.Warn(ex, $"Could not read resourcepack logo: {archive.ToString()}");
				    }

				    return new ResourcePackManifest("", info.Description);
			    }
			    else
			    {
					throw new InvalidResourcePackException();
			    }
			}
			else if (type == ResourcePackType.Bedrock)
		    {

		    }

		    return null;
	    }

		public static ResourcePack LoadFromArchive(ZipArchive archive)
	    {
			ResourcePack pack = new ResourcePack();
		    if (archive.GetEntry("pack.mcmeta") != null)
		    {
			    pack.Type = ResourcePackType.Java;
			}
		    else if (archive.GetEntry("manifest.json") != null)
		    {
			    pack.Type = ResourcePackType.Bedrock;
			}
		    else
		    {
			    throw new InvalidResourcePackException();
		    }

		    pack.Info = GetManifest(archive, pack.Type);

		    return pack;
	    }
    }

	public class InvalidResourcePackException : Exception
	{

	}
}
