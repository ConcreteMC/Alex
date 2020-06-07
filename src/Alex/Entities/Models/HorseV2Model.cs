



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class HorseV2Model : EntityModel
	{
		public HorseV2Model()
		{
			Name = "geometry.horse.v2";
			VisibleBoundsWidth = 2;
			VisibleBoundsHeight = 3;
			VisibleBoundsOffset = new Vector3(0f, 1f, 0f);
			Texturewidth = 64;
			Textureheight = 64;
			Bones = new EntityModelBone[22]
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
							Origin = new Vector3(-5f,11f,-11f),
							Size = new Vector3(10f, 10f, 22f),
							Uv = new Vector2(0f, 32f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "TailA",
					Parent = "",
					Pivot = new Vector3(0f,20f,11f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,6f,9f),
							Size = new Vector3(3f, 14f, 4f),
							Uv = new Vector2(42f, 36f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Leg1A",
					Parent = "",
					Pivot = new Vector3(3f,11f,9f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1f,0f,7f),
							Size = new Vector3(4f, 11f, 4f),
							Uv = new Vector2(48f, 21f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Leg2A",
					Parent = "",
					Pivot = new Vector3(-3f,11f,9f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,0f,7f),
							Size = new Vector3(4f, 11f, 4f),
							Uv = new Vector2(48f, 21f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Leg3A",
					Parent = "",
					Pivot = new Vector3(3f,11f,-9f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1f,0f,-11f),
							Size = new Vector3(4f, 11f, 4f),
							Uv = new Vector2(48f, 21f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Leg4A",
					Parent = "",
					Pivot = new Vector3(-3f,11f,-9f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,0f,-11f),
							Size = new Vector3(4f, 11f, 4f),
							Uv = new Vector2(48f, 21f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Head",
					Parent = "",
					Pivot = new Vector3(0f,28f,-11f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,28f,-11f),
							Size = new Vector3(6f, 5f, 7f),
							Uv = new Vector2(0f, 13f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "UMouth",
					Parent = "",
					Pivot = new Vector3(0f,28f,-11f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,28f,-16f),
							Size = new Vector3(4f, 5f, 5f),
							Uv = new Vector2(0f, 25f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Ear1",
					Parent = "",
					Pivot = new Vector3(0f,17f,-8f),
					Rotation = new Vector3(30f,0f,5f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-0.5f,32f,-5.01f),
							Size = new Vector3(2f, 3f, 1f),
							Uv = new Vector2(19f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Ear2",
					Parent = "",
					Pivot = new Vector3(0f,17f,-8f),
					Rotation = new Vector3(30f,0f,-5f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,32f,-5.01f),
							Size = new Vector3(2f, 3f, 1f),
							Uv = new Vector2(19f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "MuleEarL",
					Parent = "",
					Pivot = new Vector3(0f,17f,-8f),
					Rotation = new Vector3(30f,0f,15f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,32f,-5.01f),
							Size = new Vector3(2f, 7f, 1f),
							Uv = new Vector2(0f, 12f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "MuleEarR",
					Parent = "",
					Pivot = new Vector3(0f,17f,-8f),
					Rotation = new Vector3(30f,0f,-15f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1f,32f,-5.01f),
							Size = new Vector3(2f, 7f, 1f),
							Uv = new Vector2(0f, 12f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Neck",
					Parent = "",
					Pivot = new Vector3(0f,17f,-8f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,16f,-11f),
							Size = new Vector3(4f, 12f, 7f),
							Uv = new Vector2(0f, 35f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Bag1",
					Parent = "",
					Pivot = new Vector3(-5f,21f,11f),
					Rotation = new Vector3(0f,-90f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-14f,13f,11f),
							Size = new Vector3(8f, 8f, 3f),
							Uv = new Vector2(26f, 21f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Bag2",
					Parent = "",
					Pivot = new Vector3(5f,21f,11f),
					Rotation = new Vector3(0f,90f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(6f,13f,11f),
							Size = new Vector3(8f, 8f, 3f),
							Uv = new Vector2(26f, 21f)
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
							Origin = new Vector3(-5f,12f,-3.5f),
							Size = new Vector3(10f, 9f, 9f),
							Uv = new Vector2(26f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "SaddleMouthL",
					Parent = "",
					Pivot = new Vector3(0f,17f,-8f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2f,29f,-14f),
							Size = new Vector3(1f, 2f, 2f),
							Uv = new Vector2(29f, 5f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "SaddleMouthR",
					Parent = "",
					Pivot = new Vector3(0f,17f,-8f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,29f,-14f),
							Size = new Vector3(1f, 2f, 2f),
							Uv = new Vector2(29f, 5f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "SaddleMouthLine",
					Parent = "",
					Pivot = new Vector3(0f,17f,-8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(3.1f,24f,-19.5f),
							Size = new Vector3(0f, 3f, 16f),
							Uv = new Vector2(32f, 2f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "SaddleMouthLineR",
					Parent = "",
					Pivot = new Vector3(0f,17f,-8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3.1f,24f,-19.5f),
							Size = new Vector3(0f, 3f, 16f),
							Uv = new Vector2(32f, 2f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "Mane",
					Parent = "",
					Pivot = new Vector3(0f,17f,-8f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,17f,-4f),
							Size = new Vector3(2f, 16f, 2f),
							Uv = new Vector2(56f, 36f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "HeadSaddle",
					Parent = "",
					Pivot = new Vector3(0f,17f,-8f),
					Rotation = new Vector3(30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,28f,-13f),
							Size = new Vector3(4f, 5f, 2f),
							Uv = new Vector2(19f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,28f,-11f),
							Size = new Vector3(6f, 5f, 7f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
			};
		}

	}

}