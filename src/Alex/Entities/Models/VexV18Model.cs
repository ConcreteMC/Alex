



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class VexV18Model : EntityModel
	{
		public VexV18Model()
		{
			Name = "geometry.vex.v1.8";
			VisibleBoundsWidth = 2;
			VisibleBoundsHeight = 1;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 64;
			Textureheight = 64;
			Bones = new EntityModelBone[8]
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
							Size = new Vector3(8f, 8f, 8f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
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
					Name = "rightArm",
					Parent = "body",
					Pivot = new Vector3(-5f,22f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-8f,12f,-2f),
							Size = new Vector3(4f, 12f, 4f),
							Uv = new Vector2(40f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightItem",
					Parent = "rightarm",
					Pivot = new Vector3(-6f,13f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = true,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[0]
				},
				new EntityModelBone(){ 
					Name = "leftArm",
					Parent = "body",
					Pivot = new Vector3(5f,22f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(4f,12f,-2f),
							Size = new Vector3(4f, 12f, 4f),
							Uv = new Vector2(40f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg0",
					Parent = "body",
					Pivot = new Vector3(-1.9f,12f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3.9f,0f,-2f),
							Size = new Vector3(4f, 12f, 4f),
							Uv = new Vector2(0f, 16f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-2.9f,3f,-2f),
							Size = new Vector3(6f, 10f, 4f),
							Uv = new Vector2(32f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftwing",
					Parent = "body",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,12f,0f),
							Size = new Vector3(20f, 12f, 1f),
							Uv = new Vector2(0f, 32f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightwing",
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
							Origin = new Vector3(-20f,12f,0f),
							Size = new Vector3(20f, 12f, 1f),
							Uv = new Vector2(0f, 32f)
						},
					}
				},
			};
		}

	}

}