



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class RabbitModel : EntityModel
	{
		public RabbitModel()
		{
			Name = "geometry.rabbit";
			VisibleBoundsWidth = 1;
			VisibleBoundsHeight = 1;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 64;
			Textureheight = 32;
			Bones = new EntityModelBone[12]
			{
				new EntityModelBone(){ 
					Name = "rearFootLeft",
					Parent = "",
					Pivot = new Vector3(3f,6.5f,3.7f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2f,0f,0f),
							Size = new Vector3(2f, 1f, 7f),
							Uv = new Vector2(8f, 24f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rearFootRight",
					Parent = "",
					Pivot = new Vector3(-3f,6.5f,3.7f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,0f,0f),
							Size = new Vector3(2f, 1f, 7f),
							Uv = new Vector2(26f, 24f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "haunchLeft",
					Parent = "",
					Pivot = new Vector3(3f,6.5f,3.7f),
					Rotation = new Vector3(-20f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2f,2.5f,3.7f),
							Size = new Vector3(2f, 4f, 5f),
							Uv = new Vector2(16f, 15f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "haunchRight",
					Parent = "",
					Pivot = new Vector3(-3f,6.5f,3.7f),
					Rotation = new Vector3(-20f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,2.5f,3.7f),
							Size = new Vector3(2f, 4f, 5f),
							Uv = new Vector2(30f, 15f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,5f,8f),
					Rotation = new Vector3(-20f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,2f,-2f),
							Size = new Vector3(6f, 5f, 10f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "frontLegLeft",
					Parent = "",
					Pivot = new Vector3(3f,7f,-1f),
					Rotation = new Vector3(-10f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2f,0f,-2f),
							Size = new Vector3(2f, 7f, 2f),
							Uv = new Vector2(8f, 15f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "frontLegRight",
					Parent = "",
					Pivot = new Vector3(-3f,7f,-1f),
					Rotation = new Vector3(-10f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,0f,-2f),
							Size = new Vector3(2f, 7f, 2f),
							Uv = new Vector2(0f, 15f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "head",
					Parent = "",
					Pivot = new Vector3(0f,8f,-1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.5f,8f,-6f),
							Size = new Vector3(5f, 4f, 5f),
							Uv = new Vector2(32f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "earRight",
					Parent = "",
					Pivot = new Vector3(0f,8f,-1f),
					Rotation = new Vector3(0f,-15f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.5f,12f,-2f),
							Size = new Vector3(2f, 5f, 1f),
							Uv = new Vector2(58f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "earLeft",
					Parent = "",
					Pivot = new Vector3(0f,8f,-1f),
					Rotation = new Vector3(0f,15f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0.5f,12f,-2f),
							Size = new Vector3(2f, 5f, 1f),
							Uv = new Vector2(52f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tail",
					Parent = "",
					Pivot = new Vector3(0f,4f,7f),
					Rotation = new Vector3(-20f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,2.5f,7f),
							Size = new Vector3(3f, 3f, 2f),
							Uv = new Vector2(52f, 6f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "nose",
					Parent = "",
					Pivot = new Vector3(0f,8f,-1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-0.5f,9.5f,-6.5f),
							Size = new Vector3(1f, 1f, 1f),
							Uv = new Vector2(32f, 9f)
						},
					}
				},
			};
		}

	}

}