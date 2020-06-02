using System.IO;
using System.IO.Compression;

namespace Alex.ResourcePackLib
{
    internal static class ZipExtensions
    {
	    public static bool IsFile(this ZipArchiveEntry entry)
	    {
		    return !entry.FullName.EndsWith("/");
	    }

	    public static bool IsDirectory(this ZipArchiveEntry entry)
	    {
		    return entry.FullName.EndsWith("/");
	    }

	    public static string ReadAsString(this ZipArchiveEntry entry)
	    {
		    using (TextReader reader = new StreamReader(entry.Open()))
		    {
			    return reader.ReadToEnd();
		    }
	    }
    }
}
