using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Alex.API.Services
{
    public interface IStorageSystem
    {
        bool TryWrite<T>(string key, T value);
        bool TryRead<T>(string key, out T value);

	    bool TryWriteBytes(string key, byte[] value);
	    bool TryReadBytes(string key, out byte[] value);

	    bool TryGetDirectory(string key, out DirectoryInfo info);
    }
}
