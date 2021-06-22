using System.Linq;
using Alex.Blocks.Minecraft;
using Alex.Common.Utils;
using Alex.Graphics.Models.Entity;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.Worlds;
using fNbt;

namespace Alex.Entities.BlockEntities
{
	public class BedBlockEntity : BlockEntity
	{
		private EntityDescription _entityDescription = null;
		/// <inheritdoc />
		public BedBlockEntity(World level, Block block) : base(level, block)
		{
			Width = 1f;
			Height = 1f;

			if (Alex.Instance.Resources.TryGetEntityDefinition(
				"minecraft:bed", out var entityDescription, out var source))
			{
				_entityDescription = entityDescription;
			}
		}

		/// <inheritdoc />
		protected override void ReadFrom(NbtCompound compound)
		{
			base.ReadFrom(compound);
			
		}

		/// <inheritdoc />
		protected override void BlockChanged(Block oldBlock, Block newBlock)
		{
			base.BlockChanged(oldBlock, newBlock);

			if (newBlock == null || !(newBlock is Bed))
				return;

			if (ModelRenderer == null)
			{
				if (ModelFactory.TryGetModel("geometry.bed", out var geo))
				{
					if (EntityModelRenderer.TryGetRenderer(geo, out var renderer))
					{
						ModelRenderer = renderer;
					}
				}

				if (_entityDescription != null)
				{
					if (_entityDescription.Textures.TryGetValue("default", out var texturePath))
					{
						if (Alex.Instance.Resources.TryGetBedrockBitmap(texturePath, out var bmp))
						{
							Texture = TextureUtils.BitmapToTexture2D(this, Alex.Instance.GraphicsDevice, bmp);
						}
					}
				}
			}
		}

		public enum BedColor
		{
			White,
			Orange,
			Magenta,
			LightBlue,
			Yellow,
			Lime,
			Pink,
			Gray,
			Silver,
			Cyan,
			Purple,
			Blue,
			Brown,
			Green,
			Red,
			Black
		}
	}
}