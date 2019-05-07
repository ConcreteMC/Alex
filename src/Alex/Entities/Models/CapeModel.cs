


using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class CapeModel : EntityModel
	{
		public CapeModel()
		{
			Name = "geometry.cape";
			VisibleBoundsWidth = 1;
			VisibleBoundsHeight = 1;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 64;
			Textureheight = 32;
			Bones = new EntityModelBone[1]
			{
				new EntityModelBone(){ 
					Name = "cape",
					Parent = "",
					Pivot = new Vector3(0f,24f,-3f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,8f,-3f),
							Size = new Vector3(10f, 16f, 1f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
			};
		}

	}

}