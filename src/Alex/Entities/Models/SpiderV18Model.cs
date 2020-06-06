



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class SpiderV18Model : EntityModel
	{
		public SpiderV18Model()
		{
			Name = "geometry.spider.v1.8";
			VisibleBoundsWidth = 2;
			VisibleBoundsHeight = 1;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 64;
			Textureheight = 32;
			Bones = new EntityModelBone[11]
			{
				new EntityModelBone(){ 
					Name = "head",
					Parent = "body0",
					Pivot = new Vector3(0f,9f,-3f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,5f,-11f),
							Size = new Vector3(8f, 8f, 8f),
							Uv = new Vector2(32f, 4f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body0",
					Parent = "",
					Pivot = new Vector3(0f,9f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,6f,-3f),
							Size = new Vector3(6f, 6f, 6f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body1",
					Parent = "body0",
					Pivot = new Vector3(0f,9f,9f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,5f,3f),
							Size = new Vector3(10f, 8f, 12f),
							Uv = new Vector2(0f, 12f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg0",
					Parent = "body0",
					Pivot = new Vector3(-4f,9f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-19f,8f,1f),
							Size = new Vector3(16f, 2f, 2f),
							Uv = new Vector2(18f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg1",
					Parent = "body0",
					Pivot = new Vector3(4f,9f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(3f,8f,1f),
							Size = new Vector3(16f, 2f, 2f),
							Uv = new Vector2(18f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg2",
					Parent = "body0",
					Pivot = new Vector3(-4f,9f,1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-19f,8f,0f),
							Size = new Vector3(16f, 2f, 2f),
							Uv = new Vector2(18f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg3",
					Parent = "body0",
					Pivot = new Vector3(4f,9f,1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(3f,8f,0f),
							Size = new Vector3(16f, 2f, 2f),
							Uv = new Vector2(18f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg4",
					Parent = "body0",
					Pivot = new Vector3(-4f,9f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-19f,8f,-1f),
							Size = new Vector3(16f, 2f, 2f),
							Uv = new Vector2(18f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg5",
					Parent = "body0",
					Pivot = new Vector3(4f,9f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(3f,8f,-1f),
							Size = new Vector3(16f, 2f, 2f),
							Uv = new Vector2(18f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg6",
					Parent = "body0",
					Pivot = new Vector3(-4f,9f,-1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-19f,8f,-2f),
							Size = new Vector3(16f, 2f, 2f),
							Uv = new Vector2(18f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg7",
					Parent = "body0",
					Pivot = new Vector3(4f,9f,-1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(3f,8f,-2f),
							Size = new Vector3(16f, 2f, 2f),
							Uv = new Vector2(18f, 0f)
						},
					}
				},
			};
		}

	}

}