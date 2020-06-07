



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class ShulkerV18Model : EntityModel
	{
		public ShulkerV18Model()
		{
			Name = "geometry.shulker.v1.8";
			VisibleBoundsWidth = 3;
			VisibleBoundsHeight = 3;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 64;
			Textureheight = 64;
			Bones = new EntityModelBone[3]
			{
				new EntityModelBone(){ 
					Name = "lid",
					Parent = "base",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-8f,4f,-8f),
							Size = new Vector3(16f, 12f, 16f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "base",
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
							Origin = new Vector3(-8f,0f,-8f),
							Size = new Vector3(16f, 8f, 16f),
							Uv = new Vector2(0f, 28f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "head",
					Parent = "base",
					Pivot = new Vector3(0f,12f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,6f,-3f),
							Size = new Vector3(6f, 6f, 6f),
							Uv = new Vector2(0f, 52f)
						},
					}
				},
			};
		}

	}

}