using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using Alex.ResourcePackLib.Generic;
using Alex.ResourcePackLib.Json;

namespace Alex.ResourcePackLib
{
    public class ResourcePack
    {
		public ResourcePackType Type { get; private set; }
		public ResourcePackManifest Info { get; private set; }
		protected ResourcePack()
	    {

	    }

	    private static ResourcePackManifest GetManifest(ZipArchive archive, ResourcePackType type)
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

				    var imgEntry = archive.GetEntry("pack.png");
				    if (imgEntry != null)
				    {
					    Bitmap bmp = new Bitmap(imgEntry.Open());
					    return new ResourcePackManifest(bmp, "", info.Description);
					}
				    else
				    {
					    return new ResourcePackManifest("", info.Description);
					}
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
