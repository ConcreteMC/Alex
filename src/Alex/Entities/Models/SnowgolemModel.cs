



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class SnowgolemModel : EntityModel
	{
		public SnowgolemModel()
		{
			Name = "geometry.snowgolem";
			VisibleBoundsWidth = 1;
			VisibleBoundsHeight = 2;
			VisibleBoundsOffset = new Vector3(0f, 1f, 0f);
			Texturewidth = 0;
			Textureheight = 0;
			Bones = new EntityModelBone[5]
			{
				new EntityModelBone(){ 
					Name = "head",
					Parent = "",
					Pivot = new Vector3(0f,20f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,20f,-4f),
							Size = new Vector3(8f, 8f, 8f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "arm1",
					Parent = "",
					Pivot = new Vector3(0f,18f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,16f,-1f),
							Size = new Vector3(12f, 2f, 2f),
							Uv = new Vector2(32f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "arm2",
					Parent = "",
					Pivot = new Vector3(0f,18f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,16f,-1f),
							Size = new Vector3(12f, 2f, 2f),
							Uv = new Vector2(32f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "piece1",
					Parent = "",
					Pivot = new Vector3(0f,11f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,11f,-5f),
							Size = new Vector3(10f, 10f, 10f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "piece2",
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
							Origin = new Vector3(-6f,0f,-6f),
							Size = new Vector3(12f, 12f, 12f),
							Uv = new Vector2(0f, 36f)
						},
					}
				},
			};
		}

	}

}