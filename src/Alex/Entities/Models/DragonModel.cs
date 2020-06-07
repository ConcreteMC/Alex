



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class DragonModel : EntityModel
	{
		public DragonModel()
		{
			Name = "geometry.dragon";
			VisibleBoundsWidth = 14;
			VisibleBoundsHeight = 13;
			VisibleBoundsOffset = new Vector3(0f, 2f, 0f);
			Texturewidth = 256;
			Textureheight = 256;
			Bones = new EntityModelBone[20]
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
					Cubes = new EntityModelCube[6]{
						new EntityModelCube()
						{
							Origin = new Vector3(-6f,20f,-24f),
							Size = new Vector3(12f, 5f, 16f),
							Uv = new Vector2(176f, 44f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-8f,16f,-10f),
							Size = new Vector3(16f, 16f, 16f),
							Uv = new Vector2(112f, 30f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,32f,-4f),
							Size = new Vector3(2f, 4f, 6f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,25f,-22f),
							Size = new Vector3(2f, 2f, 4f),
							Uv = new Vector2(112f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(3f,32f,-4f),
							Size = new Vector3(2f, 4f, 6f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(3f,25f,-22f),
							Size = new Vector3(2f, 2f, 4f),
							Uv = new Vector2(112f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "jaw",
					Parent = "",
					Pivot = new Vector3(0f,20f,-8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-6f,16f,-24f),
							Size = new Vector3(12f, 4f, 16f),
							Uv = new Vector2(176f, 65f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "neck",
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
							Origin = new Vector3(-5f,19f,-5f),
							Size = new Vector3(10f, 10f, 10f),
							Uv = new Vector2(192f, 104f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,29f,-3f),
							Size = new Vector3(2f, 4f, 6f),
							Uv = new Vector2(48f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,20f,8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[4]{
						new EntityModelCube()
						{
							Origin = new Vector3(-12f,-4f,-8f),
							Size = new Vector3(24f, 24f, 64f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,20f,-2f),
							Size = new Vector3(2f, 6f, 12f),
							Uv = new Vector2(220f, 53f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,20f,18f),
							Size = new Vector3(2f, 6f, 12f),
							Uv = new Vector2(220f, 53f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,20f,38f),
							Size = new Vector3(2f, 6f, 12f),
							Uv = new Vector2(220f, 53f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "wing",
					Parent = "",
					Pivot = new Vector3(-12f,19f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-68f,15f,-2f),
							Size = new Vector3(56f, 8f, 8f),
							Uv = new Vector2(112f, 88f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-68f,19f,4f),
							Size = new Vector3(56f, 0f, 56f),
							Uv = new Vector2(-56f, 88f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "wingtip",
					Parent = "",
					Pivot = new Vector3(-56f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-112f,22f,-2f),
							Size = new Vector3(56f, 4f, 4f),
							Uv = new Vector2(112f, 136f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-112f,24f,2f),
							Size = new Vector3(56f, 0f, 56f),
							Uv = new Vector2(-56f, 144f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "wing1",
					Parent = "",
					Pivot = new Vector3(12f,19f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-44f,15f,-2f),
							Size = new Vector3(56f, 8f, 8f),
							Uv = new Vector2(112f, 88f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-44f,19f,4f),
							Size = new Vector3(56f, 0f, 56f),
							Uv = new Vector2(-56f, 88f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "wingtip1",
					Parent = "",
					Pivot = new Vector3(-56f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-112f,22f,-2f),
							Size = new Vector3(56f, 4f, 4f),
							Uv = new Vector2(112f, 136f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-112f,24f,2f),
							Size = new Vector3(56f, 0f, 56f),
							Uv = new Vector2(-56f, 144f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rearleg",
					Parent = "",
					Pivot = new Vector3(-16f,8f,42f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-24f,-20f,34f),
							Size = new Vector3(16f, 32f, 16f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rearleg1",
					Parent = "",
					Pivot = new Vector3(16f,8f,42f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(8f,-20f,34f),
							Size = new Vector3(16f, 32f, 16f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "frontleg",
					Parent = "",
					Pivot = new Vector3(-12f,4f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-16f,-16f,-2f),
							Size = new Vector3(8f, 24f, 8f),
							Uv = new Vector2(112f, 104f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "frontleg1",
					Parent = "",
					Pivot = new Vector3(12f,4f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(8f,-16f,-2f),
							Size = new Vector3(8f, 24f, 8f),
							Uv = new Vector2(112f, 104f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rearlegtip",
					Parent = "",
					Pivot = new Vector3(0f,-8f,-4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-6f,-38f,-4f),
							Size = new Vector3(12f, 32f, 12f),
							Uv = new Vector2(196f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rearlegtip1",
					Parent = "",
					Pivot = new Vector3(0f,-8f,-4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-6f,-38f,-4f),
							Size = new Vector3(12f, 32f, 12f),
							Uv = new Vector2(196f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "frontlegtip",
					Parent = "",
					Pivot = new Vector3(0f,4f,-1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,-19f,-4f),
							Size = new Vector3(6f, 24f, 6f),
							Uv = new Vector2(226f, 138f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "frontlegtip1",
					Parent = "",
					Pivot = new Vector3(0f,4f,-1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,-19f,-4f),
							Size = new Vector3(6f, 24f, 6f),
							Uv = new Vector2(226f, 138f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rearfoot",
					Parent = "",
					Pivot = new Vector3(0f,-7f,4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-9f,-13f,-16f),
							Size = new Vector3(18f, 6f, 24f),
							Uv = new Vector2(112f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rearfoot1",
					Parent = "",
					Pivot = new Vector3(0f,-7f,4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-9f,-13f,-16f),
							Size = new Vector3(18f, 6f, 24f),
							Uv = new Vector2(112f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "frontfoot",
					Parent = "",
					Pivot = new Vector3(0f,1f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,-3f,-12f),
							Size = new Vector3(8f, 4f, 16f),
							Uv = new Vector2(144f, 104f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "frontfoot1",
					Parent = "",
					Pivot = new Vector3(0f,1f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,-3f,-12f),
							Size = new Vector3(8f, 4f, 16f),
							Uv = new Vector2(144f, 104f)
						},
					}
				},
			};
		}

	}

}