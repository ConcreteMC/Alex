



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class ChalkboardSmallModel : EntityModel
	{
		public ChalkboardSmallModel()
		{
			Name = "geometry.chalkboard_small";
			VisibleBoundsWidth = 1;
			VisibleBoundsHeight = 1;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 64;
			Textureheight = 32;
			Bones = new EntityModelBone[2]
			{
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(-8.881784E-18f,0f,7.105427E-17f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-8f,4f,-1f),
							Size = new Vector3(16f, 12f, 2f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
				new EntityModelBone(){ 
					Name = "rightLeg",
					Parent = "",
					Pivot = new Vector3(0f,2.842171E-16f,7.105427E-17f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[1]{
						new EntityModelCube()
						{
							Origin = new Vector3(-1f,0f,-1f),
							Size = new Vector3(2f, 4f, 2f),
							Uv = new Vector2(0f, 14f)
						},
					}
				},
			};
		}

	}

}