



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class SquidModel : EntityModel
	{
		public SquidModel()
		{
			Name = "geometry.squid";
			VisibleBoundsWidth = 3;
			VisibleBoundsHeight = 2;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 64;
			Textureheight = 32;
			Bones = new EntityModelBone[9]
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
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-6f,-8f,-6f),
							Size = new Vector3(12f, 16f, 12f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tentacle1",
					Parent = "",
					Pivot = new Vector3(5f,-7f,0f),
					Rotation = new Vector3(0f,90f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(4f,-25f,-1f),
							Size = new Vector3(2f, 18f, 2f),
							Uv = new Vector2(48f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tentacle2",
					Parent = "",
					Pivot = new Vector3(3.5f,-7f,3.5f),
					Rotation = new Vector3(0f,45f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2.5f,-25f,2.5f),
							Size = new Vector3(2f, 18f, 2f),
							Uv = new Vector2(48f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tentacle3",
					Parent = "",
					Pivot = new Vector3(0f,-7f,5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,-25f,4f),
							Size = new Vector3(2f, 18f, 2f),
							Uv = new Vector2(48f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tentacle4",
					Parent = "",
					Pivot = new Vector3(-3.5f,-7f,3.5f),
					Rotation = new Vector3(0f,-45f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4.5f,-25f,2.5f),
							Size = new Vector3(2f, 18f, 2f),
							Uv = new Vector2(48f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tentacle5",
					Parent = "",
					Pivot = new Vector3(-5f,-7f,0f),
					Rotation = new Vector3(0f,-90f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-6f,-25f,-1f),
							Size = new Vector3(2f, 18f, 2f),
							Uv = new Vector2(48f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tentacle6",
					Parent = "",
					Pivot = new Vector3(-3.5f,-7f,-3.5f),
					Rotation = new Vector3(0f,-135f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4.5f,-25f,-4.5f),
							Size = new Vector3(2f, 18f, 2f),
							Uv = new Vector2(48f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tentacle7",
					Parent = "",
					Pivot = new Vector3(0f,-7f,-5f),
					Rotation = new Vector3(0f,-180f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,-25f,-6f),
							Size = new Vector3(2f, 18f, 2f),
							Uv = new Vector2(48f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "tentacle8",
					Parent = "",
					Pivot = new Vector3(3.5f,-7f,-3.5f),
					Rotation = new Vector3(0f,-225f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(2.5f,-25f,-4.5f),
							Size = new Vector3(2f, 18f, 2f),
							Uv = new Vector2(48f, 0f)
						},
					}
				},
			};
		}

	}

}