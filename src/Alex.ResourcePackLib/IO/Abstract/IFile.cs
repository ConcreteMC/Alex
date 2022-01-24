using System.IO;

namespace Alex.ResourcePackLib.IO.Abstract
{
	public interface IFile
	{
		string FullName { get; }
		string Name { get; }
		long Length { get; }

		Stream Open();
	}
}