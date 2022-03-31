using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Alex.Blocks.State;
using Alex.Blocks.Storage.Palette;
using fNbt;
using MiNET.Utils;
using NLog;

namespace Alex.Blocks.Storage
{
	public class BlockStorage : GenericStorage<BlockState>
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BlockStorage));

		public BlockStorage(int bitsPerBlock = 8, int size = 4096) : base(BlockFactory.GetBlockState("minecraft:air"), bitsPerBlock, size)
		{
			X = Y = Z = 16;
		}

		/// <inheritdoc />
		protected override int GetIndex(int x, int y, int z)
		{
			return y << 8 | z << 4 | x;
		}

		/// <inheritdoc />
		protected override int CalculateDirectPaletteSize()
		{
			return (int)Math.Ceiling(Math.Log2(BlockFactory.AllBlockstates.Count));
		}

		/// <inheritdoc />
		protected override DirectPalette<BlockState> GetGlobalPalette()
		{
			return new DirectPalette<BlockState>(GlobalLookup);
		}

		private static BlockState GlobalLookup(uint arg)
		{
			return BlockFactory.GetBlockState(arg);
		}
	}
}