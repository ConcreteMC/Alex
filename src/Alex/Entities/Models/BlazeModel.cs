



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class BlazeModel : EntityModel
	{
		public BlazeModel()
		{
			Name = "geometry.blaze";
			VisibleBoundsWidth = 2;
			VisibleBoundsHeight = 2;
			VisibleBoundsOffset = new Vector3(0f, 1f, 0f);
			Texturewidth = 64;
			Textureheight = 32;
			Bones = new EntityModelBone[13]
			{
				new EntityModelBone(){ 
					Name = "upperBodyParts0",
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
							Origin = new Vector3(0f,16f,0f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "upperBodyParts1",
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
							Origin = new Vector3(0f,16f,0f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "upperBodyParts2",
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
							Origin = new Vector3(0f,16f,0f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "upperBodyParts3",
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
							Origin = new Vector3(0f,16f,0f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "upperBodyParts4",
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
							Origin = new Vector3(0f,16f,0f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "upperBodyParts5",
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
							Origin = new Vector3(0f,16f,0f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "upperBodyParts6",
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
							Origin = new Vector3(0f,16f,0f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "upperBodyParts7",
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
							Origin = new Vector3(0f,16f,0f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "upperBodyParts8",
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
							Origin = new Vector3(0f,16f,0f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "upperBodyParts9",
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
							Origin = new Vector3(0f,16f,0f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "upperBodyParts10",
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
							Origin = new Vector3(0f,16f,0f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "upperBodyParts11",
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
							Origin = new Vector3(0f,16f,0f),
							Size = new Vector3(2f, 8f, 2f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "head",
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
							Origin = new Vector3(-4f,20f,-4f),
							Size = new Vector3(8f, 8f, 8f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
			};
		}

	}

}