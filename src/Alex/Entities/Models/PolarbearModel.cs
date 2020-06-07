



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class PolarbearModel : EntityModel
	{
		public PolarbearModel()
		{
			Name = "geometry.polarbear";
			VisibleBoundsWidth = 3;
			VisibleBoundsHeight = 2;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 128;
			Textureheight = 64;
			Bones = new EntityModelBone[6]
			{
				new EntityModelBone(){ 
					Name = "head",
					Parent = "body",
					Pivot = new Vector3(0f,14f,-16f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[4]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3.5f,10f,-19f),
							Size = new Vector3(7f, 7f, 7f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-2.5f,10f,-22f),
							Size = new Vector3(5f, 3f, 3f),
							Uv = new Vector2(0f, 44f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-4.5f,16f,-17f),
							Size = new Vector3(2f, 2f, 1f),
							Uv = new Vector2(26f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(2.5f,16f,-17f),
							Size = new Vector3(2f, 2f, 1f),
							Uv = new Vector2(26f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(-2f,15f,12f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(90f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-7f,14f,5f),
							Size = new Vector3(14f, 14f, 11f),
							Uv = new Vector2(0f, 19f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-6f,28f,5f),
							Size = new Vector3(12f, 12f, 10f),
							Uv = new Vector2(39f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg0",
					Parent = "body",
					Pivot = new Vector3(-4.5f,10f,6f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-6.5f,0f,4f),
							Size = new Vector3(4f, 10f, 8f),
							Uv = new Vector2(50f, 22f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg1",
					Parent = "body",
					Pivot = new Vector3(4.5f,10f,6f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2.5f,0f,4f),
							Size = new Vector3(4f, 10f, 8f),
							Uv = new Vector2(50f, 22f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg2",
					Parent = "body",
					Pivot = new Vector3(-3.5f,10f,-8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5.5f,0f,-10f),
							Size = new Vector3(4f, 10f, 6f),
							Uv = new Vector2(50f, 40f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg3",
					Parent = "body",
					Pivot = new Vector3(3.5f,10f,-8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1.5f,0f,-10f),
							Size = new Vector3(4f, 10f, 6f),
							Uv = new Vector2(50f, 40f)
						},
					}
				},
			};
		}

	}

}