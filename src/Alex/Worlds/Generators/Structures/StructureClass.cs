using System;
using Alex.API.Utils;
using Alex.Blocks;

namespace Alex.Worlds.Generators.Structures
{
	public class BlockEntry
	{
		public int Id;
		public byte Metadata;

		public BlockCoordinates Coordinates;

		public BlockEntry(int v)
		{
			this.Id = v;
		}
	}

	public class Structure
	{
		protected readonly Random Rnd = new Random();
		public virtual string Name
		{
			get { return null; }
		}

		public virtual BlockEntry[] Blocks
		{
			get { return null; }
		}

		public virtual int MaxHeight { get { return 0; } }

		public virtual void Create(ChunkColumn chunk, int x, int y, int z)
		{
			if (chunk.GetBlock(x, y + MaxHeight, z).IsReplacible)
			{
				foreach (BlockEntry b in Blocks)
				{
					chunk.SetBlock(x + b.Coordinates.X, y + b.Coordinates.Y, z + b.Coordinates.Z, BlockFactory.GetBlock(b.Id, b.Metadata));
					//chunk.SetMetadata(x + b.Coordinates.X, y + b.Coordinates.Y, z + b.Coordinates.Z, b.Metadata);
				}
			}
		}

		public virtual void Create(World level, int x, int y, int z)
		{
			if (level.GetBlock(new BlockCoordinates(x, y + MaxHeight, z)).IsReplacible)
			{
				foreach (BlockEntry b in Blocks)
				{
					Block clone = BlockFactory.GetBlock(b.Id, b.Metadata);
					clone.Coordinates = new BlockCoordinates(x + b.Coordinates.X, y + b.Coordinates.Y, z + b.Coordinates.Z);
					
					level.SetBlock(clone);
				}
			}
		}
	}
}
