using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.ResourcePackLib;
using Alex.Worlds;
using fNbt;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Items;
using NLog;

namespace Alex.Entities.BlockEntities
{
	public class BlockEntityFactory
	{
		private static readonly Logger          Log = LogManager.GetCurrentClassLogger(typeof(BlockEntityFactory));
		internal static         PooledTexture2D ChestTexture      { get; set; }
		internal static         PooledTexture2D EnderChestTexture { get; set; }
		internal static         PooledTexture2D SkullTexture      { get; set; }
		internal static         PooledTexture2D SignTexture      { get; set; }
		
		public static void LoadResources(GraphicsDevice graphicsDevice, ResourceManager resources)
		{
			if (resources.TryGetBitmap("minecraft:entity/chest/normal", out var bmp))
			{
				ChestTexture = TextureUtils.BitmapToTexture2D(graphicsDevice, bmp);
			}
			else
			{
				Log.Warn($"Could not load chest texture.");
			}
			
			if (resources.TryGetBitmap("minecraft:entity/chest/ender", out var enderBmp))
			{
				EnderChestTexture = TextureUtils.BitmapToTexture2D(graphicsDevice, enderBmp);
			}
			else
			{
				Log.Warn($"Could not load enderchest texture");
			}
			
			if (resources.TryGetBitmap("minecraft:entity/steve", out var steveBmp))
			{
				SkullTexture = TextureUtils.BitmapToTexture2D(graphicsDevice, steveBmp);
			}
			else
			{
				Log.Warn($"Could not load skull texture");
			}
			
						
			if (resources.TryGetBitmap("minecraft:entity/signs/oak", out var signBmp))
			{
				SignTexture = TextureUtils.BitmapToTexture2D(graphicsDevice, signBmp);
			}
			else
			{
				Log.Warn($"Could not load sign texture");
			}
		}
		
		public static BlockEntity ReadFrom(NbtCompound compound, World world, Block block)
		{
			if (compound.TryGet("id", out var tag) || compound.TryGet("ID", out tag))
			{
				var id = tag.StringValue;

				BlockEntity blockEntity = null;

				switch (id.ToLower())
				{
					case "minecraft:chest":
					case "chest":
						blockEntity = new ChestBlockEntity(block, world, ChestTexture);

						break;
					case "minecraft:ender_chest":
					case "ender_chest":
					case "enderchest":
						blockEntity = new EnderChestBlockEntity(block, world, EnderChestTexture);
						break;

					case "minecraft:sign":
					case "sign":
						blockEntity = new SignBlockEntity(world, block);

						break;
					
					case "minecraft:skull":
					case "skull":
						blockEntity = new SkullBlockEntity(world, block, SkullTexture);
						break;
					
					case "minecraft:flowerpot":
					case "flowerpot":
						blockEntity = new FlowerPotBlockEntity(world, block);
						break;
					
					case "minecraft:itemframe":
					case "itemframe":
						blockEntity = new ItemFrameBlockEntity(world, block);
						break;
					
					case "minecraft:furnace":
					case "furnace":
						blockEntity = new FurnaceBlockEntity(world, block);
						break;
					
					default:
						Log.Debug($"Missing block entity type: {id}");

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