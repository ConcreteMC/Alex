using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Alex.Common.Utils;
using Newtonsoft.Json;
using NLog;

namespace Alex.Common.Services
{
    public class StorageSystem : IStorageSystem
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(StorageSystem));
        private static readonly Regex FileKeySanitizeRegex = new Regex(@"[\W]", RegexOptions.Compiled);

        private string DataDirectory { get; }

        public StorageSystem(string directory)
        {
            DataDirectory = directory;

            Directory.CreateDirectory(DataDirectory);
        }

        private bool EncryptionEnabled { get; set; } = false;
        private byte[] EncryptionKey { get; set; } = null;

        private ICryptoTransform _encryptor = null;
        private ICryptoTransform _decryptor = null;
        /// <inheritdoc />
        public void EnableEncryption(byte[] key)
        {
            if (EncryptionEnabled)
                return;
            
            EncryptionEnabled = true;
            EncryptionKey = key;

            using (var aesAlg = Aes.Create())
            {
                _encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV);
                _decryptor = aesAlg.CreateDecryptor(key, aesAlg.IV);
            }
        }

        public bool TryWriteJson<T>(string key, T value)
        {
            try
            {
                var json = JsonConvert.SerializeObject(value, Formatting.Indented);
                return TryWriteString($"{key}.json", json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Log.Warn($"Could not to file: {key} {ex.ToString()}");
                return false;
            }
        }

        public bool TryReadJson<T>(string key, out T value)
        {
            return TryReadJson<T>(key, out value, Encoding.UTF8);
        }
        
        public bool TryReadJson<T>(string key, out T value, Encoding encoding)
        {
            if (!TryReadString($"{key}.json", out string json, encoding))
            {
                value = default(T);
                return false;
            }

            try
            {
                value = JsonConvert.DeserializeObject<T>(json);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn($"Failed to read file ({key}): {ex.ToString()}");
                value = default(T);
                return false;
            }
        }

        public bool TryWriteBytes(string key, byte[] value)
        {
            var fileName = Path.Combine(DataDirectory, key);

            try
            {
                using (var fs = OpenFileStream(fileName, FileMode.Create))
                {
                    fs.Write(value);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool TryReadBytes(string key, out byte[] value)
        {
            var fileName = Path.Combine(DataDirectory, key);

            if (!File.Exists(fileName))
            {
                value = null;
                return false;
            }

            try
            {
                using (var fs = OpenFileStream(fileName, FileMode.Open))
                { 
                    value = fs.ReadToEnd().ToArray();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                value = null;
                return false;
            }
        }

        /// <inheritdoc />
        public Stream OpenFileStream(string key, FileMode mode)
        {
            var fileName = Path.Combine(DataDirectory, key);

            bool isWriting = false;
            FileAccess access = FileAccess.Read;
            FileShare fileShare = FileShare.Read;
            switch (mode)
            {
                case FileMode.CreateNew:
                    access = FileAccess.Write;
                    fileShare = FileShare.None;
                    isWriting = true;
                    break;
                case FileMode.Append:
                case FileMode.Create:
                    access = FileAccess.ReadWrite;
                    fileShare = FileShare.None;
                    isWriting = true;
                    break;
            }
            
            Stream fs = new FileStream(fileName, mode, access, fileShare);

            if (EncryptionEnabled)
            {
                if (mode == FileMode.Open)
                {
                    fs = new CryptoStream(fs, _decryptor, CryptoStreamMode.Read);
                }
                else if (mode == FileMode.Create)
                {
                    fs = new CryptoStream(fs, _encryptor, CryptoStreamMode.Write);
                }
            }
            
            return fs;
        }

        public bool TryWriteString(string key, string value)
        {
            return TryWriteString(key, value, Encoding.Unicode);
        }

        /// <inheritdoc />
        public bool TryWriteString(string key, string value, Encoding encoding)
        {
            var fileName = GetFileName(key);

            try
            {
                using (var fs = OpenFileStream(fileName, FileMode.Create))
                { 
                    fs.Write(encoding.GetBytes(value));
                }
                
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool TryReadString(string key, Encoding encoding, out string value)
        {
            var fileName = GetFileName(key);

            if (!File.Exists(fileName))
            {
                value = null;
                return false;
            }

            try
            {
                using (var fs = OpenFileStream(fileName, FileMode.Open))
                {
                    value = encoding.GetString(fs.ReadToEnd());
                }
                
                return true;
            }
            catch (Exception ex)
            {
                value = null;
                return false;
            }
        }

        public bool TryReadString(string key, out string value)
        {
            return TryReadString(key, out value, Encoding.Unicode);
        }

        public bool TryReadString(string key, out string value, Encoding encoding)
        {
            return TryReadString(key, encoding, out value);
        }

        public bool Exists(string key)
        {
            return File.Exists(Path.Combine(DataDirectory, key));
        }
        
        public bool Delete(string key)
        {
            try
            {
                File.Delete(Path.Combine(DataDirectory, key));
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public bool TryGetDirectory(string key, out DirectoryInfo info)
        {
            var path = Path.Combine(DataDirectory, key);
            if (Directory.Exists(path))
            {
                info = new DirectoryInfo(path);
                return true;
            }

            info = default(DirectoryInfo);
            return false;
        }

        public bool TryCreateDirectory(string key)
        {
            var path = Path.Combine(DataDirectory, key);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public bool TryDeleteDirectory(string key)
        {
            var path = Path.Combine(DataDirectory, key);

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);

                return true;
            }

            return false;
        }
        
        private string GetFileName(string key)
        {
            return Path.Combine(DataDirectory, key.ToLowerInvariant());
        }

        public IStorageSystem Open(params string[] path)
        {
            var subpath = Path.Combine(path);

            return new StorageSystem(Path.Combine(DataDirectory, subpath))
            {
                EncryptionEnabled = EncryptionEnabled,
                EncryptionKey = EncryptionKey
            };
        }
    }
}