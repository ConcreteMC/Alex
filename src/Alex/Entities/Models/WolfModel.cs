



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class WolfModel : EntityModel
	{
		public WolfModel()
		{
			Name = "geometry.wolf";
			VisibleBoundsWidth = 2;
			VisibleBoundsHeight = 1;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 64;
			Textureheight = 32;
			Bones = new EntityModelBone[8]
			{
				new EntityModelBone(){ 
					Name = "head",
					Parent = "",
					Pivot = new Vector3(-1f,10.5f,-7f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[4]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,7.5f,-9f),
							Size = new Vector3(6f, 6f, 4f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,13.5f,-7f),
							Size = new Vector3(2f, 2f, 1f),
							Uv = new Vector2(16f, 14f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(0f,13.5f,-7f),
							Size = new Vector3(2f, 2f, 1f),
							Uv = new Vector2(16f, 14f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-2.5f,7.515625f,-12f),
							Size = new Vector3(3f, 3f, 4f),
							Uv = new Vector2(0f, 10f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,10f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,3f,-1f),
							Size = new Vector3(6f, 9f, 6f),
							Uv = new Vector2(18f, 14f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "upperBody",
					Parent = "",
					Pivot = new Vector3(-1f,10f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,7f,-1f),
							Size = new Vector3(8f, 6f, 7f),
							Uv = new Vector2(21f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg0",
					Parent = "",
					Pivot = new Vector3(-2.5f,8f,7f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3.5f,0f,6f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(0f, 18f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg1",
					Parent = "",
					Pivot = new Vector3(0.5f,8f,7f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-0.5f,0f,6f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(0f, 18f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg2",
					Parent = "",
					Pivot = new Vector3(-2.5f,8f,-4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3.5f,0f,-5f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(0f, 18f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg3",
					Parent = "",
					Pivot = new Vector3(0.5f,8f,-4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-0.5f,0f,-5f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(0f, 18f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tail",
					Parent = "",
					Pivot = new Vector3(-1f,12f,8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,4f,7f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(9f, 18f)
						},
					}
				},
			};
		}

	}

}