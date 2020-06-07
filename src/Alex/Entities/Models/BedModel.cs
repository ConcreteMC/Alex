



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class BedModel : EntityModel
	{
		public BedModel()
		{
			Name = "geometry.bed";
			VisibleBoundsWidth = 1;
			VisibleBoundsHeight = 1;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 64;
			Textureheight = 64;
			Bones = new EntityModelBone[5]
			{
				new EntityModelBone(){ 
					Name = "bed",
					Parent = "",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[5]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,0f,0f),
							Size = new Vector3(16f, 32f, 6f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(3f,31f,6f),
							Size = new Vector3(10f, 1f, 3f),
							Uv = new Vector2(38f, 2f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(3f,0f,6f),
							Size = new Vector3(10f, 1f, 3f),
							Uv = new Vector2(38f, 38f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(15f,3f,6f),
							Size = new Vector3(1f, 26f, 3f),
							Uv = new Vector2(52f, 6f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(0f,3f,6f),
							Size = new Vector3(1f, 26f, 3f),
							Uv = new Vector2(44f, 6f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg1",
					Parent = "",
					Pivot = new Vector3(5f,22f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(13f,29f,6f),
							Size = new Vector3(3f, 3f, 3f),
							Uv = new Vector2(12f, 38f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg0",
					Parent = "",
					Pivot = new Vector3(-5f,22f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,29f,6f),
							Size = new Vector3(3f, 3f, 3f),
							Uv = new Vector2(0f, 38f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg3",
					Parent = "",
					Pivot = new Vector3(2f,12f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(13f,0f,6f),
							Size = new Vector3(3f, 3f, 3f),
							Uv = new Vector2(12f, 44f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg2",
					Parent = "",
					Pivot = new Vector3(-2f,12f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,0f,6f),
							Size = new Vector3(3f, 3f, 3f),
							Uv = new Vector2(0f, 44f)
						},
					}
				},
			};
		}

	}

}