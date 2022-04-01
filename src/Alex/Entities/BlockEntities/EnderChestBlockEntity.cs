using Alex.Common.Resources;
using Alex.Graphics.Models.Entity.BlockEntities;
using Alex.Utils;
using Alex.Worlds;

namespace Alex.Entities.BlockEntities
{
	public class EnderChestBlockEntity : ChestBlockEntity
	{
		/// <inheritdoc />
		public EnderChestBlockEntity(World level) : base(level)
		{
			Type = new ResourceLocation("minecraft:enderchest");
			if (new ChestEntityModel().TryGetRenderer(out var renderer))
			{
				ModelRenderer = renderer;
			}

			Texture = BlockEntityFactory.EnderChestTexture;
		}
	}
}