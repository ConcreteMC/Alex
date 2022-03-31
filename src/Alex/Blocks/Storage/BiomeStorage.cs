using System;
using System.IO;
using Alex.Blocks.Storage.Palette;
using Alex.Worlds;
using MiNET.Utils;
using NLog;

namespace Alex.Blocks.Storage;

public class BiomeStorage : GenericStorage<Biome>
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BiomeStorage));
	private Biome Air { get; }

	public BiomeStorage(int bitsPerBlock = 3, int size = 64, int x = 4, int y = 4, int z = 4) : base(BiomeUtils.Biomes[0], bitsPerBlock, size)
	{
		Air = BiomeUtils.Biomes[0];
		X = x;
		Y = y;
		Z = z;
		SmallestValue = 0;
		MaxBitsPerEntry = 3;
	}

	/// <inheritdoc />
	protected override Biome GetDefault()
	{
		return Air;
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
	
	public static IPalette<Biome> ReadPalette(Stream stream, uint blockSize)
	{
		var paletteCount = 1;
		if (blockSize != 0)
		{
			var paletteEntryCount = VarInt.ReadInt32(stream);

			if (paletteEntryCount <= 0)
			{
				Log.Warn($"Invalid palette entry count: {paletteEntryCount}");
				return null;
			}

			paletteCount = paletteEntryCount;
		}

		uint[] blocks = new uint[paletteCount];

		for (int i = 0; i < paletteCount; i++)
		{
			blocks[i] = (uint)VarInt.ReadInt32(stream);
		}
			
		var palette = new IntIdentityHashBiMap<Biome>(paletteCount + 1);
		palette.Add(BiomeUtils.GetBiome(0));

		for (uint id = 0; id < paletteCount; id++)
		{
			uint stateId = blocks[id];
			var state = BiomeUtils.GetBiome(stateId);
			palette.Put(state, id);
		}

		return palette;
	}
}