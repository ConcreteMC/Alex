using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using Alex.API.Utils;
using Alex.Blocks;
using Alex.Blocks.Mapping;
using Alex.Blocks.Minecraft;
using Alex.Entities.BlockEntities;
using Alex.Utils;
using Alex.Worlds.Chunks;
using Alex.Worlds.Singleplayer;
using fNbt;
using MiNET;
using MiNET.Net;
using MiNET.Utils;
using NLog;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using BlockState = Alex.Blocks.State.BlockState;
using ChunkCoordinates = Alex.API.Utils.ChunkCoordinates;
using NibbleArray = MiNET.Utils.NibbleArray;

namespace Alex.Worlds.Multiplayer.Bedrock
{
    public class ChunkProcessor : IDisposable
    {
	    private static readonly Logger         Log = LogManager.GetCurrentClassLogger(typeof(ChunkProcessor));
	    public static           ChunkProcessor Instance { get; private set; }
	    static ChunkProcessor()
	    {
		    
	    }
	    
	    private bool UseAlexChunks { get; }

	    public IReadOnlyDictionary<uint, BlockStateContainer> BlockStateMap { get; set; } =
		    new Dictionary<uint, BlockStateContainer>();

	    public static Itemstates Itemstates { get; set; } = new Itemstates();
	    
	    private readonly ConcurrentDictionary<uint, uint> _convertedStates = new ConcurrentDictionary<uint, uint>();
	    
	    private CancellationToken CancellationToken  { get; }
	    public  bool              ClientSideLighting { get; set; } = true;

	    private BedrockClient    Client           { get; }
	    public BlobCache        Cache            { get; }

	    public ChunkProcessor(BedrockClient client, bool useAlexChunks, CancellationToken cancellationToken, BlobCache blobCache)
        {
	        Client = client;
	        UseAlexChunks = useAlexChunks;
	        CancellationToken = cancellationToken;
	        Cache = blobCache;

	        Instance = this;
        }

	    private ConcurrentQueue<KeyValuePair<ulong, byte[]>> _blobQueue =
		    new ConcurrentQueue<KeyValuePair<ulong, byte[]>>();
	    private ConcurrentQueue<ChunkCoordinates>            _actionQueue = new ConcurrentQueue<ChunkCoordinates>();

	    private ConcurrentDictionary<ChunkCoordinates, ChunkData> _actions =
		    new ConcurrentDictionary<ChunkCoordinates, ChunkData>();
        public void HandleChunkData(bool cacheEnabled,
	        ulong[] blobs,
	        uint subChunkCount,
	        byte[] chunkData,
	        int cx,
	        int cz,
	        Action<ChunkColumn> callback)
        {
	        if (CancellationToken.IsCancellationRequested || subChunkCount < 1)
		        return;

	        var coords = new ChunkCoordinates(cx, cz);

	        var value = new ChunkData(cacheEnabled, blobs, subChunkCount, chunkData, cx, cz, callback);
	        
	        bool updated = false;
	        _actions.AddOrUpdate(coords, value, (coordinates, action1) =>
	        {
		        updated = true;
		        return value;
	        });

	        // _actions.TryAdd(coords, );
	        if (!updated)
				_actionQueue.Enqueue(coords);

	        _resetEvent.Set();
	        
	        CheckThreads();
        }
        
        //public void AddAction(ChunkCoordinates co)

        private Thread           _workerThread = null;
        private object           _syncObj      = new object();
        private object           _threadSync   = new object();
        private ManualResetEvent _resetEvent   = new ManualResetEvent(false);
        private long             _threadHelper = 0;
        private void CheckThreads()
        {
	        if (!Monitor.TryEnter(_syncObj, 5))
		        return;

	        if (_actionQueue.IsEmpty && _blobQueue.IsEmpty)
		        return;
	        
	        try
	        {
		        if (_workerThread == null && Interlocked.CompareExchange(ref _threadHelper, 1, 0) == 0)
		        {
			        ThreadPool.QueueUserWorkItem(
				        o =>
				        {
					        lock (_threadSync)
					        {
						        _workerThread = Thread.CurrentThread;

						        while (!CancellationToken.IsCancellationRequested)
						        {
							        bool handled = false;
							        if (_actionQueue.TryDequeue(out var chunkCoordinates))
							        {
								        if (_actions.TryRemove(chunkCoordinates, out var chunk))
								        {
									        if (chunk.CacheEnabled)
									        {
										        HandleChunkCachePacket(chunk.Blobs, chunk.SubChunkCount, chunk.Data, chunk.X, chunk.Z, chunk.Callback);
										        return;
									        }

									        chunk.Callback?.Invoke(HandleChunk(chunk.SubChunkCount, chunk.Data, chunk.X, chunk.Z));
									        //chunk?.Invoke();
								        }

								        handled = true;
							        }
							        
							        if (_blobQueue.TryDequeue(out var kv))
							        {
								        ulong  hash = kv.Key;
								        byte[] data = kv.Value;

								        if (!Cache.Contains(hash))
											Cache.TryStore(hash, data);
								        // _blobs.TryAdd(hash, data);

								        var chunks = _futureChunks.Where(c => c.SubChunks.Contains(hash) || c.Biome == hash);
								        foreach (CachedChunk chunk in chunks)
								        {
									        //CachedChunk chunk = kvp.Key;

									        if (chunk.Biome == hash)
									        {
										        if (data.Length >= 256)
										        {
											        for (int x = 0; x < 16; x++)
											        {
												        for (int z = 0; z < 16; z++)
												        {
													        var biomeId = data[(z << 4) + (x)];

													        for (int y = 0; y < 255; y++)
													        {
														        chunk.Chunk.SetBiome(x, y, z, biomeId);
													        }
												        }
											        }

											        //  chunk.Chunk.biomeId = data;
											        chunk.Biome = 0;
										        }
									        }
									        else
									        {
										        for (int i = 0; i < chunk.SubChunks.Length; i++)
										        {
											        ulong subChunkHash = chunk.SubChunks[i];
											        if (subChunkHash == hash)
											        {
												        // parse data
												        chunk.Sections[i] = data;
												        // chunk.Chunk.Sections[i] = HandleChunk(1, data, chunk.X, chunk.Z).Sections[0];//ClientUtils.DecodeChunkColumn(1, data, BlockPalette, _internalStates).Sections[0];
												        //chunk.SubChunks[i] = 0;
												        chunk.SubChunkCount--;
											        }
										        }
									        }
			        
									        //TRYBUILD
									        chunk.TryBuild(this);
								        }

								        handled = true;
							        }
							        
							        if (!handled)
							        {
								        _resetEvent.Reset();
								        
								        if (!_resetEvent.WaitOne(50))
											break;
							        }
						        }

						        _threadHelper = 0;
						        _workerThread = null;
					        }
				        });
		        }
	        }
	        finally
	        {
		        Monitor.Exit(_syncObj);
	        }
        }

        private List<string> Failed     { get; set; } = new List<string>();
        public  bool         UseCaching { get; set; }

        public BlockState GetBlockState(uint p)
        {
	        if (_convertedStates.TryGetValue(p, out var existing))
	        {
		        return BlockFactory.GetBlockState(existing);
	        }
	        
	        if (!BlockStateMap.TryGetValue(p, out var bs))
	        {
		        var a = MiNET.Blocks.BlockFactory.BlockPalette.FirstOrDefault(x => x.RuntimeId == p);
		        bs = a;
	        }

	        if (bs != null)
	        {
		        var copy = new BlockStateContainer()
		        {
			        Data = bs.Data,
			        Id = bs.Id,
			        Name = bs.Name,
			        States = bs.States,
			        RuntimeId = bs.RuntimeId
		        };
				        
		        if (TryConvertBlockState(copy, out var convertedState))
		        {
			        if (convertedState != null)
			        {
				        _convertedStates.TryAdd(p, convertedState.ID);
			        }

			        return convertedState;
		        }

		        var t = TranslateBlockState(
			        BlockFactory.GetBlockState(copy.Name),
			        -1, copy.Data);

		        if (t.Name == "Unknown" && !Failed.Contains(copy.Name))
		        {
			        Failed.Add(copy.Name);

			        return t;
			        //  File.WriteAllText(Path.Combine("failed", bs.Name + ".json"), JsonConvert.SerializeObject(bs, Formatting.Indented));
		        }
		        else
		        {
			        _convertedStates.TryAdd(p, t.ID);
			        return t;
		        }
	        }

	        return null;
        }

        public class CachedChunk
        {
	        public int      X         { get; set; }
	        public int      Z         { get; set; }
	        public ulong[]  SubChunks { get; set; } = new ulong[16];
	        public byte[][] Sections  { get; set; } = new byte[16][];
	        public ulong    Biome     { get; set; }

	        public ChunkColumn         Chunk    { get; set; } = new ChunkColumn(0,0);
	        public Action<ChunkColumn> Callback { get; set; }
	        public uint               SubChunkCount = 0;

	        public bool TryBuild(ChunkProcessor processor)
	        {
		        if (SubChunkCount <= 0 && processor.Cache.TryGet(Biome, out var biomeIds))
		        {
			        if (biomeIds.Length >= 256){
				        for (int x = 0; x < 16; x++)
				        {
					        for (int z = 0; z < 16; z++)
					        {
						        var biomeId = biomeIds[(z << 4) + (x)];

						        for (int y = 0; y < 255; y++)
						        {
							        Chunk.SetBiome(x, y, z, biomeId);
						        }
					        }
				        }
			        }

			        processor._futureChunks.Remove(this);
			        
			        var coordinates = new ChunkCoordinates(Chunk.X, Chunk.Z);

			        foreach (KeyValuePair<BlockCoordinates, NbtCompound> bePair in processor._futureBlockEntities.Where(
				        be => (ChunkCoordinates) be.Key == coordinates))
			        {
				        Chunk.BlockEntities.TryAdd(bePair.Key, bePair.Value);
				        processor._futureBlockEntities.TryRemove(bePair.Key, out _);
			        }

			        for (int i = 0; i < 16; i++)
			        {
				        var sectionData = Sections[i];

				        if (sectionData == null)
				        {
					        processor.Cache.TryGet(SubChunks[i], out sectionData);
				        }
				        
				        if (sectionData != null && sectionData.Length > 0)
				        {
					        Chunk.Sections[i] = processor.HandleChunk(1, sectionData, coordinates.X, coordinates.Z).Sections[0];
				        }
			        }

			        Chunk.CalculateHeight();
			        Callback?.Invoke(Chunk);
			        //Client.Chunks[coordinates] = chunk.Chunk;

			        return true;
		        }

		        return false;
	        }
        }

        public ThreadSafeList<CachedChunk>                 _futureChunks        = new ThreadSafeList<CachedChunk>();
        public ConcurrentDictionary<BlockCoordinates, NbtCompound> _futureBlockEntities = new ConcurrentDictionary<BlockCoordinates, NbtCompound>();
        public void HandleClientCacheMissResponse(McpeClientCacheMissResponse message)
        {
	        foreach (KeyValuePair<ulong, byte[]> kv in message.blobs)
	        {
		        _blobQueue.Enqueue(kv);
	        }
	        
	        _resetEvent.Set();
	        
	        CheckThreads();
        }
        
        private void HandleChunkCachePacket(ulong[] blobs,
	        uint subChunkCount,
	        byte[] chunkData,
	        int cx,
	        int cz,
	        Action<ChunkColumn> callback)
        {
	        var chunk = new CachedChunk
	        {
		        X = cx,
		        Z = cz,
		        Callback = callback
	        };
	        chunk.Chunk.X = chunk.X;
	        chunk.Chunk.Z = chunk.Z;
	        chunk.SubChunkCount = subChunkCount;

	        var hits   = new List<ulong>();
	        var misses = new List<ulong>();

	        ulong biomeHash = blobs.Last();
	        chunk.Biome = biomeHash;
	        if (Cache.Contains(biomeHash))
	        { 
		        hits.Add(biomeHash);
	        }
	        else
	        {
		        misses.Add(biomeHash);
	        }

	        for (int i = 0; i < subChunkCount; i++)
	        {
		        ulong hash = blobs[i];
		        chunk.SubChunks[i] = hash;
		        
		        if (Cache.Contains(hash))
		        {
			        hits.Add(hash);
			        chunk.SubChunkCount--;
		        }
		        else
		        {
			        misses.Add(hash);
		        }
	        }
	        
	        if (misses.Count > 0)
		        _futureChunks.TryAdd(chunk);
	        
	        var status = McpeClientCacheBlobStatus.CreateObject();
	        status.hashHits = hits.ToArray();
	        status.hashMisses = misses.ToArray();
	        Client.SendPacket(status);

	        using (MemoryStream ms = new MemoryStream(chunkData))
	        {
		        ReadExtra(chunk.Chunk, ms);
	        }

	        if (chunk.SubChunkCount <= 0)
	        {
		        chunk.TryBuild(this);
	        }
        }
        
        private ChunkColumn HandleChunk(uint subChunkCount, byte[] chunkData, int cx, int cz)
        {
	         ChunkColumn chunkColumn = new ChunkColumn(cx, cz);
	      //  chunkColumn.IsDirty = true;
	       // chunkColumn.X = cx;
	      //  chunkColumn.Z = cz;

	        try
	        {
		        using (MemoryStream stream = new MemoryStream(chunkData))
		        {
			        using NbtBinaryReader defStream = new NbtBinaryReader(stream, true);

			        for (int s = 0; s < subChunkCount; s++)
			        {
				        var section = chunkColumn.Sections[s] as ChunkSection;

				        int version = defStream.ReadByte();

				        if (version == 1 || version == 8)
				        {
					        int storageSize = version == 1 ? 1 : defStream.ReadByte();

					        if (section == null)
						        section = new ChunkSection(true, storageSize);

					        for (int storage = 0; storage < storageSize; storage++)
					        {
						        int  flags         = stream.ReadByte();
						        bool isRuntime     = (flags & 1) != 0;
						        int  bitsPerBlock  = flags >> 1;
						        int  blocksPerWord = (int) Math.Floor(32f / bitsPerBlock);
						        int  wordsPerChunk = (int) Math.Ceiling(4096f / blocksPerWord);

						        long jumpPos = stream.Position;
						        stream.Seek(wordsPerChunk * 4, SeekOrigin.Current);

						        int paletteCount = VarInt.ReadSInt32(stream);
						        var palette      = new int[paletteCount];
						        bool allZero = true;
						        for (int j = 0; j < paletteCount; j++)
						        {
							        if (!isRuntime)
							        {
								        var file = new NbtFile
								        {
									        BigEndian = false,
									        UseVarInt = true
								        };
								        file.LoadFromStream(stream, NbtCompression.None);
								        var tag = (NbtCompound) file.RootTag;

								        var block =
									        MiNET.Blocks.BlockFactory.GetBlockByName(tag["name"].StringValue);
								        
								        if (block != null && block.GetType() != typeof(Block) && !(block is Air))
								        {
									        List<IBlockState> blockState = ReadBlockState(tag);
									        block.SetState(blockState);
								        }
								        else
								        {
									        block = new MiNET.Blocks.Air();
								        }

								        palette[j] = block.GetRuntimeId();
							        }
							        else
							        {
								        int runtimeId = VarInt.ReadSInt32(stream);
								        palette[j] = runtimeId;
								        //if (bedrockPalette == null || internalBlockPallet == null) continue;

								        // palette[j] = GetServerRuntimeId(bedrockPalette, internalBlockPallet, runtimeId);
							        }

							        if (palette[j] != 0)
								        allZero = false;
						        }

						        if (!allZero)
						        {
							        long afterPos = stream.Position;
							        stream.Position = jumpPos;
							        int position = 0;

							        for (int w = 0; w < wordsPerChunk; w++)
							        {
								        uint word = defStream.ReadUInt32();

								        for (uint block = 0; block < blocksPerWord; block++)
								        {
									        if (position >= 4096)
										        continue;

									        uint state = (uint) ((word >> ((position % blocksPerWord) * bitsPerBlock))
									                             & ((1 << bitsPerBlock) - 1));

									        int x = (position >> 8) & 0xF;
									        int y = position & 0xF;
									        int z = (position >> 4) & 0xF;

									        int runtimeId = palette[state];

									        if (runtimeId != 0)
									        {
										        var blockState = GetBlockState((uint) runtimeId);

										        if (blockState != null)
											        section.Set(storage, x, y, z, blockState);
									        }

									        position++;
								        }
							        }
							        
							        stream.Position = afterPos;
						        }
					        }
				        }
				        else if (version == 0)
				        {
					        if (section == null)
						        section = new ChunkSection(true, 1);

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
								        int idx  = (x << 8) + (z << 4) + y;
								        var id   = blockIds[idx];
								        var meta = data[idx];

								        //var ruid = BlockFactory.GetBlockStateID(id, meta);

								        BlockState result = GetBlockState(
									        MiNET.Blocks.BlockFactory.GetRuntimeId(id, meta));

								        if (result != null)
								        {
									        section.Set(x, y, z, result);
								        }
								        else
								        {
									        Log.Info($"Unknown block: {id}:{meta}");
								        }

							        }
						        }
					        }

					        #endregion
				        }
				        else
				        {
					        Log.Info($"Unsupported storage version: {version} - section: {s}");
				        }

				        if (section != null)
				        {
					        chunkColumn.Sections[s] = section;
				        }
			        }


			         byte[] biomeIds = new byte[256];

			         if (defStream.Read(biomeIds, 0, 256) != 256) return chunkColumn;
 
			       //  Buffer.BlockCopy(ba, 0, chunkColumn.Height, 0, 512);*/

			      //  int[] biomeIds = new int[256];
			       // for (int i = 0; i < biomeIds.Length; i++)
			       // {
				   //     biomeIds[i] = defStream.ReadByte();
			       // }

			        for (int x = 0; x < 16; x++)
			        {
				        for (int z = 0; z < 16; z++)
				        {
					        var biomeId = biomeIds[(z << 4) + (x)];

					        for (int y = 0; y < 255; y++)
					        {
						        chunkColumn.SetBiome(x, y, z, biomeId);
					        }
				        }
			        }
			        //chunkColumn.SetBiome();
			        
			       // chunkColumn.BiomeId = biomeIds;

			        if (stream.Position >= stream.Length - 1)
			        {
				        chunkColumn.CalculateHeight();
				       // callback?.Invoke(chunkColumn);
				       return chunkColumn;
			        }

			        ReadExtra(chunkColumn, stream);

			        chunkColumn.CalculateHeight();
			        
			        //Done processing this chunk, send to world
			        //callback?.Invoke(chunkColumn);
			        return chunkColumn;
		        }

	        }
	        catch (Exception ex)
	        {
		        Log.Error($"Exception in chunk loading: {ex.ToString()}");

		        return chunkColumn;
	        }
	        finally
	        {

	        }
        }

        private void ReadExtra(ChunkColumn chunkColumn, MemoryStream stream)
        {
	        int borderBlock = VarInt.ReadSInt32(stream);// defStream.ReadByte();// defStream.ReadVarInt(); //VarInt.ReadSInt32(stream);
	        if (borderBlock != 0)
	        {
		        int len   = (int) (stream.Length - stream.Position);
		        var bytes = new byte[len];
		        stream.Read(bytes, 0, len);
	        }

	        if (stream.Position < stream.Length - 1)
	        {
		        while (stream.Position < stream.Length - 1)
		        {
			        try
			        {
				        NbtFile file = new NbtFile() {BigEndian = false, UseVarInt = true};

				        file.LoadFromStream(stream, NbtCompression.None);
				        var blockEntityTag = file.RootTag;

				        int x = blockEntityTag["x"].IntValue;
				        int y = blockEntityTag["y"].IntValue;
				        int z = blockEntityTag["z"].IntValue;

				        chunkColumn.AddBlockEntity(new BlockCoordinates(x, y, z), (NbtCompound) file.RootTag);
			        }catch(EndOfStreamException){}

			        // if (Log.IsTraceEnabled()) Log.Trace($"Blockentity:\n{file.RootTag}");
		        }
	        }

	        if (stream.Position < stream.Length - 1)
	        {
		        int len   = (int) (stream.Length - stream.Position);
		        var bytes = new byte[len];
		        stream.Read(bytes, 0, len);
		        Log.Warn($"Still have data to read\n{Packet.HexDump(new ReadOnlyMemory<byte>(bytes))}");
	        }
        }

        private static List<IBlockState> ReadBlockState(NbtCompound tag)
        {
	        //Log.Debug($"Palette nbt:\n{tag}");

	        var states    = new List<IBlockState>();
	        var nbtStates = (NbtCompound) tag["states"];
	        foreach (NbtTag stateTag in nbtStates)
	        {
		        IBlockState state = stateTag.TagType switch
		        {
			        NbtTagType.Byte => (IBlockState) new BlockStateByte()
			        {
				        Name = stateTag.Name,
				        Value = stateTag.ByteValue
			        },
			        NbtTagType.Int => new BlockStateInt()
			        {
				        Name = stateTag.Name,
				        Value = stateTag.IntValue
			        },
			        NbtTagType.String => new BlockStateString()
			        {
				        Name = stateTag.Name,
				        Value = stateTag.StringValue
			        },
			        _ => throw new ArgumentOutOfRangeException()
		        };
		        states.Add(state);
	        }

	        return states;
        }

        private static int GetServerRuntimeId(BlockPalette bedrockPalette, HashSet<BlockStateContainer> internalBlockPallet, int runtimeId)
        {
	        if (runtimeId < 0 || runtimeId >= bedrockPalette.Count) Log.Error($"RuntimeId = {runtimeId}");

	        var record = bedrockPalette[runtimeId];

	        if (!internalBlockPallet.TryGetValue(record, out BlockStateContainer internalRecord))
	        {
		        Log.Error($"Did not find {record.Id}");
		        return 0; // air
	        }

	        return internalRecord.RuntimeId;
        }

        private string GetWoodBlock(BlockStateContainer record)
        {
	        string type = "oak";
	        bool stripped = false;
	       // string axis = "y";
	        
	        foreach (var state in record.States)
	        {
		        switch (state.Name)
		        {
			        case "wood_type":
				        type = state.Value();
				        //record.States.Remove(state);
				        break;
			        case "stripped_bit":
				        stripped = state.Value() == "1";
				        //record.States.Remove(state);
				        break;
			        //case "pillar_axis":
				     //   axis = state.Value();
				     //   //record.States.Remove(state);
				     //   break;
		        }
	        }

	        string result = $"{type}_log";
	        if (stripped)
	        {
		        result = "stripped_" + result;
	        }

	        return $"minecraft:{result}";
        }

        private string BlockStateToString(BlockStateContainer record)
        {
	        StringBuilder sb = new StringBuilder();
	        sb.Append($"{record.Name}[");

	        var states = record.States.ToArray();
	        foreach (var state in states)
	        {
		        sb.Append($"{state.Name}={state.Value()},");
	        }

	        sb.Append("]");
	        
	        return sb.ToString();
        }

        private bool TryConvertBlockState(BlockStateContainer record, out BlockState result)
        {
	        if (_convertedStates.TryGetValue((uint) record.RuntimeId, out var alreadyConverted))
	        {
		        result = BlockFactory.GetBlockState(alreadyConverted);
		        return true;
	        }

	        if (BlockFactory.BedrockStates.TryGetValue(record.Name, out var bedrockStates))
	        {
		        var defaultState = bedrockStates.GetDefaultState();

		        if (defaultState != null)
		        {
			        if (record.States != null)
			        {
				        foreach (var prop in record.States)
				        {
					        if (!defaultState.Contains(prop.Name))
						        continue;
					        
					        defaultState = defaultState.WithProperty(
						        prop.Name, prop.Value());
				        }
			        }

			        if (defaultState is PeBlockState pebs)
			        {
				        result = pebs.Original;
				        return true;
			        }
			        else
			        {
				        result = defaultState;

				        return true;
			        }
			        //result = defaultState.Block.BlockState.CloneSilent();
		        }
	        }
	        
	        var originalRecord = BlockStateToString(record);
	        
	        result = null;

	        string searchName = record.Name;
	        
	        switch (record.Name)
	        {
		        case "minecraft:invisible_bedrock":
		        case "minecraft:invisibleBedrock":
			        searchName = "minecraft:barrier";
			        break;
		        case "minecraft:wall_sign":
			        searchName = "minecraft:oak_wall_sign";
			        break;
		        case "minecraft:spruce_wall_sign":
			        searchName = "minecraft:spruce_wall_sign";
			        break;
		        case "minecraft:birch_wall_sign":
			        searchName = "minecraft:birch_wall_sign";
			        break;
		        case "minecraft:jungle_wall_sign":
			        searchName = "minecraft:jungle_wall_sign";
			        break;
		        case "minecraft:acacia_wall_sign":
			        searchName = "minecraft:acacia_wall_sign";
			        break;
		        case "minecraft:darkoak_wall_sign":
			        searchName = "minecraft:dark_oak_wall_sign";
			        break;
		        case "minecraft:crimson_wall_sign":
			        searchName = "minecraft:crimson_wall_sign";
			        break;
		        case "minecraft:warped_wall_sign":
			        searchName = "minecraft:warped_wall_sign";
			        break;
		        
		        
		        case "minecraft:standing_sign":
			        searchName = "minecraft:oak_sign";
			        break;
		        case "minecraft:spruce_standing_sign":
			        searchName = "minecraft:spruce_sign";
			        break;
		        case "minecraft:birch_standing_sign":
			        searchName = "minecraft:birch_sign";
			        break;
		        case "minecraft:jungle_standing_sign":
			        searchName = "minecraft:jungle_sign";
			        break;
		        case "minecraft:acacia_standing_sign":
			        searchName = "minecraft:acacia_sign";
			        break;
		        case "minecraft:darkoak_standing_sign":
			        searchName = "minecraft:dark_oak_sign";
			        break;
		        case "minecraft:crimson_standing_sign":
			        searchName = "minecraft:crimson_sign";
			        break;
		        case "minecraft:warped_standing_sign":
			        searchName = "minecraft:warped_sign";
			        break;

		        case "minecraft:snow":
			        searchName = "minecraft:snow_block";
			        break;
		        case "minecraft:snow_layer":
			        searchName = "minecraft:snow";
			        break;
		        case "minecraft:torch":
			        if (record.States.Any(x => x.Name.Equals("torch_facing_direction") && x.Value() != "top"))
			        {
				        searchName = "minecraft:wall_torch";
			        }
			        break;
		        case "minecraft:unlit_redstone_torch":
		        case "minecraft:redstone_torch":
			        if (record.States.Any(x => x.Name.Equals("torch_facing_direction") && x.Value() != "top"))
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
		        case "minecraft:tallgrass":
			        searchName = "minecraft:tall_grass";
			        break;
		        case "minecraft:grass":
			        searchName = "minecraft:grass_block";

			        break;
	        }
	        
	        string prefix = "";
	        foreach (var state in record.States.ToArray())
	        {
		        switch (state.Name)
		        {
			        case "stone_type":
				        switch (state.Value())
				        {
					        case "granite":
					        case "diorite":
					        case "andesite":
						        searchName = $"minecraft:{state.Value()}";
						        break;
					        case "granite_smooth":
					        case "diorite_smooth":
					        case "andesite_smooth":
						        var split = state.Value().Split('_');
						        searchName = $"minecraft:polished_{split[0]}";
						        break;
				        }

				        break;
			        case "old_log_type":
			        {
				        searchName = $"minecraft:{state.Value()}_log";
			        }
				        break;
			        case "old_leaf_type":
				        searchName = $"minecraft:{state.Value()}_leaves";
				        break;
			        case "wood_type":
				        switch (record.Name.ToLower())
				        {
					        case "minecraft:fence":
						        searchName = $"minecraft:{state.Value()}_fence";
						        break;
					        case "minecraft:planks":
						        searchName = $"minecraft:{state.Value()}_planks";
						        break;
					        case "minecraft:wooden_slab":
						        searchName = $"minecraft:{state.Value()}_slab";
						        break;
					        //  case "minecraft:wood":
					        //      searchName = $"minecraft:{state.Value}_log";
					        //       break;
				        }

				        //record.States.Remove(state);
				        break;
			        case "sapling_type":
				        //case "old_log_type":
				        // case "old_leaf_type":
				        searchName = $"minecraft:{state.Value()}_sapling";
				        //prefix = "_";
				        break;

			        case "flower_type":
			        {
				        var sValue = state.Value();

				        if (sValue.StartsWith("tulip"))
				        {
					        var split = sValue.Split('_');
					        searchName = $"minecraft:{split[1]}_{split[0]}";
				        }
				        else
				        {
					        searchName = $"minecraft:{state.Value()}";
				        }
				        
				        break;
			        }

			        case "double_plant_type":

				        switch (state.Value())
				        {
					        case "grass":
						        searchName = "minecraft:tall_grass";
						        break;
					        case "sunflower":
						        searchName = "minecraft:sunflower";
						        break;
					        case "fern":
						        searchName = "minecraft:large_fern";
						        break;
					        case "rose":
						        searchName = "minecraft:rose_bush";
						        break;
					        case "paeonia":
						        searchName = "minecraft:peony";
						        break;
				        }

				        break;
			        case "color":
				        var val = state.Value();

				        if (val == "silver")
				        {
					        val = "light_gray";
				        }
				        
				        switch (record.Name)
				        {
					        case "minecraft:carpet":
						        searchName = $"minecraft:{val}_carpet";
						        break;
					        case "minecraft:wool":
						        searchName = $"minecraft:{val}_wool";
						        break;
					        case "minecraft:stained_glass":
						        searchName = $"minecraft:{val}_stained_glass";
						        break;
					        case "minecraft:concrete":
						        searchName = $"minecraft:{val}_concrete";
						        break;
					        case "minecraft:stained_glass_pane":
						        searchName = $"minecraft:{val}_stained_glass_pane";
						        break;
				        }

				        //record.States.Remove(state);
				        break;
		        }
	        }
	        
	        BlockState r;// = BlockFactory.GetBlockState(record.Name);

	        r = BlockFactory.GetBlockState(prefix + searchName);

	        if (r == null || r.Name == "Unknown")
	        {
		        r = BlockFactory.GetBlockState(searchName);
	        }

	        if (r == null || r.Name == "Unknown")
	        {
		        Log.Warn($"Could not translate block: {originalRecord} (Search: {prefix + searchName})");
		        return false;
	        }

	        foreach (var state in record.States)
	        {
		        switch (state.Name)
		        {
			        case "ground_sign_direction":
				        r = r.WithProperty("rotation", state.Value());
				        //record.States.Remove(state);
				        break;
			        case "disarmed_bit":
				        r = r.WithProperty("disarmed", state.Value() == "1" ? "true" : "false");
				        //record.States.Remove(state);
				        break;
			        case "powered_bit":
				        r = r.WithProperty("powered", state.Value() == "1" ? "true" : "false");
				        //record.States.Remove(state);
				        break;
			        case "attached_bit":
				        r = r.WithProperty("attached", state.Value() == "1" ? "true" : "false");
				        //record.States.Remove(state);
				        break;
			        case "direction":
			        case "weirdo_direction":
				        if (r.Block is FenceGate || r.Block is TripwireHook)
				        {
					        switch (state.Value())
					        {
						        case "0":
							        r = r.WithProperty(facing, "north");
							        break;
						        case "1":
							        r = r.WithProperty(facing, "west");
							        break;
						        case "2":
							        r = r.WithProperty(facing, "south");
							        break;
						        case "3":
							        r = r.WithProperty(facing, "east");
							        break;
					        }
				        }
				        else
				        {
					        r = FixFacing(r, int.Parse(state.Value()));
				        }
				        //record.States.Remove(state);
				        break;
			        case "upside_down_bit":
				        r = (r).WithProperty("half", state.Value() == "1" ? "top" : "bottom");
				        //record.States.Remove(state);
				        break;
			        case "door_hinge_bit":
				        r = r.WithProperty("hinge", (state.Value() == "0") ? "left" : "right");
				        //record.States.Remove(state);
				        break;
					case "open_bit":
						r = r.WithProperty("open", (state.Value() == "1") ? "true" : "false");
						//record.States.Remove(state);
						break;
					case "upper_block_bit":
						r = r.WithProperty("half", (state.Value() == "1") ? "upper" : "lower");
						//record.States.Remove(state);
						break;
					case "torch_facing_direction":
						string facingValue = state.Value();
						switch (facingValue)
						{
							case "north":
								facingValue = "south";
								break;
							case "east":
								facingValue = "west";
								break;
							case "south":
								facingValue = "north";
								break;
							case "west":
								facingValue = "east";
								break;
						}
						r = r.WithProperty("facing", facingValue);
						//record.States.Remove(state);
						break;
					case "liquid_depth":
						r = r.WithProperty("level", state.Value());
						//record.States.Remove(state);
						break;
					case "height":
						r = r.WithProperty("layers", state.Value());
						//record.States.Remove(state);
						break;
					case "growth":
						r = r.WithProperty("age", state.Value());
						//record.States.Remove(state);
						break;
					case "button_pressed_bit":
						r = r.WithProperty("powered", state.Value() == "1" ? "true" : "false");
						//record.States.Remove(state);
						break;
					case "facing_direction":
						switch (int.Parse(state.Value()))
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
						//record.States.Remove(state);
						break;
					case "head_piece_bit":
						r = r.WithProperty("part", state.Value() == "1" ? "head" : "foot");
						//record.States.Remove(state);
						break;
					case "pillar_axis":
						r = r.WithProperty("axis", state.Value());
						//record.States.Remove(state);
						break;
					case "top_slot_bit":
						r = r.WithProperty("type", state.Value() == "1" ? "top" : "bottom");
						//record.States.Remove(state);
						break;
					case "moisturized_amount":
						r = r.WithProperty("moisture", state.Value());
						//record.States.Remove(state);
						break;
					case "age":
						r = r.WithProperty("age", state.Value());
						//record.States.Remove(state);
						break;
			        default:
			//	        Log.Info($"Unknown property for {record.Name}: {state.Name} - {state.Value()}");
					//	r = r.WithProperty(state.Name, state.Value());
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

        const string facing = "facing";
		private BlockState FixFacing(BlockState state, int meta)
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
		
		private BlockState FixFacingTrapdoor(BlockState state, int meta)
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

		internal BlockState TranslateBlockState(BlockState state, long bid, int meta)
		{
			//var dict = state.ToDictionary();

			if (bid >= 8 && bid <= 11) //water or lava
			{
				if (meta != 0)
				{
					string a = "";
					meta = Math.Clamp(meta, 0, 8);
				}
				
				state = state.WithProperty("level", meta.ToString());
			}
			else if (bid == 44 || bid == 182 || bid == 126 /*|| _slabs.Any(x => x.Equals(state.Name, StringComparison.InvariantCultureIgnoreCase))*/) //Slabs
			{
				var isUpper = (meta & 0x08) == 0x08;
				state = state.WithProperty("type", isUpper ? "top" : "bottom");
				
			} 
			else if (bid == 77 || bid == 143) //Buttons
			{
				var modifiedMeta = meta & ~0x07;
				
				if (modifiedMeta >= 1 && modifiedMeta <= 4)
				{
					state = state.WithProperty("face", "wall");
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
				}
				else if (modifiedMeta == 7 || modifiedMeta == 0)
				{
					state = state.WithProperty(facing, "north").WithProperty("face", "floor");
				}
				else if (modifiedMeta == 6 || modifiedMeta == 5)
				{
					state = state.WithProperty(facing, "north").WithProperty("face", "ceiling");
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
				
				switch (meta & ~0x04)
				{
					case 0:
						state = state.WithProperty(facing, "east");
						break;
					case 1:
						state = state.WithProperty(facing, "west");
						break;
					case 2:
						state = state.WithProperty(facing, "south");
						break;
					case 3:
						state = state.WithProperty(facing, "north");
						break;
				}
				
				state = state.WithProperty("half", (meta & 0x04) == 0x04 ? "top" : "bottom");
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
					state = state.WithProperty("hinge", (meta & 0x01) == 0x01 ? "right" : "left");
					state = state.WithProperty("half", "upper");
				}
				else
				{
					bool isOpen = (meta & 0x04) == 0x04;
					state = state.WithProperty("half", "lower");
					state = state.WithProperty("open", isOpen ? "true" : "false");
					state = FixFacing(state, (meta & ~0x03));
				}
			}
			else if (bid == 50) //Torch
			{
				if (meta >= 1 && meta <= 4)
				{
					state = BlockFactory.GetBlockState("minecraft:wall_torch");
					
					switch (meta)
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
				}
			}
			return state;
		}

		private BlockState FixVinesRotation(BlockState state, int meta)
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

		public void Dispose()
		{
			if (Instance != this)
			{
				//BackgroundWorker?.Dispose();
				//BackgroundWorker = null;
			}
		}

		private class ChunkData
		{
			public readonly bool                CacheEnabled;
			public readonly ulong[]             Blobs;
			public readonly uint                SubChunkCount;
			public readonly byte[]              Data;
			public readonly int                 X;
			public readonly int                 Z;
			public readonly Action<ChunkColumn> Callback;

			public ChunkData(bool cacheEnabled,
				ulong[] blobs,
				uint subChunkCount,
				byte[] chunkData,
				int cx,
				int cz,
				Action<ChunkColumn> callback)
			{
				CacheEnabled = cacheEnabled;
				Blobs = blobs;
				SubChunkCount = subChunkCount;
				Data = chunkData;
				X = cx;
				Z = cz;
				Callback = callback;
			}
		}
    }
}