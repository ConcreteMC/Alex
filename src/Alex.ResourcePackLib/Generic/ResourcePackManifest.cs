using System.Drawing;
using System.IO;
using System.Text;
using Alex.ResourcePackLib.Properties;

namespace Alex.ResourcePackLib.Generic
{
    public sealed class ResourcePackManifest
    {
	    private static Bitmap UnknownPack = null;

	    static ResourcePackManifest()
	    {
		    if (UnknownPack == null)
		    {
			    using (MemoryStream ms = new MemoryStream(Encoding.Default.GetBytes(Resources.unknown_pack)))
			    {
				    UnknownPack = new Bitmap(ms);
			    }
		    }
	    }

	    public string Name { get; }
		public string Description { get; }
		public Bitmap Icon { get; }

	    internal ResourcePackManifest(Bitmap icon, string name, string description)
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
