



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class TropicalfishBModel : EntityModel
	{
		public TropicalfishBModel()
		{
			Name = "geometry.tropicalfish_b";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 32;
			Textureheight = 32;
			Bones = new EntityModelBone[4]
			{
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(-0.5f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[3]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,0f,-0.0008f),
							Size = new Vector3(2f, 6f, 6f),
							Uv = new Vector2(0f, 20f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(0f,-5f,-0.0008f),
							Size = new Vector3(0f, 5f, 6f),
							Uv = new Vector2(20f, 21f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(0f,6f,-0.0008f),
							Size = new Vector3(0f, 5f, 6f),
							Uv = new Vector2(20f, 10f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tailfin",
					Parent = "body",
					Pivot = new Vector3(0f,0f,6f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,0.0008f,6f),
							Size = new Vector3(0f, 6f, 5f),
							Uv = new Vector2(21f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftFin",
					Parent = "body",
					Pivot = new Vector3(0.5f,0f,1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,-35f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2.05673f,0f,2.35152f),
							Size = new Vector3(2f, 2f, 0f),
							Uv = new Vector2(2f, 12f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightFin",
					Parent = "body",
					Pivot = new Vector3(-0.5f,0f,1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,35f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4.05673f,0f,2.35152f),
							Size = new Vector3(2f, 2f, 0f),
							Uv = new Vector2(2f, 16f)
						},
					}
				},
			};
		}

	}

}