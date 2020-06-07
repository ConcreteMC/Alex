



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class EndermiteModel : EntityModel
	{
		public EndermiteModel()
		{
			Name = "geometry.endermite";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 64;
			Textureheight = 32;
			Bones = new EntityModelBone[4]
			{
				new EntityModelBone(){ 
					Name = "section_0",
					Parent = "section_2",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,0f,-4.4f),
							Size = new Vector3(4f, 3f, 2f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "section_1",
					Parent = "section_2",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,0f,-2.4f),
							Size = new Vector3(6f, 4f, 5f),
							Uv = new Vector2(0f, 5f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "section_2",
					Parent = "",
					Pivot = new Vector3(0f,0f,2.5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,0f,2.5f),
							Size = new Vector3(3f, 3f, 1f),
							Uv = new Vector2(0f, 14f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "section_3",
					Parent = "section_2",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-0.5f,0f,3.5f),
							Size = new Vector3(1f, 2f, 1f),
							Uv = new Vector2(0f, 18f)
						},
					}
				},
			};
		}

	}

}