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
using Newtonsoft.Json;
using NLog;
using StackExchange.Profiling;
using BlockState = Alex.Blocks.State.BlockState;

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

	    public IReadOnlyDictionary<uint, MiNET.Utils.BlockRecord> _blockStateMap { get; set; } =
		    new Dictionary<uint, MiNET.Utils.BlockRecord>();
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

	        if (!Directory.Exists("failed"))
		        Directory.CreateDirectory("failed");
        }

        public void HandleChunkData(bool cacheEnabled, uint subChunkCount, byte[] chunkData, int cx, int cz, Action<ChunkColumn> callback)
        {
	        ThreadPool.QueueUserWorkItem(o =>
	        {
		        HandleChunk(cacheEnabled, subChunkCount, chunkData, cx, cz,
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
				        HandleChunk(queuedChunk.CacheEnabled, queuedChunk.SubChunkCount, queuedChunk.ChunkData, queuedChunk.ChunkX, queuedChunk.ChunkZ,
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
        
        private List<string> Failed { get; set; } = new List<string>();
        private IBlockState GetBlockState(uint palleteId)
        {
	        return _convertedStates.GetOrAdd(palleteId,
		        u =>
		        {
			        if (_blockStateMap.TryGetValue(palleteId, out var bs))
			        {
				        if (TryConvertBlockState(bs, out var convertedState))
				        {
					        return convertedState;
				        }

				        var t = TranslateBlockState(
					        BlockFactory.GetBlockState(bs.Name),
					        -1, bs.Data);

				        if (t.Name == "Unknown" && !Failed.Contains(bs.Name))
				        {
					        Failed.Add(bs.Name);
					        File.WriteAllText(Path.Combine("failed", bs.Name + ".json"), JsonConvert.SerializeObject(bs, Formatting.Indented));
				        }
			        }

			        return null;
		        });
        }

        private void HandleChunk(bool cacheEnabled, uint subChunkCount, byte[] chunkData, int cx, int cz, Action<ChunkColumn> callback)
        {
	        if (cacheEnabled)
	        {
		        Log.Warn($"Unsupported cache enabled!");
	        }
	        MeasureProfiler.StartCollectingData();
	        var profiler = MiniProfiler.StartNew("BEToJavaColumn");

	        try
	        {
		        using (MemoryStream stream = new MemoryStream(chunkData))
		        {
			        NbtBinaryReader defStream = new NbtBinaryReader(stream, true);

			        //int count = defStream.ReadByte();
			        if (subChunkCount < 1)
			        {
				        Log.Warn("Nothing to read");
				        return;
			        }

			        ChunkColumn chunkColumn = new ChunkColumn();
			        chunkColumn.IsDirty = true;
			        chunkColumn.X = cx;
			        chunkColumn.Z = cz;

			        for (int s = 0; s < subChunkCount; s++)
			        {
				        var section = chunkColumn.Sections[s] as ChunkSection;

				        int version = defStream.ReadByte();

				        if (version == 1 || version == 8)
				        {
					        int storageSize = defStream.ReadByte();
					        
					        if (section == null) 
						        section = new ChunkSection(s, true, storageSize);

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

								        if (state >= pallete.Length)
								        {
									        continue;
								        }

								        IBlockState translated = GetBlockState(pallete[state]);

								        if (translated != null)
								        {
									        section.Set(storage, x, y, z, translated);
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
									        var reverseMap = MiNET.Worlds.AnvilWorldProvider.Convert.FirstOrDefault(map =>
										        map.Value.Item1 == id);

									        if (reverseMap.Value != null)
									        {
										        id = (byte) reverseMap.Key;
									        }
									        
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
										        xx.Id == id).ToArray();

									        if (results.Length > 0)
									        {
										        var first = results.FirstOrDefault(xx => xx.Data == meta);
										        if (first == default)
											        first = results[0];
										        
										        result = TranslateBlockState(
											        BlockFactory.GetBlockState((uint) first.RuntimeId), id,
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


			       /* byte[] ba = new byte[512];
			        if (defStream.Read(ba, 0, 256 * 2) != 256 * 2) Log.Error($"Out of data height");

			        Buffer.BlockCopy(ba, 0, chunkColumn.Height, 0, 512);*/

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

					        if (file.RootTag.Name == "alex")
					        {
						        NbtCompound alexCompound = (NbtCompound) file.RootTag;

						        for (int ci = 0; ci < subChunkCount; ci++)
						        {
							        var section = (ChunkSection) chunkColumn.Sections[ci];

							        var rawSky = new Utils.NibbleArray(4096);
							        if (alexCompound.TryGet($"skylight-{ci}", out NbtByteArray skyData))
							        {
								        rawSky.Data = skyData.Value;
							        }
							        //defStream.Read(rawSky.Data, 0, rawSky.Data.Length);

							        var rawBlock = new Utils.NibbleArray(4096);
							        if (alexCompound.TryGet($"blocklight-{ci}", out NbtByteArray blockData))
							        {
								        rawBlock.Data = blockData.Value;
							        }

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

							        chunkColumn.Sections[ci] = section;
						        }
					        }
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

        private string GetWoodBlock(BlockRecord record)
        {
	        string type = "oak";
	        bool stripped = false;
	        string axis = "y";
	        
	        foreach (var state in record.States)
	        {
		        switch (state.Name)
		        {
			        case "wood_type":
				        type = state.Value;
				        break;
			        case "stripped_bit":
				        stripped = state.Value == "1";
				        break;
			        case "pillar_axis":
				        axis = state.Value;
				        break;
		        }
	        }

	        string result = $"{type}_log";
	        if (stripped)
	        {
		        result = "stripped_" + result;
	        }

	        return $"minecraft:{result}";
        }

        public bool TryConvertBlockState(BlockRecord record, out IBlockState result)
        {
	        if (_convertedStates.TryGetValue((uint) record.RuntimeId, out var alreadyConverted))
	        {
		        result = alreadyConverted;
		        return true;
	        }
	        
	        result = null;

	        string searchName = record.Name;
	        
	        switch (record.Name)
	        {
		        case "minecraft:torch":
			        if (record.States.Any(x => x.Name.Equals("torch_facing_direction") && x.Value != "top"))
			        {
				        searchName = "minecraft:wall_torch";
			        }
			        break;
		        case "minecraft:unlit_redstone_torch":
		        case "minecraft:redstone_torch":
			        if (record.States.Any(x => x.Name.Equals("torch_facing_direction") && x.Value != "top"))
			        {
				        searchName = "minecraft:redstone_wall_torch";
			        }
			        break;
		        case "minecraft:flowing_water":
			        searchName = "minecraft:water";
			        break;
		        case "minecraft:wood":
			        searchName = GetWoodBlock(record);
			        break;
	        }
	        
	        string prefix = "";
	        foreach (var state in record.States)
	        {
		        switch (state.Name)
		        {
			        case "stone_type":
				        switch (state.Value)
				        {
					        case "granite":
					        case "diorite":
					        case "andesite":
						        searchName = $"minecraft:{state.Value}";
						        break;
					        case "granite_smooth":
					        case "diorite_smooth":
							case "andesite_smooth":
						        var split = state.Value.Split('_');
						        searchName = $"minecraft:polished_{split[0]}";
						        break;
				        }
				        break;
			        case "old_log_type":
			        {
				        searchName = $"minecraft:{state.Value}_log";
			        }
				        break;
			        case "old_leaf_type":
				        searchName = $"minecraft:{state.Value}_leaves";
				        break;
			        case "wood_type":
				        switch (record.Name.ToLower())
				        {
					        case "minecraft:fence":
						        searchName = $"minecraft:{state.Value}_fence";
						        break;
					        case "minecraft:planks":
						        searchName = $"minecraft:{state.Value}_planks";
						        break;
					        case "minecraft:wooden_slab":
						        searchName = $"minecraft:{state.Value}_slab";
						        break;
					      //  case "minecraft:wood":
						  //      searchName = $"minecraft:{state.Value}_log";
						 //       break;
				        }

				        break;
			        case "sapling_type":
			        //case "old_log_type":
			       // case "old_leaf_type":
			        searchName = $"minecraft:{state.Value}_sapling";
				        //prefix = "_";
				        break;
			        case "flower_type":
				        searchName = $"minecraft:{state.Value}";
				        break;
		        }
	        }
	        
	        IBlockState r;// = BlockFactory.GetBlockState(record.Name);

	        r = BlockFactory.GetBlockState(prefix + searchName);

	        if (r == null || r.Name == "Unknown")
	        {
		        r = BlockFactory.GetBlockState(searchName);
	        }
	        
	        if (r == null || r.Name == "Unknown")
	        {
		        var mapResult =
			        BlockFactory.RuntimeIdTable.FirstOrDefault(xx =>
				        xx.Name == searchName);

		        if (mapResult != null && mapResult.Id >= 0)
		        {
			        var reverseMap = MiNET.Worlds.AnvilWorldProvider.Convert.FirstOrDefault(map =>
				        map.Value.Item1 == mapResult.Id);

			        var id = mapResult.Id;
			        if (reverseMap.Value != null)
			        {
				        id = reverseMap.Key;
			        }
													        
			        var res = BlockFactory.GetBlockStateID(
				        (int) id,
				        (byte) record.Data);

			        if (AnvilWorldProvider.BlockStateMapper.TryGetValue(
				        res,
				        out var res2))
			        {
														        
				        r = BlockFactory.GetBlockState(res2);
			        }
			        else
			        {
				        Log.Info(
					        $"Did not find anvil statemap: {result.Name}");
				        r = BlockFactory.GetBlockState(mapResult.Name);
			        }
		        }
	        }

	        if (r == null || r.Name == "Unknown")
		        return false;

	        foreach (var state in record.States)
	        {
		        switch (state.Name)
		        {
			        case "direction":
			        case "weirdo_direction":
				        r = FixFacing(r, int.Parse(state.Value));
				        break;
			        case "upside_down_bit":
				        r = (r).WithProperty("half", state.Value == "1" ? "top" : "bottom");
				        break;
			        case "door_hinge_bit":
				        r = r.WithProperty("hinge", (state.Value == "0") ? "left" : "right");
				        break;
					case "open_bit":
						r = r.WithProperty("open", (state.Value == "1") ? "true" : "false");
						break;
					case "upper_block_bit":
						r = r.WithProperty("half", (state.Value == "1") ? "upper" : "lower");
						break;
					case "torch_facing_direction":
						r = r.WithProperty("facing", state.Value);
						break;
					case "liquid_depth":
						r = r.WithProperty("level", state.Value);
						break;
					case "height":
						r = r.WithProperty("layers", state.Value);
						break;
					case "growth":
						r = r.WithProperty("age", state.Value);
						break;
					case "button_pressed_bit":
						r = r.WithProperty("powered", state.Value == "1" ? "true" : "false");
						break;
					case "facing_direction":
						switch (int.Parse(state.Value))
						{
							case 0:
							case 4:
								r = r.WithProperty(facing, "west");
								break;
							case 1:
							case 5:
								r = r.WithProperty(facing, "east");
								break;
							case 6:
							case 2:
								r = r.WithProperty(facing, "north");
								break;
							case 7:
							case 3:
								r = r.WithProperty(facing, "south");
								break;
						}
						break;
					case "head_piece_bit":
						r = r.WithProperty("part", state.Value == "1" ? "head" : "foot");
						break;
					case "pillar_axis":
						r = r.WithProperty("axis", state.Value);
						break;
					case "top_slot_bit":
						r = r.WithProperty("type", state.Value == "1" ? "top" : "bottom", true);
						break;
		        }
	        }

	        if (record.Name.Equals("minecraft:unlit_redstone_torch"))
	        {
		        r = r.WithProperty("lit", "false");
	        }

	        result = r;
	        
	        return true;
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
			else if (bid == 50) //Torch
			{
				if (meta >= 1 && meta <= 4)
				{
					
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
			public bool CacheEnabled { get; set; }
			public uint SubChunkCount { get; set; }
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