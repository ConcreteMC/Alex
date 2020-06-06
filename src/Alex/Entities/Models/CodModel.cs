



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class CodModel : EntityModel
	{
		public CodModel()
		{
			Name = "geometry.cod";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 32;
			Textureheight = 32;
			Bones = new EntityModelBone[6]
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
					Cubes = new EntityModelCube[3]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,0f,1f),
							Size = new Vector3(2f, 4f, 7f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(0f,4f,0f),
							Size = new Vector3(0f, 1f, 6f),
							Uv = new Vector2(20f, -6f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(0f,-1f,3f),
							Size = new Vector3(0f, 1f, 2f),
							Uv = new Vector2(22f, -1f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "head",
					Parent = "body",
					Pivot = new Vector3(0f,2f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-0.9992f,1.0008f,-3f),
							Size = new Vector3(2f, 3f, 1f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,0f,-2f),
							Size = new Vector3(2f, 4f, 3f),
							Uv = new Vector2(11f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftFin",
					Parent = "body",
					Pivot = new Vector3(1f,1f,0f),
					Rotation = new Vector3(0f,0f,35f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1f,0f,0f),
							Size = new Vector3(2f, 1f, 2f),
							Uv = new Vector2(24f, 4f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightFin",
					Parent = "body",
					Pivot = new Vector3(-1f,1f,0f),
					Rotation = new Vector3(0f,0f,-35f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,0f,0f),
							Size = new Vector3(2f, 1f, 2f),
							Uv = new Vector2(24f, 1f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tailfin",
					Parent = "body",
					Pivot = new Vector3(0f,0f,8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,0f,8f),
							Size = new Vector3(0f, 4f, 6f),
							Uv = new Vector2(20f, 1f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "waist",
					Parent = "body",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[0]{
					}
				},
			};
		}

	}

}