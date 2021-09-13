using System;
using System.Collections;
using System.Collections.Generic;

namespace Alex.Graphics.Models
{
	/// <summary>
	/// Represents a set of bones associated with a model.
	/// </summary>
	public class ModelBoneCollection : System.Collections.ObjectModel.ReadOnlyCollection<ModelBone>
	{
		public ModelBoneCollection(IList<ModelBone> list)
			: base(list)
		{
			SetIndexes();
		}

		private void SetIndexes()
		{
			for (var index = 0; index < Items.Count; index++)
			{
				var modelBone = Items[index];
				if (modelBone == null)
					continue;
				modelBone.Index = index;
			}
		}
		
		internal void Add(ModelBone item)
		{
			Items.Add(item);
			SetIndexes();
		}
		
		internal void Remove(ModelBone item)
		{
			Items.Remove(item);
			SetIndexes();
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

		/// <summary>
		/// Returns a ModelMeshCollection.Enumerator that can iterate through a ModelMeshCollection.
		/// </summary>
		/// <returns></returns>
		public new Enumerator GetEnumerator()
		{
			return new Enumerator(this);
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
			public void Dispose()
			{
			}

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