using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.API.Services;

namespace Alex.API.Data.Servers
{
    public class SavedServerDataProvider : IListStorageProvider<SavedServerEntry>
    {
        private const string StorageKey = "SavedServers";

        public IReadOnlyCollection<SavedServerEntry> Entries => _entries;

        private readonly List<SavedServerEntry> _entries = new List<SavedServerEntry>();

        private readonly IStorageSystem _storage;

        public SavedServerDataProvider(IStorageSystem storage)
        {
            _storage = storage;

            Load();
        }

        public void Load()
        {
            if (_storage.TryRead(StorageKey, out SavedServerEntry[] newEntries))
            {
                _entries.Clear();
                _entries.AddRange(newEntries);

                UpdateIndexes();
            }
        }

        public void Save()
        {
            _storage.TryWrite(StorageKey, Entries.ToArray());
        }

        public void MoveEntry(int index, SavedServerEntry entry)
        {
            _entries.Remove(entry);

            entry.ListIndex = index;

            _entries.Insert(index, entry);

            UpdateIndexes();

            Save();
        }

        private void UpdateIndexes()
        {
            for (var index = 0; index < _entries.Count; index++)
            {
               _entries[index].ListIndex = index;
            }
        }

        public void AddEntry(SavedServerEntry entry)
        {
            _entries.Add(entry);

            Save();
        }

        public void RemoveEntry(SavedServerEntry entry)
        {
            _entries.Remove(entry);
            
            Save();
        }
    }
}
