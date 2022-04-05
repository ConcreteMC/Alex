using Alex.Interfaces;
using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Entity.BlockEntities
{
	public class DoubleChestEntityModel : EntityModel
	{
		public DoubleChestEntityModel()
		{
			Description = new ModelDescription()
			{
				Identifier = "geometry.geometry.alex.doublechest",
				TextureHeight = 64,
				TextureWidth = 128,
				VisibleBoundsHeight = 2.5,
				VisibleBoundsWidth = 3,
				VisibleBoundsOffset = Primitives.Factory.Vector3(0f, 0.75f, 0f)
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
							Origin = Primitives.Factory.Vector3(15, 0, -7),
							Size = Primitives.Factory.Vector3(30, 10, 14),
							Uv = EntityModelUV.FromIVector2(Primitives.Factory.Vector2(0, 19))
						}
					}
				},
				new EntityModelBone()
				{
					Name = "head",
					Parent = "body",
					Pivot = Primitives.Factory.Vector3(0, 10, 7),
					Cubes = new[]
					{
						new EntityModelCube()
						{
							Origin = Primitives.Factory.Vector3(-15, 9, -7),
							Size = Primitives.Factory.Vector3(30, 5, 14)
						},
						new EntityModelCube()
						{
							Origin = Primitives.Factory.Vector3(-1, 7, -8),
							Size = Primitives.Factory.Vector3(2, 4, 1)
						}
					}
				}
			};
		}
	}
}