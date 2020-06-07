



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class EvocationFangModel : EntityModel
	{
		public EvocationFangModel()
		{
			Name = "geometry.evocation_fang";
			VisibleBoundsWidth = 2;
			VisibleBoundsHeight = 3;
			VisibleBoundsOffset = new Vector3(0f, 1.5f, 0f);
			Texturewidth = 64;
			Textureheight = 32;
			Bones = new EntityModelBone[3]
			{
				new EntityModelBone(){ 
					Name = "upper_jaw",
					Parent = "base",
					Pivot = new Vector3(0f,11f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,0f,-4f),
							Size = new Vector3(4f, 14f, 8f),
							Uv = new Vector2(40f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "lower_jaw",
					Parent = "base",
					Pivot = new Vector3(0f,11f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,180f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,0f,-4f),
							Size = new Vector3(4f, 14f, 8f),
							Uv = new Vector2(40f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "base",
					Parent = "",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,90f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,0f,-5f),
							Size = new Vector3(10f, 12f, 10f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
			};
		}

	}

}