using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Alex.Graphics.Models.Entity
{
	public sealed class ModelMeshPartCollection : ReadOnlyCollection<ModelMeshPart>
	{
		public ModelMeshPartCollection(IList<ModelMeshPart> list)
			: base(list)
		{

		}
	}
}