



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class BeeModel : EntityModel
	{
		public BeeModel()
		{
			Name = "geometry.bee";
			VisibleBoundsWidth = 2;
			VisibleBoundsHeight = 2;
			VisibleBoundsOffset = new Vector3(0f, 0.25f, 0f);
			Texturewidth = 64;
			Textureheight = 64;
			Bones = new EntityModelBone[7]
			{
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0.5f,5f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[3]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,2f,-5f),
							Size = new Vector3(7f, 7f, 10f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(2f,7f,-8f),
							Size = new Vector3(1f, 2f, 3f),
							Uv = new Vector2(2f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,7f,-8f),
							Size = new Vector3(1f, 2f, 3f),
							Uv = new Vector2(2f, 3f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "stinger",
					Parent = "body",
					Pivot = new Vector3(0.5f,6f,1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0.5f,5f,5f),
							Size = new Vector3(0f, 1f, 2f),
							Uv = new Vector2(26f, 7f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightwing_bone",
					Parent = "body",
					Pivot = new Vector3(-1f,9f,-3f),
					Rotation = new Vector3(15f,-15f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-10f,9f,-3f),
							Size = new Vector3(9f, 0f, 6f),
							Uv = new Vector2(0f, 18f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftwing_bone",
					Parent = "body",
					Pivot = new Vector3(2f,9f,-3f),
					Rotation = new Vector3(15f,15f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2f,9f,-3f),
							Size = new Vector3(9f, 0f, 6f),
							Uv = new Vector2(9f, 24f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg_front",
					Parent = "body",
					Pivot = new Vector3(2f,2f,-2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,0f,-2f),
							Size = new Vector3(7f, 2f, 0f),
							Uv = new Vector2(26f, 1f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg_mid",
					Parent = "body",
					Pivot = new Vector3(2f,2f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,0f,0f),
							Size = new Vector3(7f, 2f, 0f),
							Uv = new Vector2(26f, 3f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg_back",
					Parent = "body",
					Pivot = new Vector3(2f,2f,2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,0f,2f),
							Size = new Vector3(7f, 2f, 0f),
							Uv = new Vector2(26f, 5f)
						},
					}
				},
			};
		}

	}

}