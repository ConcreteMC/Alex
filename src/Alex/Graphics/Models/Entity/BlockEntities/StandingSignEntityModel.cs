using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Entity.BlockEntities
{
	public class StandingSignEntityModel : EntityModel
	{
		public StandingSignEntityModel()
		{
			Description = new ModelDescription()
			{
				Identifier = "geometry.alex.sign.standing", TextureHeight = 32, TextureWidth = 64
			};

			Bones = new EntityModelBone[]
			{
				new EntityModelBone()
				{
					Name = "root",
					Pivot = Vector3.Zero,
					Cubes = new[]
					{
						new EntityModelCube()
						{
							Origin = new Vector3(-8, 6f, -1f),
							Size = new Vector3(16, 11, 1),
							Uv = new Vector2(9, 2)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-1, 0f, -0.8f),
							Size = new Vector3(2, 14, 2),
							Uv = new Vector2(0, 14)
						}
					}
				}
			};
		}
	}
	
	public class WallSignEntityModel : EntityModel
	{
		public WallSignEntityModel()
		{
			Description = new ModelDescription()
			{
				Identifier = "geometry.alex.sign", TextureHeight = 32, TextureWidth = 64
			};

			Bones = new EntityModelBone[]
			{
				new EntityModelBone()
				{
					Name = "root",
					Pivot = Vector3.Zero,
					Cubes = new[]
					{
						new EntityModelCube()
						{
							Origin = new Vector3(0, 2.5f, 0),
							Size = new Vector3(16, 11, 1),
							Uv = new Vector2(9, 2)
						}
					}
				}
			};
		}
	}
}