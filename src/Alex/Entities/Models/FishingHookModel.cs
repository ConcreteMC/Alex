



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class FishingHookModel : EntityModel
	{
		public FishingHookModel()
		{
			Name = "geometry.fishing_hook";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 24;
			Textureheight = 3;
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
					Cubes = new EntityModelCube[4]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,-1.5f,-1.5f),
							Size = new Vector3(3f, 3f, 3f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(0f,-4.5f,-0.5f),
							Size = new Vector3(0f, 3f, 3f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(0f,1.5f,-1.5f),
							Size = new Vector3(0f, 3f, 3f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,1.5f,0f),
							Size = new Vector3(3f, 3f, 0f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
			};
		}

	}

}