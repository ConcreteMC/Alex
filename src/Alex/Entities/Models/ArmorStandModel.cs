


using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class ArmorStandModel : EntityModel
	{
		public ArmorStandModel()
		{
			Name = "geometry.armor_stand";
			VisibleBoundsWidth = 2;
			VisibleBoundsHeight = 1;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 64;
			Textureheight = 64;
			Bones = new EntityModelBone[10]
			{
				new EntityModelBone(){ 
					Name = "body",
					Parent = "waist",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[4]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,0f,0f),
							Size = new Vector3(12f, 3f, 3f),
							Uv = new Vector2(0f, 26f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,14f,-1f),
							Size = new Vector3(2f, 7f, 2f),
							Uv = new Vector2(16f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(1f,14f,-1f),
							Size = new Vector3(2f, 7f, 2f),
							Uv = new Vector2(48f, 16f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,12f,-1f),
							Size = new Vector3(8f, 2f, 2f),
							Uv = new Vector2(0f, 48f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "waist",
					Parent = "basePlate",
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
							Origin = new Vector3(-1f,24f,-1f),
							Size = new Vector3(2f, 7f, 2f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "hat",
					Parent = "head",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = true,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,24f,-4f),
							Size = new Vector3(8f, 8f, 8f),
							Uv = new Vector2(32f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightArm",
					Parent = "body",
					Pivot = new Vector3(-5f,22f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-7f,12f,-1f),
							Size = new Vector3(2f, 12f, 2f),
							Uv = new Vector2(24f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightItem",
					Parent = "rightArm",
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
					Pivot = new Vector3(5f,22f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(5f,12f,-1f),
							Size = new Vector3(2f, 12f, 2f),
							Uv = new Vector2(32f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightLeg",
					Parent = "body",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,0f,0f),
							Size = new Vector3(2f, 11f, 2f),
							Uv = new Vector2(8f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftLeg",
					Parent = "body",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,0f,0f),
							Size = new Vector3(2f, 11f, 2f),
							Uv = new Vector2(40f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "basePlate",
					Parent = "",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-6f,0f,-6f),
							Size = new Vector3(12f, 1f, 12f),
							Uv = new Vector2(0f, 32f)
						},
					}
				},
			};
		}

	}

}