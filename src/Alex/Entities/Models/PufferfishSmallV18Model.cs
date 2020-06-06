



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class PufferfishSmallV18Model : EntityModel
	{
		public PufferfishSmallV18Model()
		{
			Name = "geometry.pufferfish.small.v1.8";
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
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[3]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,0f,-1.5f),
							Size = new Vector3(3f, 2f, 3f),
							Uv = new Vector2(0f, 27f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(0.5f,2f,-1.5f),
							Size = new Vector3(1f, 1f, 1f),
							Uv = new Vector2(24f, 6f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,2f,-1.5f),
							Size = new Vector3(1f, 1f, 1f),
							Uv = new Vector2(28f, 6f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tailfin",
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
							Origin = new Vector3(-1.5f,1f,1.5f),
							Size = new Vector3(3f, 0f, 3f),
							Uv = new Vector2(-3f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftFin",
					Parent = "body",
					Pivot = new Vector3(6.5f,5f,0.5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1.5f,0f,-1.5f),
							Size = new Vector3(1f, 1f, 2f),
							Uv = new Vector2(25f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightFin",
					Parent = "body",
					Pivot = new Vector3(-6.5f,5f,0.5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2.5f,0f,-1.5f),
							Size = new Vector3(1f, 1f, 2f),
							Uv = new Vector2(25f, 0f)
						},
					}
				},
			};
		}

	}

}