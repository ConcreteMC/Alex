



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class SalmonModel : EntityModel
	{
		public SalmonModel()
		{
			Name = "geometry.salmon";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 32;
			Textureheight = 32;
			Bones = new EntityModelBone[8]
			{
				new EntityModelBone(){ 
					Name = "body_front",
					Parent = "",
					Pivot = new Vector3(0f,0f,-4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,3.5f,-4f),
							Size = new Vector3(3f, 5f, 8f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body_back",
					Parent = "body_front",
					Pivot = new Vector3(0f,0f,4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,3.5f,4f),
							Size = new Vector3(3f, 5f, 8f),
							Uv = new Vector2(0f, 13f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "dorsal_front",
					Parent = "body_front",
					Pivot = new Vector3(0f,5f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,8.5f,2f),
							Size = new Vector3(0f, 2f, 2f),
							Uv = new Vector2(4f, 2f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "dorsal_back",
					Parent = "body_back",
					Pivot = new Vector3(0f,5f,4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,8.5f,4f),
							Size = new Vector3(0f, 2f, 3f),
							Uv = new Vector2(2f, 3f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tailfin",
					Parent = "body_back",
					Pivot = new Vector3(0f,0f,12f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,3.5f,12f),
							Size = new Vector3(0f, 5f, 6f),
							Uv = new Vector2(20f, 10f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "head",
					Parent = "body_front",
					Pivot = new Vector3(0f,3f,-4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,4.5f,-7f),
							Size = new Vector3(2f, 4f, 3f),
							Uv = new Vector2(22f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftFin",
					Parent = "body_front",
					Pivot = new Vector3(1.5f,1f,-4f),
					Rotation = new Vector3(0f,0f,35f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-0.50752f,3.86703f,-4f),
							Size = new Vector3(2f, 0f, 2f),
							Uv = new Vector2(2f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightFin",
					Parent = "body_front",
					Pivot = new Vector3(-1.5f,1f,-4f),
					Rotation = new Vector3(0f,0f,-35f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.49258f,3.86703f,-4f),
							Size = new Vector3(2f, 0f, 2f),
							Uv = new Vector2(-2f, 0f)
						},
					}
				},
			};
		}

	}

}