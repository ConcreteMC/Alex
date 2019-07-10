using System.Collections.Generic;

namespace Alex.API.Services
{
    public interface IListStorageProvider<TEntryType> : IDataProvider<IReadOnlyCollection<TEntryType>>
    {
        void MoveEntry(int index, TEntryType entry);
        void AddEntry(TEntryType entry);
        void RemoveEntry(TEntryType entry);
    }
}
