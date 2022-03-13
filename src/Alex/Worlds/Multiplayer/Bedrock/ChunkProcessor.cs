using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;
using Alex.Blocks;
using Alex.Blocks.Mapping;
using Alex.Blocks.Minecraft;
using Alex.Blocks.Minecraft.Fences;
using Alex.Common.Utils.Collections;
using Alex.Common.World;
using Alex.Entities.BlockEntities;
using Alex.Net.Bedrock;
using Alex.Utils;
using Alex.Utils.Caching;
using Alex.Worlds.Chunks;
using Alex.Worlds.Singleplayer;
using fNbt;
using MiNET;
using MiNET.Net;
using MiNET.Utils;
using NLog;
using Air = MiNET.Blocks.Air;
using BlockCoordinates = Alex.Common.Utils.Vectors.BlockCoordinates;
using BlockState = Alex.Blocks.State.BlockState;
using ChunkCoordinates = Alex.Common.Utils.Vectors.ChunkCoordinates;
using NibbleArray = MiNET.Utils.NibbleArray;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class ChunkProcessor : IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChunkProcessor));
		public static ChunkProcessor Instance { get; private set; }

		static ChunkProcessor() { }

		private record BufferItem(KeyValuePair<ulong, byte[]> KeyValuePair, ChunkData ChunkData, SubChunkData SubChunkData);

		//public IReadOnlyDictionary<uint, BlockStateContainer> BlockStateMap { get; set; } =
		//    new Dictionary<uint, BlockStateContainer>();

		public static Itemstates Itemstates { get; set; } = new Itemstates();

		private ConcurrentDictionary<uint, uint> _convertedStates = new ConcurrentDictionary<uint, uint>();

		private CancellationToken CancellationToken { get; }
		//public  bool              ClientSideLighting { get; set; } = true;

		private BedrockClient Client { get; }
		public BlobCache Cache { get; }

		private BufferBlock<BufferItem> _dataQueue;

		public ChunkProcessor(BedrockClient client, CancellationToken cancellationToken, BlobCache blobCache)
		{
			Client = client;
			CancellationToken = cancellationToken;
			Cache = blobCache;

			Instance = this;

			var blockOptions = new ExecutionDataflowBlockOptions
			{
				CancellationToken = cancellationToken,
				EnsureOrdered = false,
				NameFormat = "Chunker: {0}-{1}",
				MaxDegreeOfParallelism = 1
			};

			_dataQueue = new BufferBlock<BufferItem>(blockOptions);

			var handleBufferItemBlock = new ActionBlock<BufferItem>(HandleBufferItem, blockOptions);
			var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
			_dataQueue.LinkTo(handleBufferItemBlock, linkOptions);
		}

		private void HandleBufferItem(BufferItem item)
		{
			if (item.ChunkData != null)
			{
				HandleChunkData(item.ChunkData);

				return;
			}

			if (item.SubChunkData != null)
			{
				HandleSubChunkData(item.SubChunkData);	
				return;
			}

			HandleKv(item.KeyValuePair);
		}

		private void HandleKv(KeyValuePair<ulong, byte[]> kv)
		{
			ulong hash = kv.Key;
			byte[] data = kv.Value;

			if (!Cache.Contains(hash))
				Cache.TryStore(hash, data);

			var chunks = _futureChunks.Where(c => c.SubChunks.Contains(hash) || c.Biome == hash).ToArray();

			foreach (CachedChunk chunk in chunks)
			{
				chunk.TryBuild(Client, this);
			}
		}

		private void HandleChunkData(ChunkData enqueuedChunk)
		{
			var chunkManager = Client?.World?.ChunkManager;

			if (chunkManager == null)
				return;

			if (enqueuedChunk.CacheEnabled)
			{
				HandleChunkCachePacket(enqueuedChunk);

				return;
			}

			var handledChunk = HandleChunk(enqueuedChunk);

			if (handledChunk != null)
			{
				chunkManager.AddChunk(handledChunk, new ChunkCoordinates(enqueuedChunk.X, enqueuedChunk.Z), true);

				return;
			}
		}

		private void HandleSubChunkData(SubChunkData data)
		{
			var chunkManager = Client?.World?.ChunkManager;

			if (chunkManager == null)
				return;
			
			if (data.Result != SubChunkRequestResult.Success)
			{
				Log.Warn($"Got subchunk response: {data.Result}");
				return;
			}

			if (data.BlobHash != null)
			{
				//Cache based
				return;
			}

			if (data.Data == null)
			{
				Log.Error($"Invalid subchunk data!");
				return;
			}

			BedrockChunkSection section = null;
			using (MemoryStream ms = new MemoryStream(data.Data))
			{
				using NbtBinaryReader defStream = new NbtBinaryReader(ms, true);
				section = ReadSection(defStream);
			}

			if (section == null)
			{
				Log.Warn($"Read null section!");
				return;
			}

			if (chunkManager.TryGetChunk(new ChunkCoordinates(data.X, data.Z), out var chunk))
			{
				var sY = data.Y + (chunk.WorldSettings.MinY / 16);
				//chunk.WorldSettings.WorldHeight
				
				var currentSection = chunk.Sections[sY];
				chunk.Sections[sY] = section;
				
				currentSection?.Dispose();
				
				chunkManager.ScheduleChunkUpdate(new ChunkCoordinates(data.X, data.Z), ScheduleType.Full);
			}
		}
		
		public void Clear()
		{
			if (_dataQueue.Count > 0)
			{
				_dataQueue.TryReceiveAll(out _);
			}
		}

		public void HandleChunkData(
			bool cacheEnabled,
			SubChunkRequestMode subChunkRequestMode,
			ulong[] blobs,
			uint subChunkCount,
			byte[] chunkData,
			int cx,
			int cz)
		{
			if (CancellationToken.IsCancellationRequested || subChunkCount < 1)
				return;

			var value = new ChunkData(cacheEnabled, subChunkRequestMode, blobs, subChunkCount, chunkData, cx, cz);

			_dataQueue.Post(new BufferItem(default, value, null));
		}

		public void HandleSubChunkData(SubChunkRequestResult result, ulong? blobHash, byte[] data, int cx, int cy, int cz)
		{
			if (CancellationToken.IsCancellationRequested)
				return;

			var value = new SubChunkData(result, blobHash, data, cx, cy, cz);
			_dataQueue.Post(new BufferItem(default, null, value));
		}

		public BlockState GetBlockState(uint p)
		{
			if (_convertedStates.TryGetValue(p, out var existing))
			{
				return BlockFactory.GetBlockState(existing);
			}

			var bs = MiNET.Blocks.BlockFactory.BlockPalette.FirstOrDefault(x => x.RuntimeId == p);

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
						_convertedStates.TryAdd(p, convertedState.Id);
					}

					return convertedState;
				}

				var t = BlockFactory.GetBlockState(copy.Name); /* TranslateBlockState(
			        BlockFactory.GetBlockState(copy.Name),
			        -1, copy.Data);*/

				if (t.Name == "Unknown")
				{
					return t;
					//  File.WriteAllText(Path.Combine("failed", bs.Name + ".json"), JsonConvert.SerializeObject(bs, Formatting.Indented));
				}
				else
				{
					_convertedStates.TryAdd(p, t.Id);

					return t;
				}
			}

			return null;
		}

		public ThreadSafeList<CachedChunk> _futureChunks = new ThreadSafeList<CachedChunk>();

		public ConcurrentDictionary<BlockCoordinates, NbtCompound> _futureBlockEntities =
			new ConcurrentDictionary<BlockCoordinates, NbtCompound>();

		public void HandleClientCacheMissResponse(McpeClientCacheMissResponse message)
		{
			foreach (KeyValuePair<ulong, byte[]> kv in message.blobs)
			{
				_dataQueue.Post(new BufferItem(kv, null, null));
			}
		}

		private void HandleChunkCachePacket(ChunkData chunkData)
		{
			var chunk = new CachedChunk(chunkData.X, chunkData.Z);
			chunk.SubChunks = new ulong[chunkData.SubChunkCount];
			chunk.Sections = new byte[chunkData.SubChunkCount][];

			var hits = new List<ulong>();
			var misses = new List<ulong>();

			ulong biomeHash = chunkData.Blobs.Last();
			chunk.Biome = biomeHash;

			if (Cache.Contains(biomeHash))
			{
				hits.Add(biomeHash);
			}
			else
			{
				misses.Add(biomeHash);
			}

			for (int i = 0; i < chunkData.SubChunkCount; i++)
			{
				ulong hash = chunkData.Blobs[i];
				chunk.SubChunks[i] = hash;

				if (Cache.TryGet(hash, out byte[] subChunkData))
				{
					chunk.Sections[i] = subChunkData;
					hits.Add(hash);
					//chunk.SubChunkCount--;
				}
				else
				{
					chunk.Sections[i] = null;
					misses.Add(hash);
				}
			}

			if (misses.Count > 0)
				_futureChunks.TryAdd(chunk);

			var status = McpeClientCacheBlobStatus.CreateObject();
			status.hashHits = hits.ToArray();
			status.hashMisses = misses.ToArray();
			Client.SendPacket(status);

			using (MemoryStream ms = new MemoryStream(chunkData.Data))
			{
				ReadExtra(chunk.Chunk, ms);
			}

			if (chunk.IsComplete)
			{
				chunk.TryBuild(Client, this);
			}
		}

		internal BedrockChunkSection ReadSection(NbtBinaryReader defStream)
		{
			int version = defStream.ReadByte();
			
			if (version == 1 || version == 8)
			{
				return ReadPalletedSection(defStream, version);
			}
			else if (version == 0)
			{
				return ReadLegacyChunkSection(defStream);
			}
			else
			{
				Log.Warn($"Unsupported storage version: {version}");
			}

			return null;
		}

		private BedrockChunkSection ReadPalletedSection(NbtBinaryReader defStream, int version)
		{
			var stream = defStream.BaseStream;
			int storageSize = version == 1 ? 1 : defStream.ReadByte();

			var section = new BedrockChunkSection(storageSize);

			for (int storage = 0; storage < storageSize; storage++)
			{
				int flags = stream.ReadByte();
				bool isRuntime = (flags & 1) != 0;
				int bitsPerBlock = flags >> 1;
				int blocksPerWord = (int)Math.Floor(32f / bitsPerBlock);
				int wordsPerChunk = (int)Math.Ceiling(4096f / blocksPerWord);

				long jumpPos = stream.Position;
				stream.Seek(wordsPerChunk * 4, SeekOrigin.Current);

				int paletteCount = VarInt.ReadSInt32(stream);
				var palette = new int[paletteCount];
				bool allZero = true;

				for (int j = 0; j < paletteCount; j++)
				{
					if (!isRuntime)
					{
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

				if (allZero) continue;

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

							uint state = (uint)((word >> ((position % blocksPerWord) * bitsPerBlock))
							                    & ((1 << bitsPerBlock) - 1));

							int x = (position >> 8) & 0xF;
							int y = position & 0xF;
							int z = (position >> 4) & 0xF;

							int runtimeId = palette[state];

							if (runtimeId != 0)
							{
								var blockState = GetBlockState((uint)runtimeId);

								if (blockState != null)
									section.Set(storage, x, y, z, blockState);
							}

							position++;
						}
					}

					stream.Position = afterPos;
				}
			}

			return section;
		}

		private BedrockChunkSection ReadLegacyChunkSection(NbtBinaryReader defStream)
		{
			byte[] blockIds = new byte[4096];

			if (defStream.Read(blockIds, 0, blockIds.Length) != blockIds.Length)
				return null;

			NibbleArray data = new NibbleArray(4096);

			if (defStream.Read(data.Data, 0, data.Data.Length) != data.Data.Length)
				return null;

			var section = new BedrockChunkSection(1);

			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					for (int y = 0; y < 16; y++)
					{
						int idx = (x << 8) + (z << 4) + y;
						var id = blockIds[idx];
						var meta = data[idx];

						//var ruid = BlockFactory.GetBlockStateID(id, meta);

						var block = MiNET.Blocks.BlockFactory.GetRuntimeId(id, meta);
						BlockState result = GetBlockState(block);

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

			return section;
		}

		public WorldSettings WorldSettings { get; set; } = new WorldSettings(384, -64);
		private BedrockChunkColumn HandleChunk(ChunkData chunkData)
		{
			if (chunkData.SubChunkCount == 0)
				return null;
			
			if (chunkData.SubChunkRequestMode != SubChunkRequestMode.SubChunkRequestModeLegacy)
			{
				/*McpeSubChunkRequestPacket subChunkRequestPacket = McpeSubChunkRequestPacket.CreateObject();
				subChunkRequestPacket.dimension = (int) Client.World.Dimension;

				subChunkRequestPacket.basePosition =
					new MiNET.Utils.Vectors.BlockCoordinates(chunk.X, 0, chunk.Z);

				List<SubChunkPositionOffset> offsets = new List<SubChunkPositionOffset>();

				for (int y = 0; y < 24; y++)
				{
					offsets.Add(new SubChunkPositionOffset()
					{
						XOffset = 0,
						YOffset = y - (4),
						ZOffset = 0
					});
				}
				subChunkRequestPacket.offsets = offsets.ToArray();
						
				Client.SendPacket(subChunkRequestPacket);*/

				/*subChunkRequestPacket.offsets = new SubChunkPositionOffset[]
				{
					new SubChunkPositionOffset() {XOffset = 0, ZOffset = 0, YOffset = -2}
				};*/
			}

			try
			{
				using (MemoryStream stream = new MemoryStream(chunkData.Data))
				{
					BedrockChunkColumn chunkColumn = new BedrockChunkColumn(chunkData.X, chunkData.Z, WorldSettings);

					using NbtBinaryReader defStream = new NbtBinaryReader(stream, true);
					//Log.Info($"SubchunkCount: {chunkData.SubChunkCount} | Mode={chunkData.SubChunkRequestMode} | CacheEnabled={chunkData.CacheEnabled}");
					if (chunkData.SubChunkRequestMode != SubChunkRequestMode.SubChunkRequestModeLimitless
					    && !chunkData.CacheEnabled)
					{
						for (int s = 0; s < chunkData.SubChunkCount; s++)
						{
							chunkColumn.Sections[s] = ReadSection(defStream);
						}
					}

					if (!chunkData.CacheEnabled)
					{
						byte[] biomeIds = new byte[256];

						if (defStream.Read(biomeIds, 0, 256) != 256) return chunkColumn;

						for (int x = 0; x < 16; x++)
						{
							for (int z = 0; z < 16; z++)
							{
								var biomeId = biomeIds[(z << 4) + (x)];

								for (int y = 0; y < 255; y++)
								{
									chunkColumn.SetBiome(x, y, z, BiomeUtils.GetBiome(biomeId));
								}
							}
						}
					}

					if (stream.Position >= stream.Length - 1)
					{
						chunkColumn.CalculateHeight();

						return chunkColumn;
					}

					ReadExtra(chunkColumn, stream);

					chunkColumn.CalculateHeight();

					return chunkColumn;
				}
			}
			catch (Exception ex)
			{
				Log.Error($"Exception in chunk loading: {ex.ToString()}");

				return null;
			}
			finally { }
		}

		private void ReadExtra(ChunkColumn chunkColumn, MemoryStream stream)
		{
			int borderBlock =
				VarInt.ReadSInt32(
					stream); // defStream.ReadByte();// defStream.ReadVarInt(); //VarInt.ReadSInt32(stream);

			if (borderBlock != 0)
			{
				int len = (int)(stream.Length - stream.Position);
				var bytes = new byte[len];
				stream.Read(bytes, 0, len);
			}


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
				}
				catch (EndOfStreamException) { }
				catch(NbtFormatException){}

				// if (Log.IsTraceEnabled()) Log.Trace($"Blockentity:\n{file.RootTag}");
			}

			if (stream.Position < stream.Length - 1)
			{
				int len = (int)(stream.Length - stream.Position);
				var bytes = new byte[len];
				stream.Read(bytes, 0, len);
				Log.Warn($"Still have data to read\n{Packet.HexDump(new ReadOnlyMemory<byte>(bytes))}");
			}
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

		private bool TryConvertBlockState(BlockStateContainer record, out BlockState result)
		{
			if (_convertedStates.TryGetValue((uint)record.RuntimeId, out var alreadyConverted))
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
							defaultState = defaultState.WithProperty(prop.Name, prop.Value());
						}
					}

					if (defaultState is PeBlockState pebs)
					{
						result = pebs.Original;

						return true;
					}

					result = defaultState;

					return true;
				}
			}

			result = null;

			return false;
		}

		public void Dispose()
		{
			_dataQueue.Complete();
			//   _actionQueue?.Clear();
			//	_actionQueue = null;

			//_blobQueue?.Clear();
			//_blobQueue = null;

			_convertedStates?.Clear();
			_convertedStates = null;

			if (Instance != this)
			{
				//BackgroundWorker?.Dispose();
				//BackgroundWorker = null;
			}
		}

		private class ChunkData
		{
			public readonly bool CacheEnabled;
			public readonly SubChunkRequestMode SubChunkRequestMode;
			public readonly ulong[] Blobs;
			public readonly uint SubChunkCount;
			public readonly byte[] Data;
			public readonly int X;
			public readonly int Z;

			public ChunkData(bool cacheEnabled,SubChunkRequestMode subChunkRequestMode, ulong[] blobs, uint subChunkCount, byte[] chunkData, int cx, int cz)
			{
				CacheEnabled = cacheEnabled;
				SubChunkRequestMode = subChunkRequestMode;
				Blobs = blobs;
				SubChunkCount = subChunkCount;
				Data = chunkData;
				X = cx;
				Z = cz;
			}
		}

		private class SubChunkData
		{
			public SubChunkRequestResult Result { get; }
			public ulong? BlobHash { get; }
			public byte[] Data { get; }
			public int X { get; }
			public int Y { get; }
			public int Z { get; }

			public SubChunkData(SubChunkRequestResult result, ulong? blobHash, byte[] data, int cx, int cy, int cz)
			{
				Result = result;
				BlobHash = blobHash;
				Data = data;
				X = cx;
				Y = cy;
				Z = cz;
			}
		}
	}
}