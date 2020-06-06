



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class LlamaModel : EntityModel
	{
		public LlamaModel()
		{
			Name = "geometry.llama";
			VisibleBoundsWidth = 2;
			VisibleBoundsHeight = 2;
			VisibleBoundsOffset = new Vector3(0f, 1f, 0f);
			Texturewidth = 128;
			Textureheight = 64;
			Bones = new EntityModelBone[8]
			{
				new EntityModelBone(){ 
					Name = "head",
					Parent = "",
					Pivot = new Vector3(0f,17f,-6f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[4]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,27f,-16f),
							Size = new Vector3(4f, 4f, 9f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,15f,-12f),
							Size = new Vector3(8f, 18f, 6f),
							Uv = new Vector2(0f, 14f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,33f,-10f),
							Size = new Vector3(3f, 3f, 2f),
							Uv = new Vector2(17f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(1f,33f,-10f),
							Size = new Vector3(3f, 3f, 2f),
							Uv = new Vector2(17f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "chest1",
					Parent = "",
					Pivot = new Vector3(-8.5f,21f,3f),
					Rotation = new Vector3(0f,90f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-11.5f,13f,3f),
							Size = new Vector3(8f, 8f, 3f),
							Uv = new Vector2(45f, 28f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "chest2",
					Parent = "",
					Pivot = new Vector3(5.5f,21f,3f),
					Rotation = new Vector3(0f,90f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2.5f,13f,3f),
							Size = new Vector3(8f, 8f, 3f),
							Uv = new Vector2(45f, 41f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,19f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-6f,11f,-5f),
							Size = new Vector3(12f, 18f, 10f),
							Uv = new Vector2(29f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg0",
					Parent = "",
					Pivot = new Vector3(-3.5f,14f,6f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5.5f,0f,4f),
							Size = new Vector3(4f, 14f, 4f),
							Uv = new Vector2(29f, 29f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg1",
					Parent = "",
					Pivot = new Vector3(3.5f,14f,6f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1.5f,0f,4f),
							Size = new Vector3(4f, 14f, 4f),
							Uv = new Vector2(29f, 29f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg2",
					Parent = "",
					Pivot = new Vector3(-3.5f,14f,-5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5.5f,0f,-7f),
							Size = new Vector3(4f, 14f, 4f),
							Uv = new Vector2(29f, 29f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg3",
					Parent = "",
					Pivot = new Vector3(3.5f,14f,-5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1.5f,0f,-7f),
							Size = new Vector3(4f, 14f, 4f),
							Uv = new Vector2(29f, 29f)
						},
					}
				},
			};
		}

	}

}