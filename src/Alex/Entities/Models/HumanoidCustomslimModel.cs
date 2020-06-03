


using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class HumanoidCustomslimModel : EntityModel
	{
		public HumanoidCustomslimModel()
		{
			Name = "geometry.humanoid.customSlim";
			VisibleBoundsWidth = 1;
			VisibleBoundsHeight = 2;
			VisibleBoundsOffset = new Vector3(0f, 1f, 0f);
			Texturewidth = 64;
			Textureheight = 64;
			Bones = new EntityModelBone[15]
			{
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,12f,-2f),
							Size = new Vector3(8f, 12f, 4f),
							Uv = new Vector2(16f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "waist",
					Parent = "body",
					Pivot = new Vector3(0f,12f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = true,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[0]
				},
				new EntityModelBone(){ 
					Name = "head",
					Parent = "body",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,24f,-4f),
							Size = new Vector3(8f, 8f, 8f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightLeg",
					Parent = "body",
					Pivot = new Vector3(-1.9f,12f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3.9f,0f,-2f),
							Size = new Vector3(4f, 12f, 4f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftLeg",
					Parent = "body",
					Pivot = new Vector3(1.9f,12f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-0.1f,0f,-2f),
							Size = new Vector3(4f, 12f, 4f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "hat",
					Parent = "head",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[0]
				},
				new EntityModelBone(){ 
					Name = "leftArm",
					Parent = "body",
					Pivot = new Vector3(5f,21.5f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = true,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(4f,11.5f,-2f),
							Size = new Vector3(3f, 12f, 4f),
							Uv = new Vector2(32f, 48f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightArm",
					Parent = "body",
					Pivot = new Vector3(-5f,21.5f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = true,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-7f,11.5f,-2f),
							Size = new Vector3(3f, 12f, 4f),
							Uv = new Vector2(40f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightItem",
					Parent = "leftArm",
					Pivot = new Vector3(-6f,15f,1f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = true,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[0]
				},
				new EntityModelBone(){ 
					Name = "leftSleeve",
					Parent = "leftArm",
					Pivot = new Vector3(5f,21.5f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(4f,11.5f,-2f),
							Size = new Vector3(3f, 12f, 4f),
							Uv = new Vector2(48f, 48f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightSleeve",
					Parent = "rightArm",
					Pivot = new Vector3(-5f,21.5f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-7f,11.5f,-2f),
							Size = new Vector3(3f, 12f, 4f),
							Uv = new Vector2(40f, 32f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftLeg",
					Parent = "body",
					Pivot = new Vector3(1.9f,12f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = true,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-0.1f,0f,-2f),
							Size = new Vector3(4f, 12f, 4f),
							Uv = new Vector2(16f, 48f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftPants",
					Parent = "leftLeg",
					Pivot = new Vector3(1.9f,12f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-0.1f,0f,-2f),
							Size = new Vector3(4f, 12f, 4f),
							Uv = new Vector2(0f, 48f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightPants",
					Parent = "rightLeg",
					Pivot = new Vector3(-1.9f,12f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3.9f,0f,-2f),
							Size = new Vector3(4f, 12f, 4f),
							Uv = new Vector2(0f, 32f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "jacket",
					Parent = "body",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,12f,-2f),
							Size = new Vector3(8f, 12f, 4f),
							Uv = new Vector2(16f, 32f)
						},
					}
				},
			};
		}

	}

}