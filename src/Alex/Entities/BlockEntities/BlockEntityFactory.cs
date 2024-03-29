using Alex.Common.Resources;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Interfaces;
using Alex.Interfaces.Resources;
using Alex.Worlds;
using fNbt;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Entities.BlockEntities
{
	public class BlockEntityFactory
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BlockEntityFactory));
		internal static Texture2D DoubleChestTexture { get; set; }
		internal static Texture2D ChestTexture { get; set; }
		internal static Texture2D EnchantingTableBookTexture { get; set; }
		internal static Texture2D EnderChestTexture { get; set; }
		internal static Texture2D SkullTexture { get; set; }
		internal static Texture2D SignTexture { get; set; }

		public static void LoadResources(GraphicsDevice graphicsDevice, ResourceManager resources)
		{
			if (resources.TryGetBitmap("minecraft:entity/chest/normal", out var bmp))
			{
				var old = ChestTexture;
				old?.Dispose();

				ChestTexture = TextureUtils.BitmapToTexture2D($"BlockEntityFactory/Chest", graphicsDevice, bmp);
				ChestTexture.Tag = 0;
				//ChestTexture?.Use();
			}
			else
			{
				Log.Warn($"Could not load chest texture.");
			}

			if (resources.TryGetBitmap("minecraft:entity/enchanting_table_book", out var enchantingTableBookBmp))
			{
				var old = EnchantingTableBookTexture;
				old?.Dispose();

				EnchantingTableBookTexture = TextureUtils.BitmapToTexture2D(
					$"BlockEntityFactory/EnchantingTableTexture", graphicsDevice, enchantingTableBookBmp);

				EnchantingTableBookTexture.Tag = 0;
				//ChestTexture?.Use();
			}
			else
			{
				Log.Warn($"Could not load enchanting table book texture.");
			}

			if (resources.TryGetBedrockBitmap("minecraft:textures/entity/chest/double_normal", out var doubleBmp))
			{
				var old = DoubleChestTexture;
				old?.Dispose();

				DoubleChestTexture = TextureUtils.BitmapToTexture2D(
					"BlockEntityFactory/Chest/Double", graphicsDevice, doubleBmp);

				DoubleChestTexture.Tag = 0;
				//DoubleChestTexture?.Use();
			}
			else
			{
				Log.Warn($"Could not load double chest texture.");
			}

			if (resources.TryGetBitmap("minecraft:entity/chest/ender", out var enderBmp))
			{
				var old = EnderChestTexture;
				old?.Dispose();

				EnderChestTexture = TextureUtils.BitmapToTexture2D(
					"BlockEntityFactory/Chest/Ender", graphicsDevice, enderBmp);

				EnderChestTexture.Tag = 0;
				//EnderChestTexture?.Use();
			}
			else
			{
				Log.Warn($"Could not load enderchest texture");
			}

			if (resources.TryGetBitmap("minecraft:entity/steve", out var steveBmp))
			{
				var old = SkullTexture;
				old?.Dispose();

				SkullTexture = TextureUtils.BitmapToTexture2D("BlockEntityFactory/Steve", graphicsDevice, steveBmp);
				SkullTexture.Tag = 0;
				//SkullTexture?.Use(this);
			}
			else
			{
				Log.Warn($"Could not load skull texture");
			}


			if (resources.TryGetBitmap("minecraft:entity/signs/oak", out var signBmp))
			{
				var old = SignTexture;
				old?.Dispose();

				SignTexture = TextureUtils.BitmapToTexture2D("BlockEntityFactory/Signs/Oak", graphicsDevice, signBmp);
				SignTexture.Tag = 0;
				//SignTexture?.Use(this);
			}
			else
			{
				Log.Warn($"Could not load sign texture");
			}
		}

		public static BlockEntity GetById(ResourceLocation id,
			World world,
			IVector3I blockCoordinates,
			NbtCompound compound = null)
		{
			BlockEntity blockEntity = null;

			switch (id.Path.ToLower())
			{
				case "bed":
					blockEntity = new BedBlockEntity(world);

					break;

				case "chest":
					blockEntity = new ChestBlockEntity(world);

					break;

				case "ender_chest":
				case "enderchest":
					blockEntity = new EnderChestBlockEntity(world);

					break;
				
				case "trapped_chest":
				case "trappedchest":
					blockEntity = new TrappedChestBlockEntity(world);
					break;

				case "sign":
					blockEntity = new SignBlockEntity(world);

					break;

				case "skull":
					blockEntity = new SkullBlockEntity(world, SkullTexture);

					break;

				case "flowerpot":
					blockEntity = new FlowerPotBlockEntity(world);

					break;

				case "item_frame":
				case "itemframe":
					blockEntity = new ItemFrameBlockEntity(world);

					break;

				case "furnace":
					blockEntity = new FurnaceBlockEntity(world);

					break;

				case "banner":
				case "wall_banner":
				case "white_banner":
				case "white_wall_banner":
				case "orange_banner":
				case "orange_wall_banner":
				case "magenta_banner":
				case "magenta_wall_banner":
				case "light_blue_banner":
				case "light_blue_wall_banner":
				case "yellow_banner":
				case "yellow_wall_banner":
				case "lime_banner":
				case "lime_wall_banner":
				case "pink_banner":
				case "pink_wall_banner":
				case "gray_banner":
				case "gray_wall_banner":
				case "light_gray_banner":
				case "light_gray_wall_banner":
				case "cyan_banner":
				case "cyan_wall_banner":
				case "purple_banner":
				case "purple_wall_banner":
				case "blue_banner":
				case "blue_wall_banner":
				case "brown_banner":
				case "brown_wall_banner":
				case "green_banner":
				case "green_wall_banner":
				case "red_banner":
				case "red_wall_banner":
				case "black_banner":
				case "black_wall_banner":
					blockEntity = new BannerBlockEntity(world, blockCoordinates);

					break;

				case "enchanting_table":
				case "enchanttable":
					blockEntity = new EnchantTableBlockEntity(world);

					break;

				case "hopper":
					blockEntity = new HopperBlockEntity(world);

					break;

				case "beacon":
					blockEntity = new BeaconBlockEntity(world, blockCoordinates);

					break;

				case "barrel":
					blockEntity = new BarrelBlockEntity(world);

					break;
				
				default:
					Log.Warn($"Missing block entity type: {id}");

					break;
			}

			if (blockEntity != null)
			{
				if (blockEntity.Type == null)
				{
					blockEntity.Type = id;
				}

				if (compound != null)
				{
					blockEntity.Read(compound);
				}
				else
				{
					blockEntity.X = blockCoordinates.X;
					blockEntity.Y = blockCoordinates.Y;
					blockEntity.Z = blockCoordinates.Z;
				}

				// var block = world.GetBlockState(blockCoordinates);

				//    blockEntity.KnownPosition = blockCoordinates;
				world.SetBlockEntity(blockCoordinates.X, blockCoordinates.Y, blockCoordinates.Z, blockEntity);
			}

			return blockEntity;
		}

		public static BlockEntity ReadFrom(NbtCompound compound, World world, IVector3I blockCoordinates)
		{
			BlockEntity blockEntity = null;
			ResourceLocation id = string.Empty;

			if (compound != null && (compound.TryGet("id", out var tag) || compound.TryGet("ID", out tag)))
			{
				id = tag.StringValue;
			}
			/*else if(block != null)
			{
			    id = block.Location.Path;
			}*/

			blockEntity = GetById(id, world, blockCoordinates, compound);

			return blockEntity;
		}
	}
}