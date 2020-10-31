using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Entity.BlockEntities
{
	public class ChestEntityModel : EntityModel
	{
		public ChestEntityModel()
		{
			Description = new ModelDescription()
			{
				Identifier = "geometry.alex.chest", TextureHeight = 64, TextureWidth = 64
			};

			Bones = new EntityModelBone[]
			{
				new EntityModelBone()
				{
					Name = "body",
					Pivot = Vector3.Zero,
					Cubes = new[]
					{
						new EntityModelCube()
						{
							Origin = new Vector3(-7, -10, -7),
							Size = new Vector3(14, 10, 14),
							Pivot = Vector3.Zero,
							Rotation = new Vector3(180, 0, 0),
							Uv = new Vector2(0, 19)
						}
					}
				},
				new EntityModelBone()
				{
					Name = "head",
					Parent = "body",
					Pivot = new Vector3(0, 10, 7),
					Cubes = new[]
					{
						new EntityModelCube()
						{
							Origin = new Vector3(-7, -15, -7),
							Size = new Vector3(14, 5, 14),
							Pivot = Vector3.Zero,
							Rotation = new Vector3(180, 0, 0),
							Uv = new Vector2(0, 0)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-1, 8, -8),
							Size = new Vector3(2, 4, 1),
							Uv = Vector2.Zero
						}
					}
				}
			};
		}
	}
}