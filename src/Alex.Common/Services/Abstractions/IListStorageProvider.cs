using System.Collections.Generic;

namespace Alex.Common.Services
{
    public interface IListStorageProvider<TEntryType> : IDataProvider<IReadOnlyCollection<TEntryType>>
    {
        void MoveEntry(int index, TEntryType entry);
        void AddEntry(TEntryType entry);
        bool RemoveEntry(TEntryType entry);
    }
}
