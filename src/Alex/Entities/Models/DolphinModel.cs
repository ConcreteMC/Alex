



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class DolphinModel : EntityModel
	{
		public DolphinModel()
		{
			Name = "geometry.dolphin";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 64;
			Textureheight = 64;
			Bones = new EntityModelBone[8]
			{
				new EntityModelBone(){ 
					Name = "head",
					Parent = "body",
					Pivot = new Vector3(0f,0f,-3f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,0f,-9f),
							Size = new Vector3(8f, 7f, 6f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,0f,-3f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,0f,-3f),
							Size = new Vector3(8f, 7f, 13f),
							Uv = new Vector2(0f, 13f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tail",
					Parent = "body",
					Pivot = new Vector3(0f,2.5f,11f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,0f,10f),
							Size = new Vector3(4f, 5f, 11f),
							Uv = new Vector2(0f, 33f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tail_fin",
					Parent = "tail",
					Pivot = new Vector3(0f,2.5f,20f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,2f,19f),
							Size = new Vector3(10f, 1f, 6f),
							Uv = new Vector2(0f, 49f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "back_fin",
					Parent = "body",
					Pivot = new Vector3(0f,7f,2f),
					Rotation = new Vector3(-30f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-0.5f,6.25f,1f),
							Size = new Vector3(1f, 5f, 4f),
							Uv = new Vector2(29f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "left_fin",
					Parent = "body",
					Pivot = new Vector3(3f,1f,-1f),
					Rotation = new Vector3(0f,-25f,20f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(3f,1f,-2.5f),
							Size = new Vector3(8f, 1f, 4f),
							Uv = new Vector2(40f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "right_fin",
					Parent = "body",
					Pivot = new Vector3(-3f,1f,-1f),
					Rotation = new Vector3(0f,25f,-20f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-11f,1f,-2.5f),
							Size = new Vector3(8f, 1f, 4f),
							Uv = new Vector2(40f, 6f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "nose",
					Parent = "head",
					Pivot = new Vector3(0f,0f,-13f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,0f,-13f),
							Size = new Vector3(2f, 2f, 4f),
							Uv = new Vector2(0f, 13f)
						},
					}
				},
			};
		}

	}

}