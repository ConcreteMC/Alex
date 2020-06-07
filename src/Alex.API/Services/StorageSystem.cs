using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NLog;

namespace Alex.API.Services
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
            Directory.CreateDirectory(Path.Combine(DataDirectory, "assets"));
            Directory.CreateDirectory(Path.Combine(DataDirectory, "assets", "bedrock"));
            Directory.CreateDirectory(Path.Combine(DataDirectory, "assets", "resourcepacks"));
        }

        public bool TryWriteJson<T>(string key, T value)
        {
            var fileName = GetFileName(key) + ".json";

            try
            {
                var json = JsonConvert.SerializeObject(value, Formatting.Indented);

                File.WriteAllText(fileName, json, Encoding.Unicode);

                return true;
            }
            catch (Exception ex)
            {
                Log.Warn($"Could not write to storage! {ex.ToString()}");
                return false;
            }
        }

        public bool TryReadJson<T>(string key, out T value)
        {
            var fileName = GetFileName(key) + ".json";

            if (!File.Exists(fileName))
            {
                value = default(T);
                return false;
            }

            try
            {
                var json = File.ReadAllText(fileName, Encoding.Unicode);

                value = JsonConvert.DeserializeObject<T>(json);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn($"Failed to read: {ex.ToString()}");
                value = default(T);
                return false;
            }
        }

        public bool TryWriteBytes(string key, byte[] value)
        {
            var fileName = Path.Combine(DataDirectory, key);

            try
            {
                File.WriteAllBytes(fileName, value);
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
                value = File.ReadAllBytes(fileName);
                return true;
            }
            catch (Exception ex)
            {
                value = null;
                return false;
            }
        }

        public bool TryWriteString(string key, string value)
        {
            var fileName = Path.Combine(DataDirectory, key);

            try
            {
                File.WriteAllText(fileName, value, Encoding.Unicode);
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
            var fileName = Path.Combine(DataDirectory, key);

            if (!File.Exists(fileName))
            {
                value = null;
                return false;
            }

            try
            {
                value = File.ReadAllText(fileName, Encoding.Unicode);
                return true;
            }
            catch (Exception ex)
            {
                value = null;
                return false;
            }
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

        private string GetFileName(string key)
        {
            return Path.Combine(DataDirectory, FileKeySanitizeRegex.Replace(key.ToLowerInvariant(), ""));
        }
    }
}