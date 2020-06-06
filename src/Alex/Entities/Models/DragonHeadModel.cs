



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class DragonHeadModel : EntityModel
	{
		public DragonHeadModel()
		{
			Name = "geometry.dragon_head";
			VisibleBoundsWidth = 1;
			VisibleBoundsHeight = 1;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 256;
			Textureheight = 256;
			Bones = new EntityModelBone[3]
			{
				new EntityModelBone(){ 
					Name = "head",
					Parent = "",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[3]{
						new EntityModelCube()
						{
							Origin = new Vector3(-8f,16f,-10f),
							Size = new Vector3(16f, 16f, 16f),
							Uv = new Vector2(112f, 30f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,32f,-4f),
							Size = new Vector3(2f, 4f, 6f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(3f,32f,-4f),
							Size = new Vector3(2f, 4f, 6f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "snout",
					Parent = "",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[3]{
						new EntityModelCube()
						{
							Origin = new Vector3(-6f,20f,-24f),
							Size = new Vector3(12f, 5f, 16f),
							Uv = new Vector2(176f, 44f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,25f,-22f),
							Size = new Vector3(2f, 2f, 4f),
							Uv = new Vector2(112f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(3f,25f,-22f),
							Size = new Vector3(2f, 2f, 4f),
							Uv = new Vector2(112f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "jaw",
					Parent = "",
					Pivot = new Vector3(0f,20f,-8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-6f,16f,-24f),
							Size = new Vector3(12f, 4f, 16f),
							Uv = new Vector2(176f, 65f)
						},
					}
				},
			};
		}

	}

}