



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class ChickenV112Model : EntityModel
	{
		public ChickenV112Model()
		{
			Name = "geometry.chicken.v1.12";
			VisibleBoundsWidth = 1.5;
			VisibleBoundsHeight = 1.5;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 64;
			Textureheight = 32;
			Bones = new EntityModelBone[8]
			{
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
							Origin = new Vector3(-3f,4f,-3f),
							Size = new Vector3(6f, 8f, 6f),
							Uv = new Vector2(0f, 9f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "head",
					Parent = "",
					Pivot = new Vector3(0f,9f,-4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,9f,-6f),
							Size = new Vector3(4f, 6f, 3f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "comb",
					Parent = "head",
					Pivot = new Vector3(0f,9f,-4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,9f,-7f),
							Size = new Vector3(2f, 2f, 2f),
							Uv = new Vector2(14f, 4f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "beak",
					Parent = "head",
					Pivot = new Vector3(0f,9f,-4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,11f,-8f),
							Size = new Vector3(4f, 2f, 2f),
							Uv = new Vector2(14f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg0",
					Parent = "",
					Pivot = new Vector3(-2f,5f,1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,0f,-2f),
							Size = new Vector3(3f, 5f, 3f),
							Uv = new Vector2(26f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg1",
					Parent = "",
					Pivot = new Vector3(1f,5f,1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,0f,-2f),
							Size = new Vector3(3f, 5f, 3f),
							Uv = new Vector2(26f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "wing0",
					Parent = "",
					Pivot = new Vector3(-3f,11f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,7f,-3f),
							Size = new Vector3(1f, 4f, 6f),
							Uv = new Vector2(24f, 13f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "wing1",
					Parent = "",
					Pivot = new Vector3(3f,11f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(3f,7f,-3f),
							Size = new Vector3(1f, 4f, 6f),
							Uv = new Vector2(24f, 13f)
						},
					}
				},
			};
		}

	}

}