



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class IrongolemModel : EntityModel
	{
		public IrongolemModel()
		{
			Name = "geometry.irongolem";
			VisibleBoundsWidth = 3;
			VisibleBoundsHeight = 3;
			VisibleBoundsOffset = new Vector3(0f, 1.5f, 0f);
			Texturewidth = 128;
			Textureheight = 128;
			Bones = new EntityModelBone[6]
			{
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,31f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-9f,21f,-6f),
							Size = new Vector3(18f, 12f, 11f),
							Uv = new Vector2(0f, 40f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-4.5f,16f,-3f),
							Size = new Vector3(9f, 5f, 6f),
							Uv = new Vector2(0f, 70f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "head",
					Parent = "body",
					Pivot = new Vector3(0f,31f,-2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,33f,-7.5f),
							Size = new Vector3(8f, 10f, 8f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,32f,-9.5f),
							Size = new Vector3(2f, 4f, 2f),
							Uv = new Vector2(24f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "arm0",
					Parent = "body",
					Pivot = new Vector3(0f,31f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-13f,3.5f,-3f),
							Size = new Vector3(4f, 30f, 6f),
							Uv = new Vector2(60f, 21f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "arm1",
					Parent = "body",
					Pivot = new Vector3(0f,31f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(9f,3.5f,-3f),
							Size = new Vector3(4f, 30f, 6f),
							Uv = new Vector2(60f, 58f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg0",
					Parent = "body",
					Pivot = new Vector3(-4f,13f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-7.5f,0f,-3f),
							Size = new Vector3(6f, 16f, 5f),
							Uv = new Vector2(37f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg1",
					Parent = "body",
					Pivot = new Vector3(5f,13f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1.5f,0f,-3f),
							Size = new Vector3(6f, 16f, 5f),
							Uv = new Vector2(60f, 0f)
						},
					}
				},
			};
		}

	}

}