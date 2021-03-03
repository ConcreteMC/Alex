using System.Collections.Generic;
using System.Linq;
using Alex.API.Services;

namespace Alex.API.Data.Servers
{
    public class SavedServerDataProvider : IListStorageProvider<SavedServerEntry>
    {
        private const string StorageKey = "SavedServers";

        public IReadOnlyCollection<SavedServerEntry> Data => _data;

        private readonly List<SavedServerEntry> _data = new List<SavedServerEntry>();

        private readonly IStorageSystem _storage;

        public SavedServerDataProvider(IStorageSystem storage)
        {
            _storage = storage;

            Load();
        }

        public void Load()
        {
            if (_storage.TryReadJson(StorageKey, out SavedServerEntry[] newEntries))
            {
                _data.Clear();
                _data.AddRange(newEntries);

                UpdateIndexes();
            }
        }

        public void Save()
        {
            _storage.TryWriteJson(StorageKey, Data.ToArray());
        }

        void IDataProvider<IReadOnlyCollection<SavedServerEntry>>.Save(IReadOnlyCollection<SavedServerEntry> entries)
        {
            _storage.TryWriteJson(StorageKey, Data.ToArray());
        }

        public void MoveEntry(int index, SavedServerEntry entry)
        {
            _data.Remove(entry);

            entry.ListIndex = index;

            _data.Insert(index, entry);

            UpdateIndexes();

            Save();
        }

        private void UpdateIndexes()
        {
            for (var index = 0; index < _data.Count; index++)
            {
               _data[index].ListIndex = index;
            }
        }

        public void AddEntry(SavedServerEntry entry)
        {
            _data.Add(entry);

            Save();
        }

        public bool RemoveEntry(SavedServerEntry entry)
        {
            var newEntry = _data.FirstOrDefault(x => x.InternalIdentifier.Equals(entry.InternalIdentifier));
            if (newEntry == null || !_data.Remove(newEntry))
                return false;
            
            UpdateIndexes();
            
            Save();

            return true;
        }
    }
}
