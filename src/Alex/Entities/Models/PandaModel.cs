



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class PandaModel : EntityModel
	{
		public PandaModel()
		{
			Name = "geometry.panda";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 64;
			Textureheight = 64;
			Bones = new EntityModelBone[6]
			{
				new EntityModelBone(){ 
					Name = "head",
					Parent = "body",
					Pivot = new Vector3(0f,12.5f,-17f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[4]{
						new EntityModelCube()
						{
							Origin = new Vector3(-6.5f,7.5f,-21f),
							Size = new Vector3(13f, 10f, 9f),
							Uv = new Vector2(0f, 6f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-3.5f,7.5f,-23f),
							Size = new Vector3(7f, 5f, 2f),
							Uv = new Vector2(45f, 16f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-8.5f,16.5f,-18f),
							Size = new Vector3(5f, 4f, 1f),
							Uv = new Vector2(52f, 25f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(3.5f,16.5f,-18f),
							Size = new Vector3(5f, 4f, 1f),
							Uv = new Vector2(52f, 25f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,14f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(90f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-9.5f,1f,-6.5f),
							Size = new Vector3(19f, 26f, 13f),
							Uv = new Vector2(0f, 25f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg0",
					Parent = "body",
					Pivot = new Vector3(-5.5f,9f,9f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-8.5f,0f,6f),
							Size = new Vector3(6f, 9f, 6f),
							Uv = new Vector2(40f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg1",
					Parent = "body",
					Pivot = new Vector3(5.5f,9f,9f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2.5f,0f,6f),
							Size = new Vector3(6f, 9f, 6f),
							Uv = new Vector2(40f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg2",
					Parent = "body",
					Pivot = new Vector3(-5.5f,9f,-9f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-8.5f,0f,-12f),
							Size = new Vector3(6f, 9f, 6f),
							Uv = new Vector2(40f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg3",
					Parent = "body",
					Pivot = new Vector3(5.5f,9f,-9f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2.5f,0f,-12f),
							Size = new Vector3(6f, 9f, 6f),
							Uv = new Vector2(40f, 0f)
						},
					}
				},
			};
		}

	}

}