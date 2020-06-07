



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class ArrowModel : EntityModel
	{
		public ArrowModel()
		{
			Name = "geometry.arrow";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 32;
			Textureheight = 32;
			Bones = new EntityModelBone[1]
			{
				new EntityModelBone(){ 
					Name = "body",
					Parent = "",
					Pivot = new Vector3(0f,1f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[3]{
						new EntityModelCube()
						{
							Origin = new Vector3(0f,-1.5f,-3f),
							Size = new Vector3(0f, 5f, 16f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(0f,-1.5f,-3f),
							Size = new Vector3(0f, 5f, 16f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-2.5f,-1.5f,12f),
							Size = new Vector3(5f, 5f, 0f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
			};
		}

	}

}