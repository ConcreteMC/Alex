using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Data;
using Alex.API.Data.Options;
using Alex.API.Entities;
using Alex.API.Network;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Entities;
using Alex.Worlds.Generators;
using Microsoft.Xna.Framework;
using MiNET;
using MiNET.Net;
using NLog;

namespace Alex.Worlds
{
	internal class DebugNetworkProvider : INetworkProvider
	{
		public bool IsConnected { get; } = true;
		public void EntityAction(int entityId, EntityAction action)
		{
			
		}

		public void SendChatMessage(string message)
		{
			
		}

		public void BlockPlaced(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition)
		{
			
		}

		public void PlayerDigging(DiggingStatus status, BlockCoordinates position, BlockFace face, Vector3 cursorPosition)
		{
			
		}

		public void EntityInteraction(IEntity player, IEntity target, McpeInventoryTransaction.ItemUseOnEntityAction action)
		{
			
		}

		public void WorldInteraction(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition)
		{
			
		}

		public void UseItem(int hand)
		{
			
		}

		public void HeldItemChanged(short slot)
		{
			
		}

		public void Close()
		{
			
		}
	}
	public class SPWorldProvider : WorldProvider
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));

		private readonly IWorldGenerator _generator;
		private readonly List<ChunkCoordinates> _loadedChunks = new List<ChunkCoordinates>();
		private ChunkCoordinates PreviousChunkCoordinates { get; set; } = new ChunkCoordinates(int.MaxValue, int.MaxValue);
		private Alex Alex { get; }
		public INetworkProvider Network { get; } = new DebugNetworkProvider();

        private CancellationTokenSource ThreadCancellationTokenSource;

        private IOptionsProvider OptionsProvider { get; }
        private AlexOptions Options => OptionsProvider.AlexOptions;
		public SPWorldProvider(Alex alex, IWorldGenerator worldGenerator)
		{
			Alex = alex;
			OptionsProvider = alex.Services.GetService<IOptionsProvider>();
			
			_generator = worldGenerator;
		
			ThreadCancellationTokenSource = new CancellationTokenSource();
		}

		private void RunThread()
		{
			while (!ThreadCancellationTokenSource.IsCancellationRequested)
			{
				Vector3 pp = Vector3.Zero;
				var e = base.WorldReceiver?.GetPlayerEntity();
				if (e != null)
				{
					pp = e.KnownPosition;
				}
				//var pp = base.WorldReceiver.GetPlayerEntity();
				ChunkCoordinates currentCoordinates =
					new ChunkCoordinates(new PlayerLocation(pp.X, pp.Y, pp.Z));

				if (PreviousChunkCoordinates.DistanceTo(currentCoordinates) >= 1)
				{
					PreviousChunkCoordinates = currentCoordinates;

					foreach(var chunk in GenerateChunks(currentCoordinates, Options.VideoOptions.RenderDistance))
					{
						var c = (ChunkColumn) chunk;

						base.LoadChunk(chunk, c.X, c.Z, true);
						LoadEntities(c);
					}
				}

				Thread.Sleep(500);
			}
		}

		private IEnumerable<IChunkColumn> GenerateChunks(ChunkCoordinates center, int renderDistance)
		{
			var oldChunks = _loadedChunks.ToArray();

			double radiusSquared = Math.Pow(renderDistance, 2);

			List<ChunkCoordinates> newChunkCoordinates = new List<ChunkCoordinates>();

			List<ChunkCoordinates> results = new List<ChunkCoordinates>((renderDistance * 2) * (renderDistance * 2));

			for (int y = -renderDistance; y <= renderDistance; y++)
			for (int x = -renderDistance; x <= renderDistance; x++)
				results.Add(new ChunkCoordinates(x, y));

			foreach (var cc in results.OrderBy(p =>
				{
					int dx = p.X;
					int dy = p.Z;
					return dx * dx + dy * dy;
				})
				.TakeWhile(p =>
				{
					int dx = p.X;
					int dy = p.Z;
					var r = dx * dx + dy * dy;
					return r < radiusSquared;
				}))
			{
				var acc = center + cc;
				newChunkCoordinates.Add(acc);

				if (!_loadedChunks.Contains(acc))
				{
					IChunkColumn chunk =
						_generator.GenerateChunkColumn(acc);

					if (chunk == null) continue;

					_loadedChunks.Add(acc);

					yield return chunk;
				}
			}

			foreach (var chunk in oldChunks)
			{
				if (!newChunkCoordinates.Contains((ChunkCoordinates)chunk))
				{
					UnloadChunk(chunk.X, chunk.Z);
					_loadedChunks.Remove(chunk);
				}
			}
		}

		private long _spEntityIdCounter = 0;
		private void LoadEntities(ChunkColumn chunk)
		{
			var column = (ChunkColumn)chunk;
			if (column.Entities != null)
			{
				foreach (var nbt in column.Entities)
				{
					var eId = Interlocked.Increment(ref _spEntityIdCounter);
					if (EntityFactory.TryLoadEntity(nbt, eId, out Entity entity))
					{
						base.SpawnEntity(eId, entity);
					}
				}
			}
		}

		private Thread UpdateThread { get; set; }
		protected override void Initiate(out LevelInfo info, out IChatProvider chatProvider)
		{
			info = _generator.GetInfo();
			chatProvider = null;

			/*lock (genLock)
			{
				while (_preGeneratedChunks.TryDequeue(out ChunkColumn chunk))
				{
					if (chunk != null)
					{
						base.LoadChunk(chunk, chunk.X, chunk.Z, false);
						LoadEntities(chunk);
					}
				}

				_preGeneratedChunks = null;
			}*/

			UpdateThread = new Thread(RunThread)
			{
				IsBackground = true
			};

			UpdateThread.Start();

			if (WorldReceiver is World world)
			{
				world.Player.CanFly = true;
				world.Player.IsFlying = true;
				world.Player.Controller.IsFreeCam = true;
			} 

			WorldReceiver?.UpdatePlayerPosition(new PlayerLocation(GetSpawnPoint()));

			Log.Info($"World {info.LevelName} loaded!");
		}

		public override Vector3 GetSpawnPoint()
		{
			return _generator.GetSpawnPoint();
		}

		private Queue<ChunkColumn> _preGeneratedChunks = new Queue<ChunkColumn>();
		private readonly object genLock = new object();
		public override Task Load(ProgressReport progressReport)
		{
			return Task.Run(() =>
			{
				//Dictionary<ChunkCoordinates, IChunkColumn> newChunks = new Dictionary<ChunkCoordinates, IChunkColumn>();
				//using (CachedWorld cached = new CachedWorld(Alex))
				//{
					int t = Options.VideoOptions.RenderDistance;
					double radiusSquared = Math.Pow(t, 2);

					var target = radiusSquared * 3;
					int count = 0;

					var pp = GetSpawnPoint();
					//Log.Info($"Spawnpoint: {pp}");

					Stopwatch sw = Stopwatch.StartNew();
					//List<ChunkColumn> generatedChunks = new List<ChunkColumn>();
					foreach (var chunk in GenerateChunks(new ChunkCoordinates(new PlayerLocation(pp.X, pp.Y, pp.Z)), t))
					{
						var c = (ChunkColumn) chunk;
						count++;
                    //generatedChunks.Add(c);
                    base.LoadChunk(c, c.X, c.Z, true);
						//cached.ChunkManager.AddChunk(chunk, new ChunkCoordinates(c.X, c.Z), false);

						progressReport(LoadingState.LoadingChunks, (int)Math.Floor((count / target) * 100));
					}

					var loaded = sw.Elapsed;

					count = 0;

				
					/*Parallel.ForEach(generatedChunks, (c) =>
					{
						cached.ChunkManager.UpdateChunk(c);

						lock (genLock)
						{
							base.LoadChunk(c, c.X, c.Z, false);
							LoadEntities(c);
						//	_preGeneratedChunks.Enqueue(c);
						}

						cached.ChunkManager.RemoveChunk(new ChunkCoordinates(c.X, c.Z), false);

						//		newChunks.TryAdd(new ChunkCoordinates(c.X, c.Z), c);

						progressReport(LoadingState.GeneratingVertices, (int)Math.Floor((count / target) * 100));

						count++;
					});
					*/
					sw.Stop();
					Log.Info($"Chunk pre-loading took {sw.Elapsed.TotalMilliseconds}ms (Loading: {loaded.TotalMilliseconds}ms Initializing: {(sw.Elapsed - loaded).TotalMilliseconds}ms)");
				
					PreviousChunkCoordinates = new ChunkCoordinates(new PlayerLocation(pp.X, pp.Y, pp.Z));
				//}

				//return newChunks.ToArray();
			});
		}

		public override void Dispose()
		{
			ThreadCancellationTokenSource?.Cancel();
			base.Dispose();
		}
	}
}
