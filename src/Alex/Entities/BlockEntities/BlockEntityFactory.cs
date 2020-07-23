using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.ResourcePackLib;
using Alex.Worlds;
using fNbt;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Entities.BlockEntities
{
	public class BlockEntityFactory
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BlockEntityFactory));
		private static PooledTexture2D ChestTexture { get; set; }

		public static void LoadResources(GraphicsDevice graphicsDevice, McResourcePack resourcePack)
		{
			if (resourcePack.TryGetBitmap("minecraft:entity/chest/normal", out var bmp))
			{
				ChestTexture = TextureUtils.BitmapToTexture2D(graphicsDevice, bmp);
			}
			else
			{
				Log.Warn($"Could not load chest texture.");
			}
		}
		
		public static BlockEntity ReadFrom(NbtCompound compound, World world, Block block)
		{
			if (compound.TryGet("id", out var tag))
			{
				var id = tag.StringValue;

				BlockEntity blockEntity = null;

				switch (id)
				{
					case "minecraft:chest":
					case "chest":
						blockEntity = new ChestBlockEntity(block, world, ChestTexture);

						break;

					default:
						Log.Warn($"Missing block entity type: {id}");

						break;
				}

				if (blockEntity != null)
				{
					blockEntity.Read(compound);
				}

				return blockEntity;
			}

			return null;
		}
	}
}