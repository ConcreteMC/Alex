using System.Drawing;
using System.IO;
using System.Text;
using Alex.API.Utils;
using Alex.ResourcePackLib.Properties;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace Alex.ResourcePackLib.Generic
{
    public sealed class ResourcePackManifest
    {
	    private static readonly Image<Rgba32> UnknownPack = null;

	    static ResourcePackManifest()
	    {
		    if (UnknownPack == null)
		    {
				UnknownPack = Image.Load(EmbeddedResourceUtils.GetApiRequestFile("Alex.ResourcePackLib.Resources.unknown_pack.png"));
		    }
	    }

	    public string Name { get; set; }
		public string Description { get; }
		public Image<Rgba32> Icon { get; }

	    internal ResourcePackManifest(Image<Rgba32> icon, string name, string description)
	    {
		    Icon = icon;
		    Name = name;
		    Description = description;
	    }

	    internal ResourcePackManifest(string name, string description) : this(UnknownPack, name, description)
	    {

	    }
    }
}
