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

        private bool _optionsLoaded = false;
        public OptionsProvider(IStorageSystem storage)
        {
            _storage = storage;
            AlexOptions = new AlexOptions();

            //Load();
        }

        public void Load()
        {
            if (_optionsLoaded)
                return;
            
            if (_storage.TryReadJson(StorageKey, out AlexOptions options))
            {
                AlexOptions = options;
            }
            else
            {
                Log.Warn($"Could not read from storage.");
            }
            
            _optionsLoaded = true;
        }

        public void Save()
        {
            if (!_optionsLoaded)
                return;
            
            if (!_storage.TryWriteJson(StorageKey, AlexOptions))
            {
                Log.Warn($"Could not save settings.");
            }
        }

        public void ResetAllToDefault()
        {
            AlexOptions.ResetToDefault();
        }
    }
}
