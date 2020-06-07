



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class OcelotModel : EntityModel
	{
		public OcelotModel()
		{
			Name = "geometry.ocelot";
			VisibleBoundsWidth = 2;
			VisibleBoundsHeight = 1;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 64;
			Textureheight = 32;
			Bones = new EntityModelBone[8]
			{
				new EntityModelBone(){ 
					Name = "head",
					Parent = "",
					Pivot = new Vector3(0f,9f,-9f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[4]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.5f,7f,-12f),
							Size = new Vector3(5f, 4f, 5f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,7.015625f,-13f),
							Size = new Vector3(3f, 2f, 2f),
							Uv = new Vector2(0f, 24f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,11f,-9f),
							Size = new Vector3(1f, 1f, 2f),
							Uv = new Vector2(0f, 10f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(1f,11f,-9f),
							Size = new Vector3(1f, 1f, 2f),
							Uv = new Vector2(6f, 10f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,12f,-10f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(90f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,-7f,-18f),
							Size = new Vector3(4f, 16f, 6f),
							Uv = new Vector2(20f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tail1",
					Parent = "",
					Pivot = new Vector3(0f,9f,8f),
					Rotation = new Vector3(50f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-0.5f,1f,8f),
							Size = new Vector3(1f, 8f, 1f),
							Uv = new Vector2(0f, 15f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tail2",
					Parent = "",
					Pivot = new Vector3(0f,4f,13f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-0.5f,-4f,13f),
							Size = new Vector3(1f, 8f, 1f),
							Uv = new Vector2(4f, 15f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "backLegL",
					Parent = "",
					Pivot = new Vector3(1.1f,6f,5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0.1f,0f,6f),
							Size = new Vector3(2f, 6f, 2f),
							Uv = new Vector2(8f, 13f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "backLegR",
					Parent = "",
					Pivot = new Vector3(-1.1f,6f,5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.1f,0f,6f),
							Size = new Vector3(2f, 6f, 2f),
							Uv = new Vector2(8f, 13f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "frontLegL",
					Parent = "",
					Pivot = new Vector3(1.2f,10.2f,-5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0.2f,0.2f,-5f),
							Size = new Vector3(2f, 10f, 2f),
							Uv = new Vector2(40f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "frontLegR",
					Parent = "",
					Pivot = new Vector3(-1.2f,10.2f,-5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.2f,0.2f,-5f),
							Size = new Vector3(2f, 10f, 2f),
							Uv = new Vector2(40f, 0f)
						},
					}
				},
			};
		}

	}

}