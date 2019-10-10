using System;
using Alex.API.Data.Options;
using Alex.API.Services;
using Newtonsoft.Json;
using NLog;

namespace Alex.Services
{
    public class OptionsProvider : IOptionsProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(OptionsProvider));
        
        private const string StorageKey = "gamesettings";

        public AlexOptions AlexOptions { get; private set; }

        private readonly IStorageSystem _storage;

        public OptionsProvider(IStorageSystem storage)
        {
            _storage = storage;
            AlexOptions = new AlexOptions();

            Load();
        }

        public void Load()
        {
            if (_storage.TryRead(StorageKey, out AlexOptions options))
            {
                AlexOptions = options;
            }
            else
            {
                Log.Warn($"Could not read from storage.");
            }
        }

        public void Save()
        {
            if (!_storage.TryWrite(StorageKey, AlexOptions))
            {
                
            }
        }

        public void ResetAllToDefault()
        {
            AlexOptions.ResetToDefault();
        }
    }
}
