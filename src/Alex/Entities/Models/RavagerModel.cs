



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class RavagerModel : EntityModel
	{
		public RavagerModel()
		{
			Name = "geometry.ravager";
			VisibleBoundsWidth = 4;
			VisibleBoundsHeight = 4;
			VisibleBoundsOffset = new Vector3(0f, 1.25f, 0f);
			Texturewidth = 128;
			Textureheight = 128;
			Bones = new EntityModelBone[9]
			{
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,19f,2f),
					Rotation = new Vector3(90f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-7f,10f,-2f),
							Size = new Vector3(14f, 16f, 20f),
							Uv = new Vector2(0f, 55f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-6f,-3f,-2f),
							Size = new Vector3(12f, 13f, 18f),
							Uv = new Vector2(0f, 91f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "mouth",
					Parent = "head",
					Pivot = new Vector3(0f,15f,-10f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-8f,13f,-24f),
							Size = new Vector3(16f, 3f, 16f),
							Uv = new Vector2(0f, 36f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "neck",
					Parent = "",
					Pivot = new Vector3(0f,20f,-20f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,21f,-10f),
							Size = new Vector3(10f, 10f, 18f),
							Uv = new Vector2(68f, 73f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "head",
					Parent = "neck",
					Pivot = new Vector3(0f,28f,-10f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-8f,14f,-24f),
							Size = new Vector3(16f, 20f, 16f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,12f,-28f),
							Size = new Vector3(4f, 8f, 4f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg0",
					Parent = "",
					Pivot = new Vector3(-12f,30f,22f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-12f,0f,17f),
							Size = new Vector3(8f, 37f, 8f),
							Uv = new Vector2(96f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg1",
					Parent = "",
					Pivot = new Vector3(4f,30f,22f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(4f,0f,17f),
							Size = new Vector3(8f, 37f, 8f),
							Uv = new Vector2(96f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg2",
					Parent = "",
					Pivot = new Vector3(-4f,26f,-4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-12f,0f,-8f),
							Size = new Vector3(8f, 37f, 8f),
							Uv = new Vector2(64f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg3",
					Parent = "",
					Pivot = new Vector3(-4f,26f,-4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(4f,0f,-8f),
							Size = new Vector3(8f, 37f, 8f),
							Uv = new Vector2(64f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "horns",
					Parent = "head",
					Pivot = new Vector3(-5f,27f,-19f),
					Rotation = new Vector3(60f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-10f,27f,-20f),
							Size = new Vector3(2f, 14f, 4f),
							Uv = new Vector2(74f, 55f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(8f,27f,-20f),
							Size = new Vector3(2f, 14f, 4f),
							Uv = new Vector2(74f, 55f)
						},
					}
				},
			};
		}

	}

}