using Alex.API.Utils;
using Alex.Blocks;

namespace Alex.Worlds.Generators.Structures
{
	public class CactusStructure : Structure
	{
		public CactusStructure() : this(3)
		{

		}

		private int _height;

		public CactusStructure(int height)
		{
			_height = height;
		}

		public override void Create(ChunkColumn chunk, int x, int y, int z)
		{
			if (chunk.GetBlock(x, y - 1, z).Name != "minecraft:sand") return; //Not sand, do not generate.

			var growth = Rnd.Next(0x1, 0x15);
			for (int modifiedY = y; modifiedY < y + _height; modifiedY++)
			{
				if (!CheckSafe(chunk, x, modifiedY, z)) break;

				chunk.SetBlock(x, modifiedY, z, BlockFactory.GetBlock(81, (byte) growth)); //Cactus block
				//chunk.SetMetadata(x, modifiedY, z, (byte)growth);
			}
		}

		public override void Create(World level, int x, int y, int z)
		{
			if (level.GetBlock(x, y - 1, z).Name != "minecraft:sand") return; //Not sand, do not generate.

			var growth = Rnd.Next(1, 15);
			for (int modifiedY = y; modifiedY < y + _height; modifiedY++)
			{
				if (!CheckSafe(level, x, modifiedY, z)) break;

				Block b = BlockFactory.GetBlock(81, (byte) growth);
				b.Coordinates = new BlockCoordinates(x, modifiedY, z);

				level.SetBlock(b);
			}
		}

		private bool CheckSafe(ChunkColumn chunk, int x, int y, int z)
		{
			if (!chunk.GetBlock(x - 1, y , z).IsReplacible) return false;
			if (!chunk.GetBlock(x + 1, y, z).IsReplacible) return false;
			if (!chunk.GetBlock(x, y, z - 1).IsReplacible) return false;
			if (!chunk.GetBlock(x, y, z + 1).IsReplacible) return false;

			return true;
		}

		private bool CheckSafe(World level, int x, int y, int z)
		{
			if (!level.GetBlock(new BlockCoordinates(x - 1, y, z)).IsReplacible) return false;
			if (!level.GetBlock(new BlockCoordinates(x + 1, y, z)).IsReplacible) return false;
			if (!level.GetBlock(new BlockCoordinates(x, y, z - 1)).IsReplacible) return false;
			if (!level.GetBlock(new BlockCoordinates(x, y, z + 1)).IsReplacible) return false;

			return true;
		}
	}
}
