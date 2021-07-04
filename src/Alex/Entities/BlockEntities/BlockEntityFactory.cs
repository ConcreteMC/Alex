using Alex.Blocks.Minecraft;
using Alex.Common.Graphics.GpuResources;
using Alex.Common.Utils;
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
		internal static         Texture2D DoubleChestTexture      { get; set; }
		internal static         Texture2D ChestTexture      { get; set; }
		internal static         Texture2D EnderChestTexture { get; set; }
		internal static         Texture2D SkullTexture      { get; set; }
		internal static         Texture2D SignTexture       { get; set; }
		
		public static void LoadResources(GraphicsDevice graphicsDevice, ResourceManager resources)
		{
			if (resources.TryGetBitmap("minecraft:entity/chest/normal", out var bmp))
			{
				ChestTexture = TextureUtils.BitmapToTexture2D($"BlockEntityFactory/Chest", graphicsDevice, bmp);
				//ChestTexture?.Use();
			}
			else
			{
				Log.Warn($"Could not load chest texture.");
			}
			
			if (resources.TryGetBedrockBitmap("minecraft:textures/entity/chest/double_normal", out var doubleBmp))
			{
				DoubleChestTexture = TextureUtils.BitmapToTexture2D("BlockEntityFactory/Chest/Double", graphicsDevice, doubleBmp);
				//DoubleChestTexture?.Use();
			}
			else
			{
				Log.Warn($"Could not load double chest texture.");
			}
			
			if (resources.TryGetBitmap("minecraft:entity/chest/ender", out var enderBmp))
			{
				EnderChestTexture = TextureUtils.BitmapToTexture2D("BlockEntityFactory/Chest/Ender", graphicsDevice, enderBmp);
				//EnderChestTexture?.Use();
			}
			else
			{
				Log.Warn($"Could not load enderchest texture");
			}
			
			if (resources.TryGetBitmap("minecraft:entity/steve", out var steveBmp))
			{
				SkullTexture = TextureUtils.BitmapToTexture2D("BlockEntityFactory/Steve", graphicsDevice, steveBmp);
				//SkullTexture?.Use(this);
			}
			else
			{
				Log.Warn($"Could not load skull texture");
			}
			
						
			if (resources.TryGetBitmap("minecraft:entity/signs/oak", out var signBmp))
			{
				SignTexture = TextureUtils.BitmapToTexture2D("BlockEntityFactory/Signs/Oak", graphicsDevice, signBmp);
				//SignTexture?.Use(this);
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
					case "minecraft:bed":
					case "bed":
						blockEntity = new BedBlockEntity(world);
						break;
					
					case "minecraft:chest":
					case "chest":
						blockEntity = new ChestBlockEntity(world);

						break;
					case "minecraft:ender_chest":
					case "ender_chest":
					case "enderchest":
						blockEntity = new EnderChestBlockEntity(world);
						break;

					case "minecraft:sign":
					case "sign":
						blockEntity = new SignBlockEntity(world);

						break;
					
					case "minecraft:skull":
					case "skull":
						blockEntity = new SkullBlockEntity(world, SkullTexture);
						break;
					
					case "minecraft:flowerpot":
					case "flowerpot":
						blockEntity = new FlowerPotBlockEntity(world);
						break;
					
					case "minecraft:itemframe":
					case "itemframe":
						blockEntity = new ItemFrameBlockEntity(world);
						break;
					
					case "minecraft:furnace":
					case "furnace":
						blockEntity = new FurnaceBlockEntity(world);
						break;
					
					default:
						Log.Debug($"Missing block entity type: {id}");

						break;
				}

				if (blockEntity != null)
				{
					blockEntity.Read(compound);
					
					if (blockEntity.SetBlock(block))
						return blockEntity;
				}
			}

			return null;
		}
	}
}