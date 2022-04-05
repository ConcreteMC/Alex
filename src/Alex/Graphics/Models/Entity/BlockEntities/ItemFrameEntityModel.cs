using Alex.Interfaces;
using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Entity.BlockEntities
{
	public class ItemFrameEntityModel : EntityModel
	{
		public ItemFrameEntityModel()
		{
			Description = new ModelDescription()
			{
				Identifier = "geometry.alex.item_frame", TextureHeight = 32, TextureWidth = 64
			};

			Bones = new[]
			{
				new EntityModelBone()
				{
					Name = "head",
					Cubes = new[]
					{
						new EntityModelCube()
						{
							Origin = Primitives.Factory.Vector3(-4f, 0f, -4f),
							Size =Primitives.Factory.Vector3(8f, 8f, 8f)
						},
					}
				}
			};
		}
	}
}