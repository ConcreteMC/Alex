using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Alex.API.Blocks.State;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using fNbt;
using JetBrains.Profiler.Api;
using MiNET;
using MiNET.Net;
using MiNET.Utils;
using NLog;
using StackExchange.Profiling;

namespace Alex.Worlds.Bedrock
{
    public class ChunkProcessor : IDisposable
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChunkProcessor));

	    //private static readonly IReadOnlyDictionary<int, int> PeToJava
	    static ChunkProcessor()
	    {
		    
	    }
	    
	    private bool UseAlexChunks { get; }
	    private BlockingCollection<QueuedChunk> QueuedChunks { get; }

	    public IReadOnlyDictionary<uint, MiNET.Blockstate> _blockStateMap { get; set; } =
		    new Dictionary<uint, MiNET.Blockstate>();
	    private Thread[] Threads { get; set; }
	    private CancellationToken CancellationToken { get; }
	    private int MaxThreads { get; }
        public ChunkProcessor(int workerThreads, bool useAlexChunks, CancellationToken cancellationToken)
        {
	        MaxThreads = workerThreads;
	        UseAlexChunks = useAlexChunks;
	        QueuedChunks = new BlockingCollection<QueuedChunk>();
	        CancellationToken = cancellationToken;
	        
	        Threads = new Thread[workerThreads];
	        //for(int i = 0; i < workerThreads; i++) DispatchWorker();
        }

        public void HandleChunkData(byte[] chunkData, int cx, int cz, Action<ChunkColumn> callback)
        {
	        ThreadPool.QueueUserWorkItem(o =>
	        {
		        HandleChunk(chunkData, cx, cz,
			       callback);
	        });
	        //QueuedChunks.Add(new QueuedChunk(chunkData, cx, cz, callback));

	        //DispatchWorker();
        }

        private object _workerLock = new object();
        private long _workers = 0;

        private void DispatchWorker()
        {
	        for (int i = 0; i < Threads.Length; i++)
	        {
		        if (Threads[i] == default || Threads[i].ThreadState == ThreadState.Unstarted ||
		            Threads[i].ThreadState == ThreadState.Stopped)
		        {
			        Threads[i] = new Thread(Worker)
			        {
				        Name = $"ChunkProcessing-{i}"
			        };
			        Threads[i].Start();
			        break;
		        }
	        }
        }

        private void Worker()
        {
	        try
	        {
		        // while (!CancellationToken.IsCancellationRequested)
		        {
			        while (QueuedChunks.TryTake(out var queuedChunk, 1000, CancellationToken))
			        {
				        HandleChunk(queuedChunk.ChunkData, queuedChunk.ChunkX, queuedChunk.ChunkZ,
					        queuedChunk.Callback);
			        }
		        }
	        }
	        catch (OperationCanceledException)
	        {
		        
	        }

	        Thread.Yield();
        }

        private static ConcurrentDictionary<uint, IBlockState> _convertedStates = new ConcurrentDictionary<uint, IBlockState>();

        private void HandleChunk(byte[] chunkData, int cx, int cz, Action<ChunkColumn> callback)
        {
	        MeasureProfiler.StartCollectingData();
	        var profiler = MiniProfiler.StartNew("BEToJavaColumn");

	        try
	        {
		        using (MemoryStream stream = new MemoryStream(chunkData))
		        {
			        NbtBinaryReader defStream = new NbtBinaryReader(stream, true);

			        int count = defStream.ReadByte();
			        if (count < 1)
			        {
				        Log.Warn("Nothing to read");
				        return;
			        }

			        ChunkColumn chunkColumn = new ChunkColumn();
			        chunkColumn.IsDirty = true;
			        chunkColumn.X = cx;
			        chunkColumn.Z = cz;

			        for (int s = 0; s < count; s++)
			        {
				        var section = chunkColumn.Sections[s] as ChunkSection;
				        if (section == null) section = new ChunkSection(s, true);

				        int version = defStream.ReadByte();

				        if (version == 1 || version == 8)
				        {
					        int storageSize = defStream.ReadByte();

					        for (int storage = 0; storage < storageSize; storage++)
					        {
						        int paletteAndFlag = defStream.ReadByte();
						        bool isRuntime = (paletteAndFlag & 1) != 0;
						        int bitsPerBlock = paletteAndFlag >> 1;
						        int blocksPerWord = (int) Math.Floor(32f / bitsPerBlock);
						        int wordCount = (int) Math.Ceiling(4096.0f / blocksPerWord);

						        uint[] words = new uint[wordCount];
						        for (int w = 0; w < wordCount; w++)
						        {
							        int word = defStream.ReadInt32();
							        words[w] = SwapBytes((uint) word);
						        }

						        uint[] pallete = new uint[0];

						        if (isRuntime)
						        {
							        int palleteSize = VarInt.ReadSInt32(stream);
							        pallete = new uint[palleteSize];

							        for (int pi = 0; pi < pallete.Length; pi++)
							        {
								        var ui = (uint) VarInt.ReadSInt32(stream);
								        pallete[pi] = ui;
							        }

							        if (palleteSize == 0)
							        {
								        Log.Warn($"Pallete size is 0");
								        continue;
							        }
						        }

						        int position = 0;
						        for (int w = 0; w < wordCount; w++)
						        {
							        uint word = words[w];
							        for (int block = 0; block < blocksPerWord; block++)
							        {
								        if (position >= 4096) break; // padding bytes

								        uint state =
									        (uint) ((word >> ((position % blocksPerWord) * bitsPerBlock)) &
									                ((1 << bitsPerBlock) - 1));
								        int x = (position >> 8) & 0xF;
								        int y = position & 0xF;
								        int z = (position >> 4) & 0xF;

								        if (storage == 0)
								        {
									        if (state >= pallete.Length)
									        {
										        continue;
									        }

									        IBlockState translated = _convertedStates.GetOrAdd(pallete[state],
										        u =>
										        {
											        if (_blockStateMap.TryGetValue(pallete[state], out var bs))
											        {

												        var result =
													        BlockFactory.RuntimeIdTable.FirstOrDefault(xx =>
														        xx.Name == bs.Name);

												        if (result != null && result.Id >= 0)
												        {
													        var reverseMap = MiNET.Worlds.AnvilWorldProvider.Convert.FirstOrDefault(map =>
														        map.Value.Item1 == result.Id);

													        var id = result.Id;
													        if (reverseMap.Value != null)
													        {
														        id = reverseMap.Key;
													        }
													        
													        var res = BlockFactory.GetBlockStateID(
														        (int) id,
														        (byte) bs.Data);

													        if (AnvilWorldProvider.BlockStateMapper.TryGetValue(
														        res,
														        out var res2))
													        {
														        
														        var t = BlockFactory.GetBlockState(res2);
														        t = TranslateBlockState(t, id,
															        bs.Data);

														        return t;
													        }
													        else
													        {
														        Log.Info(
															        $"Did not find anvil statemap: {result.Name}");
														        return TranslateBlockState(
															        BlockFactory.GetBlockState(result.Name),
															        id, bs.Data);
													        }
												        }

												        return TranslateBlockState(
													        BlockFactory.GetBlockState(bs.Name),
													        -1, bs.Data);
											        }

											        return null;
										        });

									        if (translated != null)
									        {
										        section.Set(x, y, z, translated);
									        }
								        }
								        else
								        {
									        //TODO.
								        }

								        position++;
							        }

							        if (position >= 4096) break;
						        }
					        }
				        }
				        else
				        {
					        #region OldFormat 

					        byte[] blockIds = new byte[4096];
					        defStream.Read(blockIds, 0, blockIds.Length);

					        NibbleArray data = new NibbleArray(4096);
					        defStream.Read(data.Data, 0, data.Data.Length);

					        for (int x = 0; x < 16; x++)
					        {
						        for (int z = 0; z < 16; z++)
						        {
							        for (int y = 0; y < 16; y++)
							        {
								        int idx = (x << 8) + (z << 4) + y;
								        var id = blockIds[idx];
								        var meta = data[idx];

								        IBlockState result = null;

								        if (id > 0 && result == null)
								        {
									        var res = BlockFactory.GetBlockStateID(id, meta);

									        if (AnvilWorldProvider.BlockStateMapper.TryGetValue(res,
										        out var res2))
									        {
										        var t = BlockFactory.GetBlockState(res2);
										        t = TranslateBlockState(t, id,
											        meta);

										        result = t;
									        }
									        else
									        {
										        Log.Info($"Did not find anvil statemap: {result.Name}");
										        result = TranslateBlockState(BlockFactory.GetBlockState(res),
											        id, meta);
									        }
								        }

								        if (result == null)
								        {
									        var results = BlockFactory.RuntimeIdTable.Where(xx =>
										        xx.Id == id && xx.Data == meta).ToArray();

									        if (results.Length > 0)
									        {
										        result = TranslateBlockState(
											        BlockFactory.GetBlockState((uint) results[0].RuntimeId), id,
											        meta);
									        }
								        }

								        if (result != null)
								        {
									        section.Set(x, y, z, result);
								        }
							        }
						        }
					        }

					        #endregion
				        }

				        if (UseAlexChunks)
				        {
					        //  Log.Info($"Alex chunk!");
					        
					        var rawSky = new Utils.NibbleArray(4096);
					        defStream.Read(rawSky.Data, 0, rawSky.Data.Length);
					        
					        var rawBlock = new Utils.NibbleArray(4096);
					        defStream.Read(rawBlock.Data, 0, rawBlock.Data.Length);

					        for (int x = 0; x < 16; x++)
					        for (int y = 0; y < 16; y++)
					        for (int z = 0; z < 16; z++)
					        {
						        var peIndex = (x * 256) + (z * 16) + y;
						        var sky = rawSky[peIndex];
						        var block = rawBlock[peIndex];

						        var idx = y << 8 | z << 4 | x;
						        
						        section.SkyLight[idx] = sky;
						        section.BlockLight[idx] = block;
					        }
				        }

				        section.RemoveInvalidBlocks();
				        section.IsDirty = true;

				        //Make sure the section is saved.
				        chunkColumn.Sections[s] = section;
			        }


			        byte[] ba = new byte[512];
			        if (defStream.Read(ba, 0, 256 * 2) != 256 * 2) Log.Error($"Out of data height");

			        Buffer.BlockCopy(ba, 0, chunkColumn.Height, 0, 512);

			        int[] biomeIds = new int[256];
			        for (int i = 0; i < biomeIds.Length; i++)
			        {
				        biomeIds[i] = defStream.ReadByte();
			        }

			        chunkColumn.BiomeId = biomeIds;

			        if (stream.Position >= stream.Length - 1)
			        {
				        callback?.Invoke(chunkColumn);
				        return;
			        }

			        int borderBlock = VarInt.ReadSInt32(stream);
			        if (borderBlock > 0)
			        {
				        byte[] buf = new byte[borderBlock];
				        int len = defStream.Read(buf, 0, borderBlock);
			        }


			        if (stream.Position < stream.Length - 1)
			        {
				        while (stream.Position < stream.Length)
				        {
					        NbtFile file = new NbtFile()
					        {
						        BigEndian = false,
						        UseVarInt = true
					        };

					        file.LoadFromStream(stream, NbtCompression.None);
				        }
			        }

			        if (stream.Position < stream.Length - 1)
			        {
				        Log.Warn(
					        $"Still have data to read\n{Packet.HexDump(defStream.ReadBytes((int) (stream.Length - stream.Position)))}");
			        }

			        //Done processing this chunk, send to world
			        callback?.Invoke(chunkColumn);
		        }

	        }
	        catch (Exception ex)
	        {
		        Log.Error($"Exception in chunk loading: {ex.ToString()}");
	        }
	        finally
	        {
		        profiler?.Stop();
		        MeasureProfiler.SaveData();
	        }
        }
        
        private uint SwapBytes(uint x)
        {
	        // swap adjacent 16-bit blocks
	        x = (x >> 16) | (x << 16);
	        // swap adjacent 8-bit blocks
	        return ((x & 0xFF00FF00) >> 8) | ((x & 0x00FF00FF) << 8);
        }

        const string facing = "facing";
		private IBlockState FixFacing(IBlockState state, int meta)
		{
			switch (meta)
			{
				case 4:
				case 0:
					state = state.WithProperty(facing, "east");
					break;
				case 5:
				case 1:
					state = state.WithProperty(facing, "west");
					break;
				case 6:
				case 2:
					state = state.WithProperty(facing, "south");
					break;
				case 7:
				case 3:
					state = state.WithProperty(facing, "north");
					break;
			}

			return state;
		}
		
		private IBlockState FixFacingTrapdoor(IBlockState state, int meta)
		{
			switch (meta)
			{
				case 4:
				case 0:
					state = state.WithProperty(facing, "east");
					break;
				case 5:
				case 1:
					state = state.WithProperty(facing, "west");
					break;
				case 6:
				case 2:
					state = state.WithProperty(facing, "south");
					break;
				case 7:
				case 3:
					state = state.WithProperty(facing, "north");
					break;
			}

			return state;
		}
		
		private static string[] _slabs = new string[]
		{
			"minecraft:stone_slab",
			"minecraft:smooth_stone_slab",
			"minecraft:stone_brick_slab",
			"minecraft:sandstone_slab",
			"minecraft:acacia_slab",
			"minecraft:birch_slab",
			"minecraft:dark_oak_slab",
			"minecraft:jungle_slab",
			"minecraft:oak_slab",
			"minecraft:spruce_slab",
			"minecraft:purpur_slab",
			"minecraft:quartz_slab",
			"minecraft:red_sandstone_slab",
			"minecraft:brick_slab",
			"minecraft:cobblestone_slab",
			"minecraft:nether_brick_slab",
			"minecraft:petrified_oak_slab",
			"minecraft:prismarine_slab",
			"minecraft:prismarine_brick_slab",
			"minecraft:dark_prismarine_slab",
			"minecraft:polished_granite_slab",
			"minecraft:smooth_red_sandstone_slab",
			"minecraft:mossy_stone_brick_slab",
			"minecraft:polished_diorite_slab",
			"minecraft:mossy_cobblestone_slab",
			"minecraft:end_stone_brick_slab",
			"minecraft:smooth_sandstone_slab",
			"minecraft:smooth_quartz_slab",
			"minecraft:granite_slab",
			"minecraft:andesite_slab",
			"minecraft:red_nether_brick_slab",
			"minecraft:polished_andesite_slab",
			"minecraft:diorite_slab",
			"minecraft:cut_sandstone_slab",
			"minecraft:cut_red_sandstone_slab"
		};

		internal IBlockState TranslateBlockState(IBlockState state, long bid, int meta)
		{
			//var dict = state.ToDictionary();

			if (bid >= 8 && bid <= 11) //water or lava
			{
				state = state.WithProperty("level", meta.ToString());
			}
			else if (bid == 44 || bid == 182 || bid == 126 /*|| _slabs.Any(x => x.Equals(state.Name, StringComparison.InvariantCultureIgnoreCase))*/) //Slabs
			{
				var isUpper = (meta & 0x08) == 0x08;
				state = state.WithProperty("type", isUpper ? "top" : "bottom", true);
				
			} 
			else if (bid == 77 || bid == 143) //Buttons
			{
				switch (meta & ~0x08)
				{
					case 0:
					case 4:
						state = state.WithProperty(facing, "west");
						break;
					case 1:
					case 5:
						state = state.WithProperty(facing, "east");
						break;
					case 6:
					case 2:
						state = state.WithProperty(facing, "north");
						break;
					case 7:
					case 3:
						state = state.WithProperty(facing, "south");
						break;
				}
				
				state = state.WithProperty("powered", (meta & 0x08) == 0x08 ? "true" : "false");
			}
			else if (bid == 69 || state.Name.Contains("lever")) //Lever
			{
				state = FixFacing(state, meta & ~0x08);
				var modifiedMeta = meta & ~0x08;
				if (modifiedMeta >= 1 && modifiedMeta <= 4)
				{
					state = state.WithProperty("face", "wall");
				}
				else if (modifiedMeta == 7 || modifiedMeta == 0)
				{
					state = state.WithProperty("face", "floor");
				}
				else if (modifiedMeta == 6 || modifiedMeta == 5)
				{
					state = state.WithProperty("face", "ceiling");
				}
				
				switch (modifiedMeta)
				{
					case 1:
						state = state.WithProperty(facing, "east");
						break;
					case 2:
						state = state.WithProperty(facing, "west");
						break;
					case 3:
						state = state.WithProperty(facing, "south");
						break;
					case 4:
						state = state.WithProperty(facing, "north");
						break;
				}

				state = state.WithProperty("powered", (meta & 0x08) == 0x08 ? "true" : "false");
			}
			else if (bid == 65) //Ladder
			{
				var face = ((BlockFace) meta).ToString();
				state = state.WithProperty(facing, face);
			}
			//Stairs
			else if (bid == 163 || bid == 135 || bid == 108 || bid == 164 || bid == 136 || bid == 114 ||
			         bid == 53 ||
			         bid == 203 || bid == 156 || bid == 180 || bid == 128 || bid == 134 || bid == 109 || bid == 67)
			{
				//state = FixFacing(state, meta);
				
				state = ((BlockState)state).WithPropertyNoResolve("half", (meta & 0x04) == 0x04 ? "top" : "bottom");
				
				switch (meta & ~0x04)
				{
					case 0:
						state = state.WithProperty(facing, "east", false, "waterlogged", "shape", "half");
						break;
					case 1:
						state = state.WithProperty(facing, "west", false, "waterlogged", "shape", "half");
						break;
					case 2:
						state = state.WithProperty(facing, "south", false, "waterlogged", "shape", "half");
						break;
					case 3:
						state = state.WithProperty(facing, "north", false, "waterlogged", "shape", "half");
						break;
				}
			}
			else if (bid == 96 || bid == 167 || state.Name.Contains("trapdoor")) //Trapdoors
			{
				state = FixFacingTrapdoor(state, (meta & ~0x04) & ~0x08);
				state = state.WithProperty("half", ((meta & (1 << 0x04)) != 0 ? "top" : "bottom"));
				state = state.WithProperty("open", ((meta & (1 << 0x08)) != 0 ? "false" : "true"));
			}
			else if (bid == 106 || state.Name.Contains("vine"))
			{
				state = FixVinesRotation(state, meta);
			}
			else if (bid == 64 || bid == 71 || bid == 193 || bid == 194 || bid == 195 || bid == 196 || bid == 197) //Doors
			{
				var isUpper = (meta & 0x08) == 0x08;
				
				if (isUpper)
				{
					//state = state.WithProperty("hinge", (meta & 0x01) == 0x01 ? "right" : "left");
					state = state.WithProperty("half", "upper");
				}
				else
				{
					bool isOpen = (meta & 0x04) == 0x04;
					state = state.WithProperty("half", "lower");
					state = state.WithProperty("open", isOpen ? "true" : "false");
					state = FixFacing(state, (meta & 0x3));
				}
			}
			return state;
		}

		private IBlockState FixVinesRotation(IBlockState state, int meta)
		{
			const byte North = 0x04;
			const byte East = 0x08;
			const byte West = 0x02;
			const byte South = 0x01;
			
			bool hasNorth = (meta & North) == North;
			bool hasEast = (meta & East) == East;
			bool hasSouth = (meta & South) == South;
			bool hasWest = (meta & West) == West;

			state = state.WithProperty("east", hasEast.ToString())
				.WithProperty("north", hasNorth.ToString())
				.WithProperty("south", hasSouth.ToString())
				.WithProperty("west", hasWest.ToString());

			/*bool hasNorthTop = (onTop.Metadata & North) == North;
			bool hasEastTop = (onTop.Metadata & East) == East;
			bool hasSouthTop = (onTop.Metadata & South) == South;
			bool hasWestTop = (onTop.Metadata & West) == West;

			bool haveFaceBlock = false;

			if (hasNorth && level.GetBlock(Coordinates + Level.South).IsSolid)
			{
				haveFaceBlock = true;
			}
			else if (hasEast && level.GetBlock(Coordinates + Level.West).IsSolid)
			{
				haveFaceBlock = true;
			}
			else if (hasSouth && level.GetBlock(Coordinates + Level.North).IsSolid)
			{
				haveFaceBlock = true;
			}
			else if (hasWest && level.GetBlock(Coordinates + Level.East).IsSolid)
			{
				haveFaceBlock = true;
			}

			bool hasVineTop = false;
			if (hasNorth && hasNorthTop)
			{
				hasVineTop = true;
			}
			else if (hasEast && hasEastTop)
			{
				hasVineTop = true;
			}
			else if (hasSouth && hasSouthTop)
			{
				hasVineTop = true;
			}
			else if (hasWest && hasWestTop)
			{
				hasVineTop = true;
			}*/

			return state;
		}
		
		private class QueuedChunk
		{
			public byte[] ChunkData { get; }
			public int ChunkX { get; }
			public int ChunkZ { get; }
			public Action<ChunkColumn> Callback { get; }
			public QueuedChunk(byte[] data, int x, int z, Action<ChunkColumn> callback)
			{
				ChunkX = x;
				ChunkZ = z;
				ChunkData = data;
				Callback = callback;
			}
		}

		public void Dispose()
		{
			QueuedChunks?.Dispose();
		}
    }
}