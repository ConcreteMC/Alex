



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class ShieldModel : EntityModel
	{
		public ShieldModel()
		{
			Name = "geometry.shield";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 64;
			Textureheight = 64;
			Bones = new EntityModelBone[1]
			{
				new EntityModelBone(){ 
					Name = "shield",
					Parent = "",
					Pivot = new Vector3(1f,15.5f,3f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,25f,0f),
							Size = new Vector3(2f, 6f, 6f),
							Uv = new Vector2(26f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,17f,-1f),
							Size = new Vector3(12f, 22f, 1f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
			};
		}

	}

}