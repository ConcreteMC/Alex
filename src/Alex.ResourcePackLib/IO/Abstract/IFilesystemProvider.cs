using System;
using System.Collections.Generic;

namespace Alex.ResourcePackLib.IO.Abstract
{
	public interface IFilesystem : IDisposable
	{
		string                     Name         { get; }
		IReadOnlyCollection<IFile> Entries      { get; }

		IFile GetEntry(string name);
	}
}