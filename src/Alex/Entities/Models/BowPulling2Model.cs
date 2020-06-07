



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class BowPulling2Model : EntityModel
	{
		public BowPulling2Model()
		{
			Name = "geometry.bow_pulling_2";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 16;
			Textureheight = 16;
			Bones = new EntityModelBone[1]
			{
				new EntityModelBone(){ 
					Name = "rightitem",
					Parent = "",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[0]
				},
			};
		}

	}

}