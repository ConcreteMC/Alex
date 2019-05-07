using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Data.Options;
using Alex.API.Services;

namespace Alex.Services
{
    public class OptionsProvider : IOptionsProvider
    {
        private const string StorageKey = "SavedServers";

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
        }

        public void Save()
        {
            if (_storage.TryWrite(StorageKey, AlexOptions))
            {
            }
        }

        public void ResetAllToDefault()
        {
            AlexOptions.ResetToDefault();
        }
    }
}
