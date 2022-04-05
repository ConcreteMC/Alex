using Alex.Interfaces;
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
					Pivot = Primitives.Factory.Vector3Zero,
					Cubes = new[]
					{
						new EntityModelCube()
						{
							Origin = Primitives.Factory.Vector3(-7, -10, -7),
							Size = Primitives.Factory.Vector3(14, 10, 14),
							Pivot = Primitives.Factory.Vector3Zero,
							Rotation =Primitives.Factory.Vector3(180, 0, 0),
							Uv = EntityModelUV.FromIVector2(Primitives.Factory.Vector2(0, 19))
						}
					}
				},
				new EntityModelBone()
				{
					Name = "head",
					Parent = "body",
					Pivot =Primitives.Factory.Vector3(0, 10, 7),
					Cubes = new[]
					{
						new EntityModelCube()
						{
							Origin = Primitives.Factory.Vector3(-7, -15, -7),
							Size = Primitives.Factory.Vector3(14, 5, 14),
							Pivot = Primitives.Factory.Vector3Zero,
							Rotation = Primitives.Factory.Vector3(180, 0, 0),
							Uv = EntityModelUV.FromIVector2(Primitives.Factory.Vector2Zero)
						},
						new EntityModelCube()
						{
							Origin = Primitives.Factory.Vector3(-1, 8, -8),
							Size = Primitives.Factory.Vector3(2, 4, 1),
							Uv = EntityModelUV.FromIVector2(Primitives.Factory.Vector2Zero)
						}
					}
				}
			};
		}
	}
}