using System.IO;
using System.IO.Compression;
using Alex.ResourcePackLib.IO.Abstract;

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

	    public static string ReadAsString(this IFile entry)
	    {
		    using (TextReader reader = new StreamReader(entry.Open()))
		    {
			    return reader.ReadToEnd();
		    }
	    }

	    public static Stream OpenEncoded(this IFile entry, string contentKey)
	    {
		    if (contentKey == null)
			    return entry.Open();

		    //TODO: Decrypt.
		    return entry.Open();
	    }
	    
	    public static string ReadAsEncodedString(this IFile entry, string contentKey)
	    {
		    using (TextReader reader = new StreamReader(entry.OpenEncoded(contentKey)))
		    {
			    return reader.ReadToEnd();
		    }
	    }
    }
}
