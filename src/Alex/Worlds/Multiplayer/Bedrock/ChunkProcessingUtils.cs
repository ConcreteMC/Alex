using System;
using System.Collections.Generic;
using System.IO;
using fNbt;
using MiNET.Blocks;
using MiNET.Utils;
using NLog;

namespace Alex.Worlds.Multiplayer.Bedrock;

public interface IPaletteEncoding
{
	void Encode(Stream stream, uint value);
	uint Decode(Stream stream);
}

public interface IChunkEncoding
{
	
}

public class BiomePaletteEncoding : IPaletteEncoding
{
	public BiomePaletteEncoding()
	{
		
	}
	/// <inheritdoc />
	public void Encode(Stream stream, uint value)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public uint Decode(Stream stream)
	{
		return (uint) VarInt.ReadSInt32(stream);
	}
}

public class BlockPaletteEncoding : IPaletteEncoding
{
	private readonly bool _isNetwork;

	public BlockPaletteEncoding(bool isNetwork)
	{
		_isNetwork = isNetwork;
	}
	/// <inheritdoc />
	public void Encode(Stream stream, uint value)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public uint Decode(Stream stream)
	{
		if (_isNetwork)
		{
			return (uint) VarInt.ReadSInt32(stream);
		}

		var file = new NbtFile { BigEndian = false, UseVarInt = true };
		file.LoadFromStream(stream, NbtCompression.None);
		var tag = (NbtCompound)file.RootTag;

		var block = MiNET.Blocks.BlockFactory.GetBlockByName(tag["name"].StringValue);

		if (block != null && block.GetType() != typeof(Block) && !(block is Air))
		{
			List<IBlockState> blockState = ReadBlockState(tag);
			block.SetState(blockState);
		}
		else
		{
			block = new MiNET.Blocks.Air();
		}

		return (uint) block.GetRuntimeId();
	}
	
	private static List<IBlockState> ReadBlockState(NbtCompound tag)
	{
		//Log.Debug($"Palette nbt:\n{tag}");

		var states = new List<IBlockState>();
		var nbtStates = (NbtCompound)tag["states"];

		foreach (NbtTag stateTag in nbtStates)
		{
			IBlockState state = stateTag.TagType switch
			{
				NbtTagType.Byte => (IBlockState)new BlockStateByte()
				{
					Name = stateTag.Name, Value = stateTag.ByteValue
				},
				NbtTagType.Int    => new BlockStateInt() { Name = stateTag.Name, Value = stateTag.IntValue },
				NbtTagType.String => new BlockStateString() { Name = stateTag.Name, Value = stateTag.StringValue },
				_                 => throw new ArgumentOutOfRangeException()
			};

			states.Add(state);
		}

		return states;
	}
}

public static class ChunkProcessingUtils
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChunkProcessingUtils));
	
	public static uint[] ReadWordArray(Stream stream, byte bitsPerBlock)
	{
		var wordCount = PaletteSize(bitsPerBlock);

		if (bitsPerBlock == 0 || wordCount == 0)
			return null;
		
		uint[] words = new uint[wordCount];
		for (int i = 0; i < wordCount; i++)
		{
			if (stream.Position + 4 >= stream.Length)
				return null;
			
			words[i] = (uint) ((byte)stream.ReadByte()) | (uint) ((byte)stream.ReadByte()) << 8 | (uint) ((byte)stream.ReadByte()) << 16 | (uint) ((byte)stream.ReadByte()) << 24;
		}
		//var byteCount = wordCount * 4;

		/*var data = new byte[byteCount];

		int read = stream.Read(data, 0, data.Length);
		if (read != data.Length)
		{
			Log.Warn($"Not enough data! Got {read} of {data.Length}");
			return null;
		}*/
			
		/*uint[] words = new uint[wordCount];
		for (int i = 0; i < wordCount; i++)
		{
			var baseIndex = i * 4;
			var word =  (uint) (data[baseIndex]) | (uint) (data[baseIndex + 1]) << 8 | (uint) (data[baseIndex + 2]) << 16 | (uint) (data[baseIndex + 3]) << 24;

			words[i] = word;
		}*/

		return words;
	}
	
	public static uint[] ReadPalette(Stream stream, uint blockSize, IPaletteEncoding encoding)
	{
		var paletteCount = 1;
		
		if (blockSize != 0)
		{
			paletteCount = VarInt.ReadSInt32(stream);
		}
		
		//var paletteEntryCount = VarInt.ReadSInt32(stream.BaseStream);

		if (paletteCount <= 0)
		{
			//Log.Warn($"Invalid palette entry count: {paletteEntryCount}");
			return null;
		}

		uint[] blocks = new uint[paletteCount];

		for (int i = 0; i < paletteCount; i++)
		{
			blocks[i] = encoding.Decode(stream);
		}

		return blocks;
	}

	public static uint PaletteSize(int bitsPerBlock)
	{
		//return 4096 / (32 / bitsPerBlock);
		var indicesPerUint = (int) Math.Floor(32d / bitsPerBlock);
		return (uint) Math.Ceiling(4096d / indicesPerUint);
	}

	public static bool IsPadded(int size)
	{
		return size == 3 || size == 5 || size == 6;
	}
}