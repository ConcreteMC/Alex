



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class ShulkerBulletModel : EntityModel
	{
		public ShulkerBulletModel()
		{
			Name = "geometry.shulker_bullet";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 64;
			Textureheight = 32;
			Bones = new EntityModelBone[1]
			{
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[3]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,-4f,-1f),
							Size = new Vector3(8f, 8f, 2f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,-4f,-4f),
							Size = new Vector3(2f, 8f, 8f),
							Uv = new Vector2(0f, 10f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,-1f,-4f),
							Size = new Vector3(8f, 2f, 8f),
							Uv = new Vector2(20f, 0f)
						},
					}
				},
			};
		}

	}

}