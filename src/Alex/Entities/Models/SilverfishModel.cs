



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class SilverfishModel : EntityModel
	{
		public SilverfishModel()
		{
			Name = "geometry.silverfish";
			VisibleBoundsWidth = 2;
			VisibleBoundsHeight = 1;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 64;
			Textureheight = 32;
			Bones = new EntityModelBone[10]
			{
				new EntityModelBone(){ 
					Name = "bodyPart_0",
					Parent = "bodyPart_2",
					Pivot = new Vector3(0f,2f,-3.5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,0f,-4.5f),
							Size = new Vector3(3f, 2f, 2f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "bodyPart_1",
					Parent = "bodyPart_2",
					Pivot = new Vector3(0f,3f,-1.5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,0f,-2.5f),
							Size = new Vector3(4f, 3f, 2f),
							Uv = new Vector2(0f, 4f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "bodyPart_2",
					Parent = "",
					Pivot = new Vector3(0f,4f,1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,0f,-0.5f),
							Size = new Vector3(6f, 4f, 3f),
							Uv = new Vector2(0f, 9f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "bodyPart_3",
					Parent = "bodyPart_2",
					Pivot = new Vector3(0f,3f,4f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,0f,2.5f),
							Size = new Vector3(3f, 3f, 3f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "bodyPart_4",
					Parent = "bodyPart_2",
					Pivot = new Vector3(0f,2f,7f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,0f,5.5f),
							Size = new Vector3(2f, 2f, 3f),
							Uv = new Vector2(0f, 22f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "bodyPart_5",
					Parent = "bodyPart_2",
					Pivot = new Vector3(0f,1f,9.5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,0f,8.5f),
							Size = new Vector3(2f, 1f, 2f),
							Uv = new Vector2(11f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "bodyPart_6",
					Parent = "bodyPart_2",
					Pivot = new Vector3(0f,1f,11.5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-0.5f,0f,10.5f),
							Size = new Vector3(1f, 1f, 2f),
							Uv = new Vector2(13f, 4f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "bodyLayer_0",
					Parent = "bodyPart_2",
					Pivot = new Vector3(0f,8f,1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,0f,-0.5f),
							Size = new Vector3(10f, 8f, 3f),
							Uv = new Vector2(20f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "bodyLayer_1",
					Parent = "bodyPart_4",
					Pivot = new Vector3(0f,4f,7f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,0f,5.5f),
							Size = new Vector3(6f, 4f, 3f),
							Uv = new Vector2(20f, 11f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "bodyLayer_2",
					Parent = "bodyPart_1",
					Pivot = new Vector3(0f,5f,-1.5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,0f,-3f),
							Size = new Vector3(6f, 5f, 2f),
							Uv = new Vector2(20f, 18f)
						},
					}
				},
			};
		}

	}

}