



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class PufferfishLargeModel : EntityModel
	{
		public PufferfishLargeModel()
		{
			Name = "geometry.pufferfish.large";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 32;
			Textureheight = 32;
			Bones = new EntityModelBone[15]
			{
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,0f,-4f),
							Size = new Vector3(8f, 8f, 8f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftFin",
					Parent = "body",
					Pivot = new Vector3(4f,7f,3f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(4f,6f,-2.9904f),
							Size = new Vector3(2f, 1f, 2f),
							Uv = new Vector2(24f, 3f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightFin",
					Parent = "body",
					Pivot = new Vector3(-4f,7f,1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5.9968f,6f,-2.992f),
							Size = new Vector3(2f, 1f, 2f),
							Uv = new Vector2(24f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_top_front",
					Parent = "body",
					Pivot = new Vector3(-4f,8f,-4f),
					Rotation = new Vector3(45f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,8f,-4f),
							Size = new Vector3(8f, 1f, 1f),
							Uv = new Vector2(14f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_top_mid",
					Parent = "body",
					Pivot = new Vector3(0f,8f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,8f,0f),
							Size = new Vector3(8f, 1f, 1f),
							Uv = new Vector2(14f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_top_back",
					Parent = "body",
					Pivot = new Vector3(0f,8f,4f),
					Rotation = new Vector3(-45f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,8f,4f),
							Size = new Vector3(8f, 1f, 1f),
							Uv = new Vector2(14f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_bottom_front",
					Parent = "body",
					Pivot = new Vector3(0f,0f,-4f),
					Rotation = new Vector3(-45f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,-1f,-4f),
							Size = new Vector3(8f, 1f, 1f),
							Uv = new Vector2(14f, 19f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_bottom_mid",
					Parent = "body",
					Pivot = new Vector3(0f,-1f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,-1f,0f),
							Size = new Vector3(8f, 1f, 1f),
							Uv = new Vector2(14f, 19f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_bottom_back",
					Parent = "body",
					Pivot = new Vector3(0f,0f,4f),
					Rotation = new Vector3(45f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,-1f,4f),
							Size = new Vector3(8f, 1f, 1f),
							Uv = new Vector2(14f, 19f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_left_front",
					Parent = "body",
					Pivot = new Vector3(4f,0f,-4f),
					Rotation = new Vector3(0f,45f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(4f,0f,-4f),
							Size = new Vector3(1f, 8f, 1f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_left_mid",
					Parent = "body",
					Pivot = new Vector3(4f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(4f,0f,0f),
							Size = new Vector3(1f, 8f, 1f),
							Uv = new Vector2(4f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_left_back",
					Parent = "body",
					Pivot = new Vector3(4f,0f,4f),
					Rotation = new Vector3(0f,-45f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(4f,0f,4f),
							Size = new Vector3(1f, 8f, 1f),
							Uv = new Vector2(8f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_right_front",
					Parent = "body",
					Pivot = new Vector3(-4f,0f,-4f),
					Rotation = new Vector3(0f,-45f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,0f,-4f),
							Size = new Vector3(1f, 8f, 1f),
							Uv = new Vector2(4f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_right_mid",
					Parent = "body",
					Pivot = new Vector3(-4f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,0f,0f),
							Size = new Vector3(1f, 8f, 1f),
							Uv = new Vector2(8f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_right_back",
					Parent = "body",
					Pivot = new Vector3(-4f,0f,4f),
					Rotation = new Vector3(0f,45f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,0f,4f),
							Size = new Vector3(1f, 8f, 1f),
							Uv = new Vector2(8f, 16f)
						},
					}
				},
			};
		}

	}

}