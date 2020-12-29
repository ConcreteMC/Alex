using System.IO;
using System.Text;

namespace Alex.API.Services
{
    public interface IStorageSystem
    {
        #region Json
        
        bool TryWriteJson<T>(string key, T value);
        bool TryReadJson<T>(string key, out T value);

        bool TryReadJson<T>(string key, out T value, Encoding encoding);

        #endregion

        #region Bytes
        
	    bool TryWriteBytes(string key, byte[] value);
	    bool TryReadBytes(string key, out byte[] value);
        
        #endregion

        #region String
        
        bool TryWriteString(string key, string value);
        bool TryWriteString(string key, string value, Encoding encoding);
        
        bool TryReadString(string key, out string value);
        bool TryReadString(string key, out string value, Encoding encoding);
        
        #endregion

        #region Directory

        bool TryGetDirectory(string key, out DirectoryInfo info);
        bool TryCreateDirectory(string key);
        bool TryDeleteDirectory(string key);
        
        #endregion

        bool Exists(string key);
        bool Delete(string key);

        FileStream OpenFileStream(string key, FileMode access);
    }
}
