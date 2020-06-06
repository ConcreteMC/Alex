



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class ChalkboardLargeModel : EntityModel
	{
		public ChalkboardLargeModel()
		{
			Name = "geometry.chalkboard_large";
			VisibleBoundsWidth = 1;
			VisibleBoundsHeight = 1;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 128;
			Textureheight = 64;
			Bones = new EntityModelBone[3]
			{
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,-1.136868E-15f,7.105427E-17f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-24f,4f,-1f),
							Size = new Vector3(48f, 28f, 2f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leftLeg",
					Parent = "",
					Pivot = new Vector3(0f,2.842171E-16f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(16f,0f,-1f),
							Size = new Vector3(2f, 4f, 2f),
							Uv = new Vector2(0f, 30f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightLeg",
					Parent = "",
					Pivot = new Vector3(0f,2.842171E-16f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-18f,0f,-1f),
							Size = new Vector3(2f, 4f, 2f),
							Uv = new Vector2(0f, 30f)
						},
					}
				},
			};
		}

	}

}