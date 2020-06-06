



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class WitherbossArmorModel : EntityModel
	{
		public WitherbossArmorModel()
		{
			Name = "geometry.witherBoss.armor";
			VisibleBoundsWidth = 3;
			VisibleBoundsHeight = 4;
			VisibleBoundsOffset = new Vector3(0f, 2f, 0f);
			Texturewidth = 64;
			Textureheight = 64;
			Bones = new EntityModelBone[6]
			{
				new EntityModelBone(){ 
					Name = "upperBodyPart1",
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
							Origin = new Vector3(-10f,17.1f,-0.5f),
							Size = new Vector3(20f, 3f, 3f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "upperBodyPart2",
					Parent = "upperBodyPart1",
					Pivot = new Vector3(-2f,17.1f,-0.5f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[4]{
						new EntityModelCube()
						{
							Origin = new Vector3(-2f,7.1f,-0.5f),
							Size = new Vector3(3f, 10f, 3f),
							Uv = new Vector2(0f, 22f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-6f,13.6f,0f),
							Size = new Vector3(11f, 2f, 2f),
							Uv = new Vector2(24f, 22f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-6f,11.1f,0f),
							Size = new Vector3(11f, 2f, 2f),
							Uv = new Vector2(24f, 22f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-6f,8.6f,0f),
							Size = new Vector3(11f, 2f, 2f),
							Uv = new Vector2(24f, 22f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "upperBodyPart3",
					Parent = "upperBodyPart2",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,18f,0f),
							Size = new Vector3(3f, 6f, 3f),
							Uv = new Vector2(12f, 22f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "head1",
					Parent = "upperBodyPart1",
					Pivot = new Vector3(0f,20f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,20f,-4f),
							Size = new Vector3(8f, 8f, 8f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "head2",
					Parent = "upperBodyPart1",
					Pivot = new Vector3(-9f,18f,-1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-12f,18f,-4f),
							Size = new Vector3(6f, 6f, 6f),
							Uv = new Vector2(32f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "head3",
					Parent = "upperBodyPart1",
					Pivot = new Vector3(9f,18f,-1f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(6f,18f,-4f),
							Size = new Vector3(6f, 6f, 6f),
							Uv = new Vector2(32f, 0f)
						},
					}
				},
			};
		}

	}

}