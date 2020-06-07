



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class LavaslimeModel : EntityModel
	{
		public LavaslimeModel()
		{
			Name = "geometry.lavaslime";
			VisibleBoundsWidth = 2;
			VisibleBoundsHeight = 5;
			VisibleBoundsOffset = new Vector3(0f, 2.5f, 0f);
			Texturewidth = 64;
			Textureheight = 32;
			Bones = new EntityModelBone[9]
			{
				new EntityModelBone(){ 
					Name = "bodyCube_0",
					Parent = "insideCube",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,7f,-4f),
							Size = new Vector3(8f, 1f, 8f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "bodyCube_1",
					Parent = "insideCube",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,6f,-4f),
							Size = new Vector3(8f, 1f, 8f),
							Uv = new Vector2(0f, 1f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "bodyCube_2",
					Parent = "insideCube",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,5f,-4f),
							Size = new Vector3(8f, 1f, 8f),
							Uv = new Vector2(24f, 10f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "bodyCube_3",
					Parent = "insideCube",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,4f,-4f),
							Size = new Vector3(8f, 1f, 8f),
							Uv = new Vector2(24f, 19f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "bodyCube_4",
					Parent = "insideCube",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,3f,-4f),
							Size = new Vector3(8f, 1f, 8f),
							Uv = new Vector2(0f, 4f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "bodyCube_5",
					Parent = "insideCube",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,2f,-4f),
							Size = new Vector3(8f, 1f, 8f),
							Uv = new Vector2(0f, 5f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "bodyCube_6",
					Parent = "insideCube",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,1f,-4f),
							Size = new Vector3(8f, 1f, 8f),
							Uv = new Vector2(0f, 6f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "bodyCube_7",
					Parent = "insideCube",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,0f,-4f),
							Size = new Vector3(8f, 1f, 8f),
							Uv = new Vector2(0f, 7f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "insideCube",
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
							Origin = new Vector3(-2f,2f,-2f),
							Size = new Vector3(4f, 4f, 4f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
			};
		}

	}

}