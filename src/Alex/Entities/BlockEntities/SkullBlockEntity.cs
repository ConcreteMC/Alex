using Alex.API.Graphics;
using Alex.Blocks.Minecraft;
using Alex.Entities.Models;
using Alex.Graphics.Models.Entity;
using Alex.Worlds;

namespace Alex.Entities.BlockEntities
{
	public class SkullBlockEntity : BlockEntity
	{
		/// <inheritdoc />
		public SkullBlockEntity(World level, Block block, PooledTexture2D texture) : base(level, block)
		{
			ModelRenderer = new EntityModelRenderer(new MobHeadModel(), texture);
			
			Width = 8;
			Height = 8;
		}

		/// <inheritdoc />
		protected override void BlockChanged(Block oldBlock, Block newBlock)
		{
			base.BlockChanged(oldBlock, newBlock);
		}

		/// <inheritdoc />
		public override void HandleBlockAction(byte actionId, int parameter)
		{
			base.HandleBlockAction(actionId, parameter);
		}
	}
}