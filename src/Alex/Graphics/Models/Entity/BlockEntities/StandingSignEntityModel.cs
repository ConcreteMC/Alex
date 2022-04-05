using Alex.Interfaces;
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
					//BindPoseRotation = Primitives.Factory.Vector3(0f, 90f, 0f),
					Cubes = new[]
					{
						new EntityModelCube()
						{
							Origin =Primitives.Factory.Vector3(-8, 6f, -1f),
							Size = Primitives.Factory.Vector3(16, 11, 1),
							Uv = EntityModelUV.FromIVector2(Primitives.Factory.Vector2(9, 2))
						},
						new EntityModelCube()
						{
							Origin = Primitives.Factory.Vector3(-1, 0f, -0.8f),
							Size = Primitives.Factory.Vector3(2, 14, 2),
							Uv = EntityModelUV.FromIVector2(Primitives.Factory.Vector2(0, 14))
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
					Pivot = Primitives.Factory.Vector3(0f, 0f, 0f),
					Cubes = new[]
					{
						new EntityModelCube()
						{
							Origin = Primitives.Factory.Vector3(-8f, 2.5f, 7f),
							Size = Primitives.Factory.Vector3(16, 11, 1),
							Uv = EntityModelUV.FromIVector2(Primitives.Factory.Vector2(9, 2))
						}
					}
				}
			};
		}
	}
}