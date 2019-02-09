


using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class EndermanModel : EntityModel
	{
		public EndermanModel()
		{
			Name = "geometry.enderman";
			VisibleBoundsWidth = 2;
			VisibleBoundsHeight = 3;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 64;
			Textureheight = 32;
			Bones = new EntityModelBone[8]
			{
				new EntityModelBone(){ 
					Name = "hat",
					Parent = "head",
					Pivot = new Vector3(0f,38f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = true,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,37.5f,-4f),
							Size = new Vector3(8f, 8f, 8f),
							Uv = new Vector2(0f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "head",
					Parent = "body",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,24f,-4f),
							Size = new Vector3(8f, 8f, 8f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,38f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = true,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,26f,-2f),
							Size = new Vector3(8f, 12f, 4f),
							Uv = new Vector2(32f, 16f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightArm",
					Parent = "body",
					Pivot = new Vector3(-3f,36f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = true,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,8f,-1f),
							Size = new Vector3(2f, 30f, 2f),
							Uv = new Vector2(56f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightItem",
					Parent = "rightArm",
					Pivot = new Vector3(-6f,15f,1f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = true,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[0]
				},
				new EntityModelBone(){ 
					Name = "leftArm",
					Parent = "body",
					Pivot = new Vector3(5f,36f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = true,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(4f,8f,-1f),
							Size = new Vector3(2f, 30f, 2f),
							Uv = new Vector2(56f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightLeg",
					Parent = "body",
					Pivot = new Vector3(-2f,26f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = true,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,-4f,-1f),
							Size = new Vector3(2f, 30f, 2f),
							Uv = new Vector2(56f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftLeg",
					Parent = "body",
					Pivot = new Vector3(2f,26f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = true,
					Reset = true,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(1f,-4f,-1f),
							Size = new Vector3(2f, 30f, 2f),
							Uv = new Vector2(56f, 0f)
						},
					}
				},
			};
		}

	}

}