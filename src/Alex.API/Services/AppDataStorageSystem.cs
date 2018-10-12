using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Alex.API.Services
{
    public class AppDataStorageSystem : IStorageSystem
    {
        private static readonly Regex FileKeySanitizeRegex = new Regex(@"[\W]", RegexOptions.Compiled);

        private string AppDataDirectory { get; }

        public AppDataStorageSystem()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
            AppDataDirectory = Path.Combine(appData, "Alex");

			Directory.CreateDirectory(AppDataDirectory);
	        Directory.CreateDirectory(Path.Combine(AppDataDirectory, "assets"));
		}
        
        public bool TryWrite<T>(string key, T value)
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
                return false;
            }
        }

        public bool TryRead<T>(string key, out T value)
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
                value = default(T);
                return false;
            }
        }

	    public bool TryWriteBytes(string key, byte[] value)
	    {
			var fileName = Path.Combine(AppDataDirectory, key);

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
		    var fileName = Path.Combine(AppDataDirectory, key);

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

	    private string GetFileName(string key)
        {
            return Path.Combine(AppDataDirectory, FileKeySanitizeRegex.Replace(key.ToLowerInvariant(), ""));
        }
    }
}
