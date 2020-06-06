



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class PufferfishMidModel : EntityModel
	{
		public PufferfishMidModel()
		{
			Name = "geometry.pufferfish.mid";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 32;
			Textureheight = 32;
			Bones = new EntityModelBone[11]
			{
				new EntityModelBone(){ 
					Name = "body",
					Parent = "body",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.5f,1f,-2.5f),
							Size = new Vector3(5f, 5f, 5f),
							Uv = new Vector2(12f, 22f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftFin",
					Parent = "body",
					Pivot = new Vector3(2.5f,5f,0.5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2.5f,4f,-1.5f),
							Size = new Vector3(2f, 1f, 2f),
							Uv = new Vector2(24f, 3f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightFin",
					Parent = "body",
					Pivot = new Vector3(-2.5f,5f,0.5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4.5f,4f,-1.5f),
							Size = new Vector3(2f, 1f, 2f),
							Uv = new Vector2(24f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_top_front",
					Parent = "body",
					Pivot = new Vector3(0f,6f,-2.5f),
					Rotation = new Vector3(45f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.5f,6f,-2.5f),
							Size = new Vector3(5f, 1f, 0f),
							Uv = new Vector2(19f, 17f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_top_back",
					Parent = "body",
					Pivot = new Vector3(0f,6f,2.5f),
					Rotation = new Vector3(-45f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.5f,6f,2.5f),
							Size = new Vector3(5f, 1f, 0f),
							Uv = new Vector2(11f, 17f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_bottom_front",
					Parent = "body",
					Pivot = new Vector3(0f,1f,-2.5f),
					Rotation = new Vector3(-45f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.5f,0f,-2.5f),
							Size = new Vector3(5f, 1f, 0f),
							Uv = new Vector2(18f, 20f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_bottom_back",
					Parent = "body",
					Pivot = new Vector3(0f,1f,2.5f),
					Rotation = new Vector3(45f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.5f,0f,2.5f),
							Size = new Vector3(5f, 1f, 0f),
							Uv = new Vector2(18f, 20f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_left_front",
					Parent = "body",
					Pivot = new Vector3(2.5f,0f,-2.5f),
					Rotation = new Vector3(0f,45f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2.5f,1f,-2.5f),
							Size = new Vector3(1f, 5f, 0f),
							Uv = new Vector2(1f, 17f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_left_back",
					Parent = "body",
					Pivot = new Vector3(2.5f,0f,2.5f),
					Rotation = new Vector3(0f,-45f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2.5f,1f,2.5f),
							Size = new Vector3(1f, 5f, 0f),
							Uv = new Vector2(1f, 17f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_right_front",
					Parent = "body",
					Pivot = new Vector3(-2.5f,0f,-2.5f),
					Rotation = new Vector3(0f,-45f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3.5f,1f,-2.5f),
							Size = new Vector3(1f, 5f, 0f),
							Uv = new Vector2(5f, 17f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "spines_right_back",
					Parent = "body",
					Pivot = new Vector3(-2.5f,0f,2.5f),
					Rotation = new Vector3(0f,45f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3.5f,1f,2.5f),
							Size = new Vector3(1f, 5f, 0f),
							Uv = new Vector2(9f, 17f)
						},
					}
				},
			};
		}

	}

}