using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models
{
	public class VertexBufferCollection: System.Collections.ObjectModel.ReadOnlyCollection<VertexBuffer>
	{
		public VertexBufferCollection(IList<VertexBuffer> list)
			: base(list)
		{
			
		}

		internal void Add(VertexBuffer item)
		{
			Items.Add(item);
		}
		
		internal void Remove(VertexBuffer item)
		{
			Items.Remove(item);
		}
		
		/// <summary>
		/// Retrieves a ModelBone from the collection, given the name of the bone.
		/// </summary>
		/// <param name="boneName">The name of the bone to retrieve.</param>
		public VertexBuffer this[string boneName]
		{
			get
			{
				VertexBuffer ret;
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
		public bool TryGetValue(string boneName, out VertexBuffer value)
		{
			if (string.IsNullOrEmpty(boneName))
				throw new ArgumentNullException("boneName");

			foreach (VertexBuffer bone in this)
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
		public struct Enumerator : IEnumerator<VertexBuffer>
		{
			private readonly VertexBufferCollection _collection;
			private int _position;

			internal Enumerator(VertexBufferCollection collection)
			{
				_collection = collection;
				_position = -1;
			}


			/// <summary>
			/// Gets the current element in the ModelMeshCollection.
			/// </summary>
			public VertexBuffer Current { get { return _collection[_position]; } }

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