using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Alex.Blocks;
using Alex.Blocks.Mapping;
using Alex.Common.Utils.Collections;
using Alex.Common.World;
using Alex.Net.Bedrock;
using Alex.Utils;
using Alex.Utils.Caching;
using Alex.Worlds.Chunks;
using fNbt;
using MiNET;
using MiNET.Net;
using MiNET.Utils;
using NLog;
using BlockCoordinates = Alex.Common.Utils.Vectors.BlockCoordinates;
using BlockState = Alex.Blocks.State.BlockState;
using ChunkCoordinates = Alex.Common.Utils.Vectors.ChunkCoordinates;

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
		
		private ObservableCollection<BlockCoordinates> _pendingRequests = new ObservableCollection<BlockCoordinates>();
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
			
			_pendingRequests.CollectionChanged += PendingRequestsOnCollectionChanged;
		}

		private void PendingRequestsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			List<ChunkCoordinates> handled = new List<ChunkCoordinates>();
			if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
			{
				foreach (BlockCoordinates item in e.OldItems)
				{
					//if (!_pendingRequests.Any(x => x.X == item.X && x.Z == item.Z))
					{	
						var chunkManager = Client?.World?.ChunkManager;

						if (chunkManager == null)
							continue;
						
						chunkManager.ScheduleChunkUpdate(new ChunkCoordinates(item.X, item.Z), ScheduleType.Full);
					}
				}
			}
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

		private List<BlockCoordinates> _missing = new List<BlockCoordinates>();

		public void RequestMissing()
		{
			var missing = _missing.ToList();


			McpeSubChunkRequestPacket subChunkRequestPacket = McpeSubChunkRequestPacket.CreateObject();
			subChunkRequestPacket.dimension = (int) Client.World.Dimension;

			var basePosition = new ChunkCoordinates(Client.World.Player.KnownPosition);
			var baseY = Client.World.Player.KnownPosition.Y;

			subChunkRequestPacket.basePosition =
				new MiNET.Utils.Vectors.BlockCoordinates(basePosition.X, 0, basePosition.Z);

			List<SubChunkPositionOffset> offsets = new List<SubChunkPositionOffset>();

			foreach (var bc in missing)
			{
				var cc = new ChunkCoordinates(bc.X, bc.Z);
				var dx = basePosition.X - cc.X;
				var dz = basePosition.Z - cc.Z;
				
				if (dx < sbyte.MinValue || dx > sbyte.MaxValue)
					continue;
				
				if (dz < sbyte.MinValue || dz > sbyte.MaxValue)
					continue;
				
				//	for (uint y = enqueuedChunk.SubChunkCount; y > 0; y--)
				{
					offsets.Add(
						new SubChunkPositionOffset()
						{
							XOffset = (sbyte) dx, YOffset = (sbyte) bc.Y, ZOffset = (sbyte) dz
						});
				}

				_missing.Remove(bc);
			}

			subChunkRequestPacket.offsets = offsets.ToArray();

			Client.SendPacket(subChunkRequestPacket);
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
				if (enqueuedChunk.SubChunkRequestMode == SubChunkRequestMode.SubChunkRequestModeLimited)
				{
					if (enqueuedChunk.SubChunkCount > 0)
					{
						for (uint y = enqueuedChunk.SubChunkCount; y > 0; y--)
						{
							_missing.Add(new BlockCoordinates(handledChunk.X, (int) y, handledChunk.Z));
						}
					}
				}
				else if (enqueuedChunk.SubChunkRequestMode == SubChunkRequestMode.SubChunkRequestModeLegacy)
				{
					chunkManager.AddChunk(handledChunk, new ChunkCoordinates(handledChunk.X, handledChunk.Z), true);
				}
				else
				{
					handledChunk?.Dispose();
				}
			}
		}
		
		private void HandleSubChunkData(SubChunkData data)
		{
			var cc = new ChunkCoordinates(data.X, data.Z);

			try
			{
				var chunkManager = Client?.World?.ChunkManager;

				if (chunkManager == null)
					return;

				if (data.Result != SubChunkRequestResult.Success)
				{
					if (data.Result != SubChunkRequestResult.SuccessAllAir)
						Log.Warn($"Got subchunk response: {data.Result}");

					return;
				}

				if (data.BlobHash != null)
				{
					Log.Info($"Blobhash!");

					//Cache based
					return;
				}

				if (data.Data == null)
				{
					Log.Error($"Invalid subchunk data!");

					return;
				}

				BedrockChunkSection section = null;
				int subChunkIndex = int.MaxValue;

				using (MemoryStream ms = new MemoryStream(data.Data))
				{
					section = BedrockChunkSection.Read(
						this, ms, ref subChunkIndex,
						WorldSettings);
				}

				if (section == null)
				{
					Log.Warn($"Read null section!");

					return;
				}
				
				var sY = data.Y - (WorldSettings.MinY >> 4);
				ChunkColumn chunk;
				if (!chunkManager.TryGetChunk(cc, out chunk))
				{
					chunk = new BedrockChunkColumn(cc.X, cc.Z, WorldSettings);
					chunk[sY] = section;
					
					chunkManager.AddChunk(chunk, cc, true);
					
					//	Log.Info($"Updating chunk! X={data.X} Y={data.Y} Z={data.Z}");
				}
				else
				{
					chunk[sY] = section;

					chunk.CalculateHeight();
					chunkManager.ScheduleChunkUpdate(cc, ScheduleType.Full);
				}
			}
			finally
			{
				/*var matching = _pendingRequests.FirstOrDefault(x => x.X == data.X && x.Z == data.Z && x.Y == data.Y);
				if (matching != null)
					_pendingRequests.Remove(matching);*/
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

			if (chunk.IsComplete)
			{
				chunk.TryBuild(Client, this);
			}
		}

		public WorldSettings WorldSettings { get; set; } = new WorldSettings(384, -64);
		private BedrockChunkColumn HandleChunk(ChunkData chunkData)
		{
			if (chunkData.SubChunkRequestMode != SubChunkRequestMode.SubChunkRequestModeLegacy)
			{
				return new BedrockChunkColumn(chunkData.X, chunkData.Z, WorldSettings);
			}
			
			if (chunkData.SubChunkCount == 0)
				return null;
			
			try
			{
				var chunkColumn = BedrockChunkColumn.ReadFrom(this, chunkData.Data, chunkData.X, chunkData.Z, chunkData.SubChunkCount, WorldSettings);
				
				return chunkColumn;
			}
			catch (Exception ex)
			{
				Log.Error($"Exception in chunk loading: {ex.ToString()}");

				return null;
			}
			finally { }
		}

		public Biome GetBiome(uint id)
		{
			return BiomeUtils.GetBiome(id);
		}

		public uint TranslateBlockState(uint stateId)
		{
			if (_convertedStates.TryGetValue(stateId, out var translated))
			{
				return translated;
			}
			
			var bs = MiNET.Blocks.BlockFactory.BlockPalette.FirstOrDefault(x => x.RuntimeId == stateId);

			if (bs == null) return stateId;

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
					_convertedStates.TryAdd(stateId, convertedState.Id);

					return convertedState.Id;
				}
			}

			var t = BlockFactory.GetBlockState(copy.Name);

			if (t.Name == "Unknown")
			{
				return t.Id;
			}
			else
			{
				_convertedStates.TryAdd(stateId, t.Id);

				return t.Id;
			}
		}

		public BlockState GetBlockState(uint p)
		{
			return BlockFactory.GetBlockState(TranslateBlockState(p));
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