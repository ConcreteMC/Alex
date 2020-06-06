



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class VillagerWitchV18GeometryVillagerV18Model : EntityModel
	{
		public VillagerWitchV18GeometryVillagerV18Model()
		{
			Name = "geometry.villager.witch.v1.8:geometry.villager.v1.8";
			VisibleBoundsWidth = 2;
			VisibleBoundsHeight = 3;
			VisibleBoundsOffset = new Vector3(0f, 1.5f, 0f);
			Texturewidth = 64;
			Textureheight = 128;
			Bones = new EntityModelBone[10]
			{
				new EntityModelBone(){ 
					Name = "nose",
					Parent = "head",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,25f,-6.75f),
							Size = new Vector3(1f, 1f, 1f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,23f,-6f),
							Size = new Vector3(2f, 4f, 2f),
							Uv = new Vector2(24f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "hat",
					Parent = "head",
					Pivot = new Vector3(-5f,32.03125f,-5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,32.05f,-5f),
							Size = new Vector3(10f, 2f, 10f),
							Uv = new Vector2(0f, 64f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "hat2",
					Parent = "hat",
					Pivot = new Vector3(1.75f,32f,2f),
					Rotation = new Vector3(-3f,0f,1.5f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3.25f,33.5f,-3f),
							Size = new Vector3(7f, 4f, 7f),
							Uv = new Vector2(0f, 76f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "hat3",
					Parent = "hat2",
					Pivot = new Vector3(1.75f,35f,2f),
					Rotation = new Vector3(-6f,0f,3f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,36.5f,-1f),
							Size = new Vector3(4f, 4f, 4f),
							Uv = new Vector2(0f, 87f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "hat4",
					Parent = "hat3",
					Pivot = new Vector3(1.75f,38f,2f),
					Rotation = new Vector3(-12f,0f,6f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0.25f,40f,1f),
							Size = new Vector3(1f, 2f, 1f),
							Uv = new Vector2(0f, 95f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "head",
					Parent = "body",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,24f,-4f),
							Size = new Vector3(8f, 10f, 8f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,12f,-3f),
							Size = new Vector3(8f, 12f, 6f),
							Uv = new Vector2(16f, 20f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,6f,-3f),
							Size = new Vector3(8f, 18f, 6f),
							Uv = new Vector2(0f, 38f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "arms",
					Parent = "body",
					Pivot = new Vector3(0f,22f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[3]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,16f,-2f),
							Size = new Vector3(8f, 4f, 4f),
							Uv = new Vector2(40f, 38f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-8f,16f,-2f),
							Size = new Vector3(4f, 8f, 4f),
							Uv = new Vector2(44f, 22f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(4f,16f,-2f),
							Size = new Vector3(4f, 8f, 4f),
							Uv = new Vector2(44f, 22f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg0",
					Parent = "body",
					Pivot = new Vector3(-2f,12f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,0f,-2f),
							Size = new Vector3(4f, 12f, 4f),
							Uv = new Vector2(0f, 22f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg1",
					Parent = "body",
					Pivot = new Vector3(2f,12f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,0f,-2f),
							Size = new Vector3(4f, 12f, 4f),
							Uv = new Vector2(0f, 22f)
						},
					}
				},
			};
		}

	}

}