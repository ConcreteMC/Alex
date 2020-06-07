



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class GhastModel : EntityModel
	{
		public GhastModel()
		{
			Name = "geometry.ghast";
			VisibleBoundsWidth = 6;
			VisibleBoundsHeight = 9;
			VisibleBoundsOffset = new Vector3(0f, 4.5f, 0f);
			Texturewidth = 64;
			Textureheight = 32;
			Bones = new EntityModelBone[10]
			{
				new EntityModelBone(){ 
					Name = "tentacles_0",
					Parent = "body",
					Pivot = new Vector3(-3.8f,1f,-5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4.8f,-8f,-6f),
							Size = new Vector3(2f, 9f, 2f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tentacles_1",
					Parent = "body",
					Pivot = new Vector3(1.3f,1f,-5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0.3f,-10f,-6f),
							Size = new Vector3(2f, 11f, 2f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tentacles_2",
					Parent = "body",
					Pivot = new Vector3(6.3f,1f,-5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(5.3f,-7f,-6f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tentacles_3",
					Parent = "body",
					Pivot = new Vector3(-6.3f,1f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-7.3f,-8f,-1f),
							Size = new Vector3(2f, 9f, 2f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tentacles_4",
					Parent = "body",
					Pivot = new Vector3(-1.3f,1f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.3f,-12f,-1f),
							Size = new Vector3(2f, 13f, 2f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tentacles_5",
					Parent = "body",
					Pivot = new Vector3(3.8f,1f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2.8f,-10f,-1f),
							Size = new Vector3(2f, 11f, 2f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tentacles_6",
					Parent = "body",
					Pivot = new Vector3(-3.8f,1f,5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4.8f,-11f,4f),
							Size = new Vector3(2f, 12f, 2f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tentacles_7",
					Parent = "body",
					Pivot = new Vector3(1.3f,1f,5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0.3f,-11f,4f),
							Size = new Vector3(2f, 12f, 2f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tentacles_8",
					Parent = "body",
					Pivot = new Vector3(6.3f,1f,5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(5.3f,-12f,4f),
							Size = new Vector3(2f, 13f, 2f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,8f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-8f,0f,-8f),
							Size = new Vector3(16f, 16f, 16f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
			};
		}

	}

}