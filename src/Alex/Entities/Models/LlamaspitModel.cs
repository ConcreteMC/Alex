



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class LlamaspitModel : EntityModel
	{
		public LlamaspitModel()
		{
			Name = "geometry.llamaspit";
			VisibleBoundsWidth = 1;
			VisibleBoundsHeight = 1;
			VisibleBoundsOffset = new Vector3(0f, 0.5f, 0f);
			Texturewidth = 64;
			Textureheight = 64;
			Bones = new EntityModelBone[1]
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
					Cubes = new EntityModelCube[7]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,22f,0f),
							Size = new Vector3(2f, 2f, 2f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(0f,26f,0f),
							Size = new Vector3(2f, 2f, 2f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(0f,22f,-4f),
							Size = new Vector3(2f, 2f, 2f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(0f,22f,0f),
							Size = new Vector3(2f, 2f, 2f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(2f,22f,0f),
							Size = new Vector3(2f, 2f, 2f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(0f,20f,0f),
							Size = new Vector3(2f, 2f, 2f),
							Uv = new Vector2(0f, 0f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(0f,22f,2f),
							Size = new Vector3(2f, 2f, 2f),
							Uv = new Vector2(0f, 0f)
						},
					}
				},
			};
		}

	}

}