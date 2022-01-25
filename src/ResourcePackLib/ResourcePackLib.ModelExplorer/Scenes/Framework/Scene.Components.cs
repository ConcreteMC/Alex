using System.Collections;
using Microsoft.Xna.Framework;

namespace ResourcePackLib.ModelExplorer.Scenes;

public abstract partial class Scene
{

    private readonly GameComponentCollection _components = new GameComponentCollection();
    public GameComponentCollection Components => _components;

    private void InitializeComponents()
    {
        InitializeExistingComponents();

        CategorizeComponents();
        _components.ComponentAdded += Components_ComponentAdded;
        _components.ComponentRemoved += Components_ComponentRemoved;
    }
    
    private void Components_ComponentAdded(object sender, GameComponentCollectionEventArgs e)
    {
        e.GameComponent.Initialize();
        CategorizeComponent(e.GameComponent);
    }

    private void Components_ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
    {
        DecategorizeComponent(e.GameComponent);
    }
    
    private void InitializeExistingComponents()
    {
        for (int index = 0; index < Components.Count; ++index)
            Components[index].Initialize();
    }

    private void CategorizeComponents()
    {
        DecategorizeComponents();
        for (int index = 0; index < Components.Count; ++index)
            CategorizeComponent(Components[index]);
    }

    private void DecategorizeComponents()
    {
        _updateables.Clear();
        _drawables.Clear();
    }

    private void CategorizeComponent(IGameComponent component)
    {
        if (component is IUpdateable)
            _updateables.Add((IUpdateable)component);
        if (!(component is IDrawable))
            return;
        _drawables.Add((IDrawable)component);
    }

    private void DecategorizeComponent(IGameComponent component)
    {
        if (component is IUpdateable)
            _updateables.Remove((IUpdateable)component);
        if (!(component is IDrawable))
            return;
        _drawables.Remove((IDrawable)component);
    }


    private SortingFilteringCollection<IDrawable> _drawables = new(
        d => d.Visible,
        (d, handler) => d.VisibleChanged += handler,
        (d, handler) => d.VisibleChanged -= handler,
        (d1, d2) => Comparer<int>.Default.Compare(d1.DrawOrder, d2.DrawOrder),
        (d, handler) => d.DrawOrderChanged += handler,
        (d, handler) => d.DrawOrderChanged -= handler);

    private SortingFilteringCollection<IUpdateable> _updateables = new(
        u => u.Enabled,
        (u, handler) => u.EnabledChanged += handler,
        (u, handler) => u.EnabledChanged -= handler,
        (u1, u2) => Comparer<int>.Default.Compare(u1.UpdateOrder, u2.UpdateOrder),
        (u, handler) => u.UpdateOrderChanged += handler,
        (u, handler) => u.UpdateOrderChanged -= handler);

    /// <summary>
    /// The SortingFilteringCollection class provides efficient, reusable
    /// sorting and filtering based on a configurable sort comparer, filter
    /// predicate, and associate change events.
    /// </summary>
    private class SortingFilteringCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private readonly List<T> _items;
        private readonly List<AddJournalEntry<T>> _addJournal;
        private readonly Comparison<AddJournalEntry<T>> _addJournalSortComparison;
        private readonly List<int> _removeJournal;
        private readonly List<T> _cachedFilteredItems;
        private bool _shouldRebuildCache;
        private readonly Predicate<T> _filter;
        private readonly Comparison<T> _sort;
        private readonly Action<T, EventHandler<EventArgs>> _filterChangedSubscriber;
        private readonly Action<T, EventHandler<EventArgs>> _filterChangedUnsubscriber;
        private readonly Action<T, EventHandler<EventArgs>> _sortChangedSubscriber;
        private readonly Action<T, EventHandler<EventArgs>> _sortChangedUnsubscriber;

        private static readonly Comparison<int> RemoveJournalSortComparison =
            (x, y) => Comparer<int>.Default.Compare(y, x);

        public SortingFilteringCollection(
            Predicate<T> filter,
            Action<T, EventHandler<EventArgs>> filterChangedSubscriber,
            Action<T, EventHandler<EventArgs>> filterChangedUnsubscriber,
            Comparison<T> sort,
            Action<T, EventHandler<EventArgs>> sortChangedSubscriber,
            Action<T, EventHandler<EventArgs>> sortChangedUnsubscriber)
        {
            _items = new List<T>();
            _addJournal = new List<AddJournalEntry<T>>();
            _removeJournal = new List<int>();
            _cachedFilteredItems = new List<T>();
            _shouldRebuildCache = true;
            _filter = filter;
            _filterChangedSubscriber = filterChangedSubscriber;
            _filterChangedUnsubscriber = filterChangedUnsubscriber;
            _sort = sort;
            _sortChangedSubscriber = sortChangedSubscriber;
            _sortChangedUnsubscriber = sortChangedUnsubscriber;
            _addJournalSortComparison = CompareAddJournalEntry;
        }

        private int CompareAddJournalEntry(AddJournalEntry<T> x, AddJournalEntry<T> y)
        {
            int num = _sort(x.Item, y.Item);
            return num != 0 ? num : x.Order - y.Order;
        }

        public void ForEachFilteredItem<TUserData>(Action<T, TUserData> action, TUserData userData)
        {
            if (_shouldRebuildCache)
            {
                ProcessRemoveJournal();
                ProcessAddJournal();
                _cachedFilteredItems.Clear();
                for (int index = 0; index < _items.Count; ++index)
                {
                    if (_filter(_items[index]))
                        _cachedFilteredItems.Add(_items[index]);
                }

                _shouldRebuildCache = false;
            }

            for (int index = 0; index < _cachedFilteredItems.Count; ++index)
                action(_cachedFilteredItems[index], userData);
            if (!_shouldRebuildCache)
                return;
            _cachedFilteredItems.Clear();
        }

        public void Add(T item)
        {
            _addJournal.Add(new AddJournalEntry<T>(_addJournal.Count, item));
            InvalidateCache();
        }

        public bool Remove(T item)
        {
            if (_addJournal.Remove(AddJournalEntry<T>.CreateKey(item)))
                return true;
            int num = _items.IndexOf(item);
            if (num < 0)
                return false;
            UnsubscribeFromItemEvents(item);
            _removeJournal.Add(num);
            InvalidateCache();
            return true;
        }

        public void Clear()
        {
            for (int index = 0; index < _items.Count; ++index)
            {
                _filterChangedUnsubscriber(_items[index], Item_FilterPropertyChanged);
                _sortChangedUnsubscriber(_items[index], Item_SortPropertyChanged);
            }

            _addJournal.Clear();
            _removeJournal.Clear();
            _items.Clear();
            InvalidateCache();
        }

        public bool Contains(T item) => _items.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        public int Count => _items.Count;

        public bool IsReadOnly => false;

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();

        private void ProcessRemoveJournal()
        {
            if (_removeJournal.Count == 0)
                return;
            _removeJournal.Sort(RemoveJournalSortComparison);
            for (int index = 0; index < _removeJournal.Count; ++index)
                _items.RemoveAt(_removeJournal[index]);
            _removeJournal.Clear();
        }

        private void ProcessAddJournal()
        {
            if (_addJournal.Count == 0)
                return;
            _addJournal.Sort(_addJournalSortComparison);
            int index1 = 0;
            for (int index2 = 0; index2 < _items.Count && index1 < _addJournal.Count; ++index2)
            {
                T x = _addJournal[index1].Item;
                if (_sort(x, _items[index2]) < 0)
                {
                    SubscribeToItemEvents(x);
                    _items.Insert(index2, x);
                    ++index1;
                }
            }

            for (; index1 < _addJournal.Count; ++index1)
            {
                T obj = _addJournal[index1].Item;
                SubscribeToItemEvents(obj);
                _items.Add(obj);
            }

            _addJournal.Clear();
        }

        private void SubscribeToItemEvents(T item)
        {
            _filterChangedSubscriber(item, Item_FilterPropertyChanged);
            _sortChangedSubscriber(item, Item_SortPropertyChanged);
        }

        private void UnsubscribeFromItemEvents(T item)
        {
            _filterChangedUnsubscriber(item, Item_FilterPropertyChanged);
            _sortChangedUnsubscriber(item, Item_SortPropertyChanged);
        }

        private void InvalidateCache() => _shouldRebuildCache = true;

        private void Item_FilterPropertyChanged(object sender, EventArgs e) => InvalidateCache();

        private void Item_SortPropertyChanged(object sender, EventArgs e)
        {
            T obj = (T)sender;
            int num = _items.IndexOf(obj);
            _addJournal.Add(new AddJournalEntry<T>(_addJournal.Count, obj));
            _removeJournal.Add(num);
            UnsubscribeFromItemEvents(obj);
            InvalidateCache();
        }
    }

    private struct AddJournalEntry<T>
    {
        public readonly int Order;
        public readonly T Item;

        public AddJournalEntry(int order, T item)
        {
            Order = order;
            Item = item;
        }

        public static AddJournalEntry<T> CreateKey(T item) => new AddJournalEntry<T>(-1, item);

        public override int GetHashCode() => Item.GetHashCode();

        public override bool Equals(object obj) =>
            obj is AddJournalEntry<T> addJournalEntry && Equals(Item, addJournalEntry.Item);
    }

}