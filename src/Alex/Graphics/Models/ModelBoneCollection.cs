using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Alex.Graphics.Models
{
	/// <summary>
	/// Represents a set of bones associated with a model.
	/// </summary>
	public class ModelBoneCollection : IReadOnlyCollection<ModelBone>
	{
		/// <inheritdoc />
		public int Count => _items.Count;

		public ImmutableArray<ModelBone> ImmutableArray => _bones;

		private ObservableCollection<ModelBone> _items;

		private ImmutableArray<ModelBone> _bones = new ImmutableArray<ModelBone>();

		private object _boneLock = new object();
		//public ModelBone[] Data => Items.

		public EventHandler<NotifyCollectionChangedEventArgs> CollectionChanged;

		public ModelBoneCollection(IList<ModelBone> list)
		{
			_items = new ObservableCollection<ModelBone>();
			_items.CollectionChanged += ItemsChanged;

			foreach (var modelBone in list)
				_items.Add(modelBone);
		}

		private void ItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var items = _items;

			//lock (_boneLock)
			{
				_bones = items.ToImmutableArray();
			}

			if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
			{
				int startIndex = e.NewStartingIndex;

				foreach (ModelBone item in e.NewItems)
				{
					item.Index = startIndex++;
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
			{
				foreach (ModelBone removed in e.OldItems)
				{
					removed.Index = -1;
				}

				if (e.OldStartingIndex < items.Count)
				{
					for (var index = e.OldStartingIndex; index < items.Count; index++)
					{
						var modelBone = items[index];

						if (modelBone == null)
							continue;

						modelBone.Index = index;
					}
				}
			}

			CollectionChanged?.Invoke(this, e);
			//SetIndexes();
		}


		internal void Add(ModelBone item)
		{
			//	lock (_boneLock)
			{
				_items.Add(item);
			}
		}

		internal void Remove(ModelBone item)
		{
			//	lock (_boneLock)
			{
				_items.Remove(item);
			}
		}

		/// <summary>
		/// Retrieves a ModelBone from the collection, given the name of the bone.
		/// </summary>
		/// <param name="boneName">The name of the bone to retrieve.</param>
		public ModelBone this[string boneName]
		{
			get
			{
				ModelBone ret;

				if (!TryGetValue(boneName, out ret))
					throw new KeyNotFoundException();

				return ret;
			}
		}

		public ModelBone this[int index]
		{
			get
			{
				return _items[index];
			}
		}

		/// <summary>
		/// Finds a bone with a given name if it exists in the collection.
		/// </summary>
		/// <param name="boneName">The name of the bone to find.</param>
		/// <param name="value">The bone named boneName, if found.</param>
		/// <returns>true if the bone was found</returns>
		public bool TryGetValue(string boneName, out ModelBone value)
		{
			if (string.IsNullOrEmpty(boneName))
				throw new ArgumentNullException("boneName");

			foreach (ModelBone bone in this)
			{
				if (string.Equals(bone.Name, boneName, StringComparison.OrdinalIgnoreCase))
				{
					value = bone;

					return true;
				}
			}

			value = null;

			return false;
		}

		/// <inheritdoc />
		IEnumerator<ModelBone> IEnumerable<ModelBone>.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Returns a ModelMeshCollection.Enumerator that can iterate through a ModelMeshCollection.
		/// </summary>
		/// <returns></returns>
		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Provides the ability to iterate through the bones in an ModelMeshCollection.
		/// </summary>
		public struct Enumerator : IEnumerator<ModelBone>
		{
			private readonly ModelBoneCollection _collection;
			private int _position;

			internal Enumerator(ModelBoneCollection collection)
			{
				_collection = collection;
				_position = -1;
			}


			/// <summary>
			/// Gets the current element in the ModelMeshCollection.
			/// </summary>
			public ModelBone Current { get { return _collection[_position]; } }

			/// <summary>
			/// Advances the enumerator to the next element of the ModelMeshCollection.
			/// </summary>
			public bool MoveNext()
			{
				_position++;

				return (_position < _collection.Count);
			}

			#region IDisposable

			/// <summary>
			/// Immediately releases the unmanaged resources used by this object.
			/// </summary>
			public void Dispose() { }

			#endregion

			#region IEnumerator Members

			object IEnumerator.Current
			{
				get { return _collection[_position]; }
			}

			public void Reset()
			{
				_position = -1;
			}

			#endregion
		}
	}
}