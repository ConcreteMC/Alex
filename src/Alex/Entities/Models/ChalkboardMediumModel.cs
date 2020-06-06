



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class ChalkboardMediumModel : EntityModel
	{
		public ChalkboardMediumModel()
		{
			Name = "geometry.chalkboard_medium";
			VisibleBoundsWidth = 1;
			VisibleBoundsHeight = 1;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 128;
			Textureheight = 32;
			Bones = new EntityModelBone[3]
			{
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,0f,1.421085E-16f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-16f,4f,-1f),
							Size = new Vector3(32f, 12f, 2f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftLeg",
					Parent = "",
					Pivot = new Vector3(2.273737E-15f,-5.684342E-16f,7.105427E-17f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(10f,0f,-1f),
							Size = new Vector3(2f, 4f, 2f),
							Uv = new Vector2(0f, 14f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightLeg",
					Parent = "",
					Pivot = new Vector3(-4.547474E-15f,-8.526513E-16f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-12f,0f,-1f),
							Size = new Vector3(2f, 4f, 2f),
							Uv = new Vector2(0f, 14f)
						},
					}
				},
			};
		}

	}

}