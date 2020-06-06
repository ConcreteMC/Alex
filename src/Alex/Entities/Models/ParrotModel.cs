



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class ParrotModel : EntityModel
	{
		public ParrotModel()
		{
			Name = "geometry.parrot";
			VisibleBoundsWidth = 1;
			VisibleBoundsHeight = 1;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 32;
			Textureheight = 32;
			Bones = new EntityModelBone[11]
			{
				new EntityModelBone(){ 
					Name = "head",
					Parent = "body",
					Pivot = new Vector3(0f,8.3f,-2.8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,6.8f,-3.8f),
							Size = new Vector3(2f, 3f, 2f),
							Uv = new Vector2(2f, 2f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "head2",
					Parent = "head",
					Pivot = new Vector3(0f,26f,-1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,9.8f,-5.8f),
							Size = new Vector3(2f, 1f, 4f),
							Uv = new Vector2(10f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "beak1",
					Parent = "head",
					Pivot = new Vector3(0f,24.5f,-1.5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-0.5f,7.8f,-4.7f),
							Size = new Vector3(1f, 2f, 1f),
							Uv = new Vector2(11f, 7f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "beak2",
					Parent = "head",
					Pivot = new Vector3(0f,25.8f,-2.5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-0.5f,8.1f,-5.7f),
							Size = new Vector3(1f, 1.7f, 1f),
							Uv = new Vector2(16f, 7f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,7.5f,-3f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(28.287f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,1.5f,-4.5f),
							Size = new Vector3(3f, 6f, 3f),
							Uv = new Vector2(2f, 8f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tail",
					Parent = "body",
					Pivot = new Vector3(0f,2.9f,1.2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,-0.1f,0.2f),
							Size = new Vector3(3f, 4f, 1f),
							Uv = new Vector2(22f, 1f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "wing0",
					Parent = "body",
					Pivot = new Vector3(1.5f,7.1f,-2.8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1f,2.1f,-4.3f),
							Size = new Vector3(1f, 5f, 3f),
							Uv = new Vector2(19f, 8f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "wing1",
					Parent = "body",
					Pivot = new Vector3(-1.5f,7.1f,-2.8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,2.1f,-4.3f),
							Size = new Vector3(1f, 5f, 3f),
							Uv = new Vector2(19f, 8f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "feather",
					Parent = "head",
					Pivot = new Vector3(0f,10.1f,0.2f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(-12.685f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,9.1f,-4.9f),
							Size = new Vector3(0f, 5f, 4f),
							Uv = new Vector2(2f, 18f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg0",
					Parent = "body",
					Pivot = new Vector3(1.5f,1f,-0.5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0.5f,-0.5f,-1.5f),
							Size = new Vector3(1f, 2f, 1f),
							Uv = new Vector2(14f, 18f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg1",
					Parent = "body",
					Pivot = new Vector3(-0.5f,1f,-0.5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,-0.5f,-1.5f),
							Size = new Vector3(1f, 2f, 1f),
							Uv = new Vector2(14f, 18f)
						},
					}
				},
			};
		}

	}

}