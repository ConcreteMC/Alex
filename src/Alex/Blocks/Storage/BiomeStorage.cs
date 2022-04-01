using System;
using Alex.Blocks.Storage.Palette;
using Alex.Worlds;
using NLog;

namespace Alex.Blocks.Storage;

public class BiomeStorage : GenericStorage<Biome>
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BiomeStorage));
	public BiomeStorage(int bitsPerBlock = 3, int size = 64, int x = 4, int y = 4, int z = 4) : base(BiomeUtils.Biomes[0], bitsPerBlock, size)
	{
		X = x;
		Y = y;
		Z = z;
		SmallestValue = 0;
		MaxBitsPerEntry = 3;
	}

	/// <inheritdoc />
	protected override DirectPalette<Biome> GetGlobalPalette()
	{
		return new DirectPalette<Biome>(GlobalLookup);
	}

	private static Biome GlobalLookup(uint arg)
	{
		return BiomeUtils.GetBiome(arg); //.GetBlockState(arg);
	}

	/// <inheritdoc />
	protected override int CalculateDirectPaletteSize()
	{
		return (int)Math.Ceiling(Math.Log2(BiomeUtils.BiomeCount));
	}

	/// <inheritdoc />
	protected override int GetIndex(int x, int y, int z)
	{
		return ((y >> 2) << 4) | ((z >> 2) << 2) | (x >> 2);
	}
}