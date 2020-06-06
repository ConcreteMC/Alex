



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class BatModel : EntityModel
	{
		public BatModel()
		{
			Name = "geometry.bat";
			VisibleBoundsWidth = 1;
			VisibleBoundsHeight = 1;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 0;
			Textureheight = 0;
			Bones = new EntityModelBone[8]
			{
				new EntityModelBone(){ 
					Name = "head",
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
							Origin = new Vector3(-3f,21f,-3f),
							Size = new Vector3(6f, 6f, 6f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightEar",
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
							Origin = new Vector3(-4f,26f,-2f),
							Size = new Vector3(3f, 4f, 1f),
							Uv = new Vector2(24f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftEar",
					Parent = "head",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1f,26f,-2f),
							Size = new Vector3(3f, 4f, 1f),
							Uv = new Vector2(24f, 0f)
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
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,8f,-3f),
							Size = new Vector3(6f, 12f, 6f),
							Uv = new Vector2(0f, 16f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,-8f,0f),
							Size = new Vector3(10f, 16f, 1f),
							Uv = new Vector2(0f, 34f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightWing",
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
							Origin = new Vector3(-12f,7f,1.5f),
							Size = new Vector3(10f, 16f, 1f),
							Uv = new Vector2(42f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightWingTip",
					Parent = "rightWing",
					Pivot = new Vector3(-12f,23f,1.5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-20f,10f,1.5f),
							Size = new Vector3(8f, 12f, 1f),
							Uv = new Vector2(24f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftWing",
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
							Origin = new Vector3(2f,7f,1.5f),
							Size = new Vector3(10f, 16f, 1f),
							Uv = new Vector2(42f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftWingTip",
					Parent = "leftWing",
					Pivot = new Vector3(12f,23f,1.5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(12f,10f,1.5f),
							Size = new Vector3(8f, 12f, 1f),
							Uv = new Vector2(24f, 16f)
						},
					}
				},
			};
		}

	}

}