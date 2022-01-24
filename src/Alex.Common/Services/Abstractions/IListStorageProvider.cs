using System.Collections.Generic;

namespace Alex.Common.Services
{
	public interface IListStorageProvider<TEntryType> : IDataProvider<IReadOnlyCollection<TEntryType>>
	{
		bool MoveUp(TEntryType entry);

		bool MoveDown(TEntryType entry);

		void MoveEntry(int index, TEntryType entry);

		void AddEntry(TEntryType entry);

		bool RemoveEntry(TEntryType entry);
	}
}