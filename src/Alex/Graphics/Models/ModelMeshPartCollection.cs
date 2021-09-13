using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Alex.Graphics.Models
{
	public sealed class ModelMeshPartCollection : ReadOnlyCollection<ModelMeshPart>
	{
		public ModelMeshPartCollection(IList<ModelMeshPart> list)
			: base(list)
		{

		}
	}
}