



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class HorseModel : EntityModel
	{
		public HorseModel()
		{
			Name = "geometry.horse";
			VisibleBoundsWidth = 2;
			VisibleBoundsHeight = 3;
			VisibleBoundsOffset = new Vector3(0f, 1f, 0f);
			Texturewidth = 128;
			Textureheight = 128;
			Bones = new EntityModelBone[39]
			{
				new EntityModelBone(){ 
					Name = "Body",
					Parent = "",
					Pivot = new Vector3(0f,13f,9f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,11f,-10f),
							Size = new Vector3(10f, 10f, 24f),
							Uv = new Vector2(0f, 34f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "TailA",
					Parent = "",
					Pivot = new Vector3(0f,21f,14f),
					Rotation = new Vector3(-65f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,20f,14f),
							Size = new Vector3(2f, 2f, 3f),
							Uv = new Vector2(44f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "TailB",
					Parent = "",
					Pivot = new Vector3(0f,21f,14f),
					Rotation = new Vector3(-65f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,19f,17f),
							Size = new Vector3(3f, 4f, 7f),
							Uv = new Vector2(38f, 7f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "TailC",
					Parent = "",
					Pivot = new Vector3(0f,21f,14f),
					Rotation = new Vector3(-80.34f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,21.5f,23f),
							Size = new Vector3(3f, 4f, 7f),
							Uv = new Vector2(24f, 3f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Leg1A",
					Parent = "",
					Pivot = new Vector3(4f,15f,11f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1.5f,8f,8.5f),
							Size = new Vector3(4f, 9f, 5f),
							Uv = new Vector2(78f, 29f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Leg1B",
					Parent = "",
					Pivot = new Vector3(4f,8f,11f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2f,3f,9.5f),
							Size = new Vector3(3f, 5f, 3f),
							Uv = new Vector2(78f, 43f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Leg1C",
					Parent = "",
					Pivot = new Vector3(4f,8f,11f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1.5f,-0.1f,9f),
							Size = new Vector3(4f, 3f, 4f),
							Uv = new Vector2(78f, 51f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Leg2A",
					Parent = "",
					Pivot = new Vector3(-4f,15f,11f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5.5f,8f,8.5f),
							Size = new Vector3(4f, 9f, 5f),
							Uv = new Vector2(96f, 29f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Leg2B",
					Parent = "",
					Pivot = new Vector3(-4f,8f,11f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,3f,9.5f),
							Size = new Vector3(3f, 5f, 3f),
							Uv = new Vector2(96f, 43f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Leg2C",
					Parent = "",
					Pivot = new Vector3(-4f,8f,11f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5.5f,-0.1f,9f),
							Size = new Vector3(4f, 3f, 4f),
							Uv = new Vector2(96f, 51f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Leg3A",
					Parent = "",
					Pivot = new Vector3(4f,15f,-8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2.1f,8f,-10.1f),
							Size = new Vector3(3f, 8f, 4f),
							Uv = new Vector2(44f, 29f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Leg3B",
					Parent = "",
					Pivot = new Vector3(4f,8f,-8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2.1f,3f,-9.6f),
							Size = new Vector3(3f, 5f, 3f),
							Uv = new Vector2(44f, 41f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Leg3C",
					Parent = "",
					Pivot = new Vector3(4f,8f,-8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1.6f,-0.1f,-10.1f),
							Size = new Vector3(4f, 3f, 4f),
							Uv = new Vector2(44f, 51f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Leg4A",
					Parent = "",
					Pivot = new Vector3(-4f,15f,-8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5.1f,8f,-10.1f),
							Size = new Vector3(3f, 8f, 4f),
							Uv = new Vector2(60f, 29f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Leg4B",
					Parent = "",
					Pivot = new Vector3(-4f,8f,-8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5.1f,3f,-9.6f),
							Size = new Vector3(3f, 5f, 3f),
							Uv = new Vector2(60f, 41f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Leg4C",
					Parent = "",
					Pivot = new Vector3(-4f,8f,-8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5.6f,-0.1f,-10.1f),
							Size = new Vector3(4f, 3f, 4f),
							Uv = new Vector2(60f, 51f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Head",
					Parent = "",
					Pivot = new Vector3(0f,20f,-10f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.5f,25f,-11.5f),
							Size = new Vector3(5f, 5f, 7f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "UMouth",
					Parent = "",
					Pivot = new Vector3(0f,20.05f,-10f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,27.05f,-17f),
							Size = new Vector3(4f, 3f, 6f),
							Uv = new Vector2(24f, 18f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "LMouth",
					Parent = "",
					Pivot = new Vector3(0f,20f,-10f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,25f,-16.5f),
							Size = new Vector3(4f, 2f, 5f),
							Uv = new Vector2(24f, 27f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Ear1",
					Parent = "",
					Pivot = new Vector3(0f,20f,-10f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0.45f,29f,-6f),
							Size = new Vector3(2f, 3f, 1f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Ear2",
					Parent = "",
					Pivot = new Vector3(0f,20f,-10f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.45f,29f,-6f),
							Size = new Vector3(2f, 3f, 1f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "MuleEarL",
					Parent = "",
					Pivot = new Vector3(0f,20f,-10f),
					Rotation = new Vector3(30f,0f,15f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,29f,-6f),
							Size = new Vector3(2f, 7f, 1f),
							Uv = new Vector2(0f, 12f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "MuleEarR",
					Parent = "",
					Pivot = new Vector3(0f,20f,-10f),
					Rotation = new Vector3(30f,0f,-15f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,29f,-6f),
							Size = new Vector3(2f, 7f, 1f),
							Uv = new Vector2(0f, 12f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Neck",
					Parent = "",
					Pivot = new Vector3(0f,20f,-10f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.05f,15.8f,-12f),
							Size = new Vector3(4f, 14f, 8f),
							Uv = new Vector2(0f, 12f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Bag1",
					Parent = "",
					Pivot = new Vector3(-7.5f,21f,10f),
					Rotation = new Vector3(0f,90f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-10.5f,13f,10f),
							Size = new Vector3(8f, 8f, 3f),
							Uv = new Vector2(0f, 34f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Bag2",
					Parent = "",
					Pivot = new Vector3(4.5f,21f,10f),
					Rotation = new Vector3(0f,90f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1.5f,13f,10f),
							Size = new Vector3(8f, 8f, 3f),
							Uv = new Vector2(0f, 47f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Saddle",
					Parent = "",
					Pivot = new Vector3(0f,22f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,21f,-1f),
							Size = new Vector3(10f, 1f, 8f),
							Uv = new Vector2(80f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "SaddleB",
					Parent = "",
					Pivot = new Vector3(0f,22f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,22f,-1f),
							Size = new Vector3(3f, 1f, 2f),
							Uv = new Vector2(106f, 9f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "SaddleC",
					Parent = "",
					Pivot = new Vector3(0f,22f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,22f,5f),
							Size = new Vector3(8f, 1f, 2f),
							Uv = new Vector2(80f, 9f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "SaddleL2",
					Parent = "",
					Pivot = new Vector3(5f,21f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(4.5f,13f,1f),
							Size = new Vector3(1f, 2f, 2f),
							Uv = new Vector2(74f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "SaddleL",
					Parent = "",
					Pivot = new Vector3(5f,21f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(4.5f,15f,1.5f),
							Size = new Vector3(1f, 6f, 1f),
							Uv = new Vector2(70f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "SaddleR2",
					Parent = "",
					Pivot = new Vector3(-5f,21f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5.5f,13f,1f),
							Size = new Vector3(1f, 2f, 2f),
							Uv = new Vector2(74f, 4f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "SaddleR",
					Parent = "",
					Pivot = new Vector3(-5f,21f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5.5f,15f,1.5f),
							Size = new Vector3(1f, 6f, 1f),
							Uv = new Vector2(80f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "SaddleMouthL",
					Parent = "",
					Pivot = new Vector3(0f,20f,-10f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1.5f,26f,-14f),
							Size = new Vector3(1f, 2f, 2f),
							Uv = new Vector2(74f, 13f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "SaddleMouthR",
					Parent = "",
					Pivot = new Vector3(0f,20f,-10f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.5f,26f,-14f),
							Size = new Vector3(1f, 2f, 2f),
							Uv = new Vector2(74f, 13f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "SaddleMouthLine",
					Parent = "",
					Pivot = new Vector3(0f,20f,-10f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2.6f,23f,-16f),
							Size = new Vector3(0f, 3f, 16f),
							Uv = new Vector2(44f, 10f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "SaddleMouthLineR",
					Parent = "",
					Pivot = new Vector3(0f,20f,-10f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.6f,23f,-16f),
							Size = new Vector3(0f, 3f, 16f),
							Uv = new Vector2(44f, 5f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Mane",
					Parent = "",
					Pivot = new Vector3(0f,20f,-10f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,15.5f,-5f),
							Size = new Vector3(2f, 16f, 4f),
							Uv = new Vector2(58f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "HeadSaddle",
					Parent = "",
					Pivot = new Vector3(0f,20f,-10f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.5f,25.1f,-17f),
							Size = new Vector3(5f, 5f, 12f),
							Uv = new Vector2(80f, 12f)
						},
					}
				},
			};
		}

	}

}