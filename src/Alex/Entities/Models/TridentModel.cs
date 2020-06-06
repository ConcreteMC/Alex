



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class TridentModel : EntityModel
	{
		public TridentModel()
		{
			Name = "geometry.trident";
			VisibleBoundsWidth = 0;
			VisibleBoundsHeight = 0;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 32;
			Textureheight = 32;
			Bones = new EntityModelBone[1]
			{
				new EntityModelBone(){ 
					Name = "pole",
					Parent = "",
					Pivot = new Vector3(0f,24f,0f),
					Rotation = new Vector3(0f,0f,0f),
					BindPoseRotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[4]{
						new EntityModelCube()
						{
							Origin = new Vector3(-0.5f,-3f,-0.5f),
							Size = new Vector3(1f, 31f, 1f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-1.5f,22f,-0.5f),
							Size = new Vector3(3f, 2f, 1f),
							Uv = new Vector2(4f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-2.5f,23f,-0.5f),
							Size = new Vector3(1f, 4f, 1f),
							Uv = new Vector2(4f, 3f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(1.5f,23f,-0.5f),
							Size = new Vector3(1f, 4f, 1f),
							Uv = new Vector2(4f, 3f)
						},
					}
				},
			};
		}

	}

}