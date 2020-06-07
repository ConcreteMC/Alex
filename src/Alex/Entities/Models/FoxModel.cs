



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class FoxModel : EntityModel
	{
		public FoxModel()
		{
			Name = "geometry.fox";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 64;
			Textureheight = 32;
			Bones = new EntityModelBone[10]
			{
				new EntityModelBone(){ 
					Name = "world",
					Parent = "",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[0]{
					}
				},
				new EntityModelBone(){ 
					Name = "root",
					Parent = "world",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[0]{
					}
				},
				new EntityModelBone(){ 
					Name = "head",
					Parent = "body",
					Pivot = new Vector3(0f,8f,-3f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[4]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,4f,-9f),
							Size = new Vector3(8f, 6f, 6f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,10f,-8f),
							Size = new Vector3(2f, 2f, 1f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(2f,10f,-8f),
							Size = new Vector3(2f, 2f, 1f),
							Uv = new Vector2(22f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,4f,-12f),
							Size = new Vector3(4f, 2f, 3f),
							Uv = new Vector2(0f, 24f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "head_sleeping",
					Parent = "head",
					Pivot = new Vector3(0f,8f,-3f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[4]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,4f,-9f),
							Size = new Vector3(8f, 6f, 6f),
							Uv = new Vector2(0f, 12f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,10f,-8f),
							Size = new Vector3(2f, 2f, 1f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(2f,10f,-8f),
							Size = new Vector3(2f, 2f, 1f),
							Uv = new Vector2(22f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,4f,-12f),
							Size = new Vector3(4f, 2f, 3f),
							Uv = new Vector2(0f, 24f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body",
					Parent = "root",
					Pivot = new Vector3(0f,8f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(90f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,0f,-3f),
							Size = new Vector3(6f, 11f, 6f),
							Uv = new Vector2(30f, 15f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg0",
					Parent = "body",
					Pivot = new Vector3(-3f,6f,6f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3.005f,0f,5f),
							Size = new Vector3(2f, 6f, 2f),
							Uv = new Vector2(14f, 24f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg1",
					Parent = "body",
					Pivot = new Vector3(1f,6f,6f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1.005f,0f,5f),
							Size = new Vector3(2f, 6f, 2f),
							Uv = new Vector2(22f, 24f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg2",
					Parent = "body",
					Pivot = new Vector3(-3f,6f,-1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3.005f,0f,-2f),
							Size = new Vector3(2f, 6f, 2f),
							Uv = new Vector2(14f, 24f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg3",
					Parent = "body",
					Pivot = new Vector3(1f,6f,-1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1.005f,0f,-2f),
							Size = new Vector3(2f, 6f, 2f),
							Uv = new Vector2(22f, 24f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tail",
					Parent = "body",
					Pivot = new Vector3(0f,8f,7f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(80f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,-2f,4.75f),
							Size = new Vector3(4f, 9f, 5f),
							Uv = new Vector2(28f, 0f)
						},
					}
				},
			};
		}

	}

}