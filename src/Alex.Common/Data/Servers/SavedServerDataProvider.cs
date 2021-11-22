using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Alex.Common.Services;
using Alex.Common.Utils.Collections;

namespace Alex.Common.Data.Servers
{
    public class SavedServerDataProvider : IListStorageProvider<SavedServerEntry>
    {
        private string StorageKey { get; }

        public IReadOnlyCollection<SavedServerEntry> Data => _data;

        private readonly ObservableCollection<SavedServerEntry> _data;

        private readonly IStorageSystem _storage;

        public SavedServerDataProvider(IStorageSystem storage) : this(storage, "SavedServers")
        {
            
        }
        
        public SavedServerDataProvider(IStorageSystem storage, string key)
        {
            StorageKey = key;
            _storage = storage;
            
            _data = new ObservableCollection<SavedServerEntry>();
            _data.CollectionChanged += DataOnCollectionChanged;
            
            Load();
        }

        private void DataOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Save();
        }

        private object _loadingLock = new object();
        public void Load()
        {
            lock (_loadingLock)
            {
                _data.CollectionChanged -= DataOnCollectionChanged;

                try
                {
                    if (_storage.TryReadJson(StorageKey, out SavedServerEntry[] newEntries))
                    {
                        _data.Clear();
                        _data.AddRange(newEntries);
                    }
                }
                finally
                {
                    _data.CollectionChanged += DataOnCollectionChanged;
                }
            }
        }

        public void Save()
        {
            lock (_loadingLock)
            {
                _storage.TryWriteJson(StorageKey, Data.ToArray());
            }
        }

        private int GetIndexOf(SavedServerEntry entry)
        {
            var newEntry = _data.FirstOrDefault(x => x.InternalIdentifier.Equals(entry.InternalIdentifier));
            return _data.IndexOf(newEntry);
        }

        public bool MoveUp(SavedServerEntry entry)
        {
            lock (_loadingLock)
            {
                var currentIndex = GetIndexOf(entry);

                if (currentIndex == -1 || currentIndex == 0)
                    return false;

                _data.Move(currentIndex, currentIndex - 1);

                return true;
            }
        }

        public bool MoveDown(SavedServerEntry entry)
        {
            lock (_loadingLock)
            {
                var currentIndex = GetIndexOf(entry);

                if (currentIndex == -1 || currentIndex == _data.Count - 1)
                    return false;

                _data.Move(currentIndex, currentIndex + 1);

                return true;

            }
        }
        
        public void MoveEntry(int index, SavedServerEntry entry)
        {
            lock (_loadingLock)
            {
                var oldIndex = GetIndexOf(entry);

                if (oldIndex == -1)
                    return;

                _data.Move(oldIndex, index);
            }
        }

        public void AddEntry(SavedServerEntry entry)
        {
            lock (_loadingLock)
            {
                _data.Add(entry);
            }
        }

        public bool RemoveEntry(SavedServerEntry entry)
        {
            lock (_loadingLock)
            {
                var newEntry = _data.FirstOrDefault(x => x.InternalIdentifier.Equals(entry.InternalIdentifier));

                return newEntry != null && _data.Remove(newEntry);
            }
        }
    }
}
