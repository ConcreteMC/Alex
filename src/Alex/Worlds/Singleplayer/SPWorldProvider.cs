using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Alex.API;
using Alex.API.Data.Options;
using Alex.API.Events;
using Alex.API.Events.World;
using Alex.API.Network;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Entities;
using Alex.Items;
using Alex.Net;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using MiNET;
using MiNET.Net;
using NLog;

namespace Alex.Worlds.Singleplayer
{
	internal class DebugNetworkProvider : NetworkProvider
	{
		public override bool IsConnected { get; } = true;
		public override ConnectionInfo GetConnectionInfo()
		{
			return new ConnectionInfo(DateTime.UtcNow, 0,0,0,0,0,0,0,0);
		}

		/// <inheritdoc />
		public override void EntityFell(long entityId, float distance, bool inVoid)
		{
			
		}

		public override void EntityAction(int entityId, EntityAction action)
		{
			
		}

		/// <inheritdoc />
		public override void PlayerAnimate(PlayerAnimations animation)
		{
			
		}

		public override void BlockPlaced(BlockCoordinates position, API.Blocks.BlockFace face, int hand, Vector3 cursorPosition, Entity p)
		{
			
		}

		public override void PlayerDigging(DiggingStatus status, BlockCoordinates position, API.Blocks.BlockFace face, Vector3 cursorPosition)
		{
			
		}

		public override void EntityInteraction(Entity player, Entity target, ItemUseOnEntityAction action, int hand)
		{
			
		}

		public override void WorldInteraction(BlockCoordinates position, API.Blocks.BlockFace face, int hand, Vector3 cursorPosition)
		{
			
		}

		public override void UseItem(Item item, int hand, ItemUseAction useAction)
		{
			
		}

		public override void HeldItemChanged(Item item, short slot)
		{
			
		}

		public override void Close()
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
		public NetworkProvider Network { get; } = new DebugNetworkProvider();

        private CancellationTokenSource ThreadCancellationTokenSource;

        private IOptionsProvider OptionsProvider { get; }
        private AlexOptions Options => OptionsProvider.AlexOptions;
        private IEventDispatcher EventDispatcher { get; }
		public SPWorldProvider(Alex alex, IWorldGenerator worldGenerator)
		{
			Alex = alex;
			OptionsProvider = alex.Services.GetRequiredService<IOptionsProvider>();
			EventDispatcher = alex.Services.GetRequiredService<IEventDispatcher>();
			
			_generator = worldGenerator;
		
			ThreadCancellationTokenSource = new CancellationTokenSource();
		}

		private void RunThread()
		{
			SpinWait sw = new SpinWait();
			while (!ThreadCancellationTokenSource.IsCancellationRequested)
			{
				if (!World.Player.IsSpawned)
				{
					sw.SpinOnce();
					continue;
				}

				/*var e = base.WorldReceiver?.Player;
				if (e != null)
				{
					pp = e.KnownPosition;
				}*/
				//var pp = base.WorldReceiver.Player;
				ChunkCoordinates currentCoordinates =
					new ChunkCoordinates(World.Player.KnownPosition);

				if (PreviousChunkCoordinates != currentCoordinates)
				{
					PreviousChunkCoordinates = currentCoordinates;

					GenerateChunks(currentCoordinates, OptionsProvider.AlexOptions.VideoOptions.RenderDistance);

					World.ChunkManager.FlagPrioritization();
				}

				//sw.SpinOnce();
				Thread.Sleep(500);
			}
		}

		private IEnumerable<ChunkColumn> GenerateChunks(ChunkCoordinates center, int renderDistance)
		{
			var oldChunks = _loadedChunks.ToArray();
			
			List<ChunkCoordinates> newChunkCoordinates = new List<ChunkCoordinates>();

			int minZ = Math.Min(center.Z - renderDistance, center.Z + renderDistance);
			int maxZ = Math.Max(center.Z - renderDistance, center.Z + renderDistance);
			
			int minX = Math.Min(center.X - renderDistance, center.X + renderDistance);
			int maxX = Math.Max(center.X - renderDistance, center.X + renderDistance);
			
			for (int x = minX; x <= maxX; x++)
			for (int z = minZ; z <= maxZ; z++)
			{
				var cc = new ChunkCoordinates(x, z);
				newChunkCoordinates.Add(cc);

				if (!_loadedChunks.Contains(cc))
				{
					_loadedChunks.Add(cc);
					
					ChunkColumn chunk =
						_generator.GenerateChunkColumn(cc);

					if (chunk == null) continue;

					base.World.ChunkManager.AddChunk(chunk, cc, false);
					LoadEntities(chunk);
					
					yield return chunk;
				}
			}

			foreach (var chunk in oldChunks)
			{
				if (!newChunkCoordinates.Contains(chunk) && chunk != center)
				{
					//World.UnloadChunk(chunk);
					World.ChunkManager.RemoveChunk(chunk, false);
					_loadedChunks.Remove(chunk);
				}
			}
		}

		private long _spEntityIdCounter = 0;
		private void LoadEntities(ChunkColumn chunk)
		{
			/*var column = (ChunkColumn)chunk;
			if (column.Entities != null)
			{
				foreach (var nbt in column.Entities)
				{
					var eId = Interlocked.Increment(ref _spEntityIdCounter);
					if (EntityFactory.TryLoadEntity(nbt, eId, out Entity entity))
					{
						World.SpawnEntity(eId, entity);
					}
				}
			}*/
		}

		private Thread UpdateThread { get; set; }
		protected override void Initiate()
		{
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

			World.Player.CanFly = true;
			World.Player.IsFlying = true;
				//world.Player.Controller.IsFreeCam = true;

			World.UpdatePlayerPosition(new PlayerLocation(GetSpawnPoint()));
		}

		public override Vector3 GetSpawnPoint()
		{
			return _generator.GetSpawnPoint();
		}
		
		public override Task Load(ProgressReport progressReport)
		{
			ChunkManager.DoMultiPartCalculations = false;
			
			return Task.Run(() =>
			{
				int t = Options.VideoOptions.RenderDistance;
					double radiusSquared = Math.Pow(t, 2);

					var target = radiusSquared * 3;
					int count = 0;

					var pp = GetSpawnPoint();
					var center = new ChunkCoordinates(new PlayerLocation(pp.X, 0, pp.Z));

					Stopwatch sw = Stopwatch.StartNew();
					foreach (var chunk in GenerateChunks(center, t))
					{
						count++;
						
						//base.World.ChunkManager.AddChunk(chunk, new ChunkCoordinates(chunk.X, chunk.Z), false);

						progressReport(LoadingState.LoadingChunks, (int)Math.Floor((count / target) * 100));
					}
					
					var loaded = sw.Elapsed;

					sw.Stop();
					Log.Info($"Chunk pre-loading took {sw.Elapsed.TotalMilliseconds}ms (Loading: {loaded}ms Initializing: {(sw.Elapsed - loaded).TotalMilliseconds}ms)");
				
					PreviousChunkCoordinates = new ChunkCoordinates(new PlayerLocation(pp.X, pp.Y, pp.Z));

					World.Player.IsSpawned = true;
					
					UpdateThread = new Thread(RunThread)
					{
						IsBackground = true
					};

					UpdateThread.Start();
			});
		}

		public override void Dispose()
		{
			ChunkManager.DoMultiPartCalculations = true;
			
			ThreadCancellationTokenSource?.Cancel();
			base.Dispose();
		}
	}
}
