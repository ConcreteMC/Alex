



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class TurtleModel : EntityModel
	{
		public TurtleModel()
		{
			Name = "geometry.turtle";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 128;
			Textureheight = 64;
			Bones = new EntityModelBone[7]
			{
				new EntityModelBone(){ 
					Name = "head",
					Parent = "body",
					Pivot = new Vector3(0f,5f,-10f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,1f,-13f),
							Size = new Vector3(6f, 5f, 6f),
							Uv = new Vector2(2f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "eggbelly",
					Parent = "body",
					Pivot = new Vector3(0f,13f,-10f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(90f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4.5f,-8f,-24f),
							Size = new Vector3(9f, 18f, 1f),
							Uv = new Vector2(69f, 33f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,13f,-10f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(90f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-9.5f,-10f,-20f),
							Size = new Vector3(19f, 20f, 6f),
							Uv = new Vector2(6f, 37f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-5.5f,-8f,-23f),
							Size = new Vector3(11f, 18f, 3f),
							Uv = new Vector2(30f, 1f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg0",
					Parent = "body",
					Pivot = new Vector3(-3.5f,2f,11f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5.5f,1f,11f),
							Size = new Vector3(4f, 1f, 10f),
							Uv = new Vector2(0f, 23f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg1",
					Parent = "body",
					Pivot = new Vector3(3.5f,2f,11f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1.5f,1f,11f),
							Size = new Vector3(4f, 1f, 10f),
							Uv = new Vector2(0f, 12f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg2",
					Parent = "body",
					Pivot = new Vector3(-5f,3f,-4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-18f,2f,-6f),
							Size = new Vector3(13f, 1f, 5f),
							Uv = new Vector2(26f, 30f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg3",
					Parent = "body",
					Pivot = new Vector3(5f,3f,-4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(5f,2f,-6f),
							Size = new Vector3(13f, 1f, 5f),
							Uv = new Vector2(26f, 24f)
						},
					}
				},
			};
		}

	}

}