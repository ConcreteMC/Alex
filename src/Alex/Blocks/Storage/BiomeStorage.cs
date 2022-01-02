using System;
using Alex.Blocks.Storage.Palette;
using Alex.Worlds;

namespace Alex.Blocks.Storage;

public class BiomeStorage : GenericStorage<Biome>
{
	private Biome Air { get; }
	public BiomeStorage() : base(BiomeUtils.Biomes[0], 3, 64)
	{
		Air = BiomeUtils.Biomes[0];
		X = Y = Z = 4;
		SmallestValue = 0;
		MaxBitsPerEntry = 3;
	}

	/// <inheritdoc />
	protected override Biome GetDefault()
	{
		return Air;
	}
	
	/// <inheritdoc />
	protected override DirectPallete<Biome> GetGlobalPalette()
	{
		return new DirectPallete<Biome>(GlobalLookup);
	}

	private static Biome GlobalLookup(uint arg)
	{
		return BiomeUtils.GetBiome(arg);//.GetBlockState(arg);
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