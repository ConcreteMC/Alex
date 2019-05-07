


using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class EvokerModel : EntityModel
	{
		public EvokerModel()
		{
			Name = "definition.evoker";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 0;
			Textureheight = 0;
			Bones = new EntityModelBone[0];
		}

	}

}