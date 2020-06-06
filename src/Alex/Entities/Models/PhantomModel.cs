



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class PhantomModel : EntityModel
	{
		public PhantomModel()
		{
			Name = "geometry.phantom";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 64;
			Textureheight = 64;
			Bones = new EntityModelBone[8]
			{
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,23f,-8f),
							Size = new Vector3(5f, 3f, 9f),
							Uv = new Vector2(0f, 8f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "wing0",
					Parent = "body",
					Pivot = new Vector3(2f,26f,-8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,5.7f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2f,24f,-8f),
							Size = new Vector3(6f, 2f, 9f),
							Uv = new Vector2(23f, 12f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "wingtip0",
					Parent = "wing0",
					Pivot = new Vector3(8f,26f,-8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,5.7f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(8f,25f,-8f),
							Size = new Vector3(13f, 1f, 9f),
							Uv = new Vector2(16f, 24f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "wing1",
					Parent = "body",
					Pivot = new Vector3(-3f,26f,-8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,-5.7f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-9f,24f,-8f),
							Size = new Vector3(6f, 2f, 9f),
							Uv = new Vector2(23f, 12f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "wingtip1",
					Parent = "wing1",
					Pivot = new Vector3(-9f,24f,-8f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,-5.7f),
					NeverRender = false,
					Mirror = true,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-22f,25f,-8f),
							Size = new Vector3(13f, 1f, 9f),
							Uv = new Vector2(16f, 24f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "head",
					Parent = "body",
					Pivot = new Vector3(0f,23f,-7f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(11.5f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,22f,-12f),
							Size = new Vector3(7f, 3f, 5f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tail",
					Parent = "body",
					Pivot = new Vector3(0f,26f,1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,24f,1f),
							Size = new Vector3(3f, 2f, 6f),
							Uv = new Vector2(3f, 20f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tailtip",
					Parent = "tail",
					Pivot = new Vector3(0f,25.5f,7f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,24.5f,7f),
							Size = new Vector3(1f, 1f, 6f),
							Uv = new Vector2(4f, 29f)
						},
					}
				},
			};
		}

	}

}