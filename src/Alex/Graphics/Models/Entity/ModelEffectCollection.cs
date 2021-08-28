using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Alex.Graphics.Models.Entity
{
	public sealed class ModelEffectCollection : ReadOnlyCollection<Microsoft.Xna.Framework.Graphics.Effect>
	{
		internal ModelEffectCollection(IList<Microsoft.Xna.Framework.Graphics.Effect> list)
			: base(list)
		{

		}

		internal ModelEffectCollection() : base(new List<Microsoft.Xna.Framework.Graphics.Effect>())
		{
		}
		
		//ModelMeshPart needs to be able to add to ModelMesh's effects list
		internal void Add(Microsoft.Xna.Framework.Graphics.Effect item)
		{
			Items.Add (item);
		}
		internal void Remove(Microsoft.Xna.Framework.Graphics.Effect item)
		{
			Items.Remove (item);
		}

		// Summary:
		//     Returns a ModelEffectCollection.Enumerator that can iterate through a ModelEffectCollection.
		public new ModelEffectCollection.Enumerator GetEnumerator()
		{
			return new ModelEffectCollection.Enumerator((List<Microsoft.Xna.Framework.Graphics.Effect>)Items);
		}

		// Summary:
		//     Provides the ability to iterate through the bones in an ModelEffectCollection.
		public struct Enumerator : IEnumerator<Microsoft.Xna.Framework.Graphics.Effect>, IDisposable, IEnumerator
		{
			List<Microsoft.Xna.Framework.Graphics.Effect>.Enumerator enumerator;
			bool disposed;

			internal Enumerator(List<Microsoft.Xna.Framework.Graphics.Effect> list)
			{
				enumerator = list.GetEnumerator();
				disposed = false;
			}

			// Summary:
			//     Gets the current element in the ModelEffectCollection.
			public Microsoft.Xna.Framework.Graphics.Effect Current { get { return enumerator.Current; } }

			// Summary:
			//     Immediately releases the unmanaged resources used by this object.
			public void Dispose()
			{
				if (!disposed)
				{
					enumerator.Dispose();
					disposed = true;
				}
			}
			//
			// Summary:
			//     Advances the enumerator to the next element of the ModelEffectCollection.
			public bool MoveNext() { return enumerator.MoveNext(); }

			#region IEnumerator Members

			object IEnumerator.Current
			{
				get { return Current; }
			}

			void IEnumerator.Reset()
			{
				IEnumerator resetEnumerator = enumerator;
				resetEnumerator.Reset ();
				enumerator = (List<Microsoft.Xna.Framework.Graphics.Effect>.Enumerator)resetEnumerator;
			}

			#endregion
		}
	}
}