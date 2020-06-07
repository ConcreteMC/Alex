



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class VillagerV2Model : EntityModel
	{
		public VillagerV2Model()
		{
			Name = "geometry.villager_v2";
			VisibleBoundsWidth = 2;
			VisibleBoundsHeight = 2;
			VisibleBoundsOffset = new Vector3(0f, 1.25f, 0f);
			Texturewidth = 0;
			Textureheight = 0;
			Bones = new EntityModelBone[9]
			{
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
					Name = "helmet",
					Parent = "head",
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
							Uv = new Vector2(32f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "brim",
					Parent = "head",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(-90f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-8f,16f,-6f),
							Size = new Vector3(16f, 16f, 1f),
							Uv = new Vector2(30f, 47f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "nose",
					Parent = "head",
					Pivot = new Vector3(0f,26f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,23f,-6f),
							Size = new Vector3(2f, 4f, 2f),
							Uv = new Vector2(24f, 0f)
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
					Name = "held_item",
					Parent = "arms",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[0]
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