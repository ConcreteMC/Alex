using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alex.API;
using Alex.API.Data;
using Alex.API.Data.Options;
using Alex.API.Entities;
using Alex.API.Events;
using Alex.API.Events.World;
using Alex.API.Items;
using Alex.API.Network;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Entities;
using Alex.Items;
using Alex.Net;
using Alex.Worlds.Generators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using MiNET;
using MiNET.Net;
using NLog;

namespace Alex.Worlds
{
	internal class DebugNetworkProvider : NetworkProvider
	{
		public override bool IsConnected { get; } = true;
		public override ConnectionInfo GetConnectionInfo()
		{
			return new ConnectionInfo(DateTime.UtcNow, 0,0,0,0,0,0,0,0);
		}

		public override void EntityAction(int entityId, EntityAction action)
		{
			
		}

		/// <inheritdoc />
		public override void PlayerAnimate(PlayerAnimations animation)
		{
			
		}

		public override void BlockPlaced(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition, Entity p)
		{
			
		}

		public override void PlayerDigging(DiggingStatus status, BlockCoordinates position, BlockFace face, Vector3 cursorPosition)
		{
			
		}

		public override void EntityInteraction(Entity player, Entity target, McpeInventoryTransaction.ItemUseOnEntityAction action)
		{
			
		}

		public override void WorldInteraction(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition)
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

				if (PreviousChunkCoordinates.DistanceTo(currentCoordinates) >= 1)
				{
					PreviousChunkCoordinates = currentCoordinates;

					foreach(var chunk in GenerateChunks(currentCoordinates, OptionsProvider.AlexOptions.VideoOptions.RenderDistance))
					{
						var c = (ChunkColumn) chunk;
		
						base.World.ChunkManager.AddChunk(c, currentCoordinates, false);
						//EventDispatcher.DispatchEvent(new ChunkReceivedEvent(currentCoordinates, c));
						LoadEntities(c);
					}
				}

				//sw.SpinOnce();
				Thread.Sleep(500);
			}
		}

		private IEnumerable<ChunkColumn> GenerateChunks(ChunkCoordinates center, int renderDistance)
		{
			var oldChunks = _loadedChunks.ToArray();

			double radiusSquared = Math.Pow(renderDistance, 2);

			List<ChunkCoordinates> newChunkCoordinates = new List<ChunkCoordinates>();
			
			List<ChunkCoordinates> results = new List<ChunkCoordinates>();

			for (int y = center.Z -renderDistance; y <= center.Z + renderDistance; y++)
			for (int x = center.X -renderDistance; x <= center.X + renderDistance; x++)
			{
				var cc = new ChunkCoordinates(x, y);
				
				newChunkCoordinates.Add(cc);

				if (!_loadedChunks.Contains(cc))
				{
					ChunkColumn chunk =
						_generator.GenerateChunkColumn(cc);

					if (chunk == null) continue;

					_loadedChunks.Add(cc);

					yield return chunk;
				}
			}

			foreach (var chunk in oldChunks)
			{
				if (!newChunkCoordinates.Contains(chunk) && chunk != center)
				{
					//UnloadChunk(chunk.X, chunk.Z);
					ChunkUnloadEvent unloadEvent = new ChunkUnloadEvent(chunk);
					EventDispatcher.DispatchEvent(unloadEvent);
					
					if (!unloadEvent.IsCancelled)
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
		private World World { get; set; }
		protected override void Initiate(out LevelInfo info)
		{
			info = _generator.GetInfo();

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

			if (base.World is World world)
			{
				World = world;
				
				world.Player.CanFly = true;
				world.Player.IsFlying = true;
				//world.Player.Controller.IsFreeCam = true;
			} 

			base.World?.UpdatePlayerPosition(new PlayerLocation(GetSpawnPoint()));

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
			ChunkManager.DoMultiPartCalculations = false;
			
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
					List<ChunkColumn> chunks = new List<ChunkColumn>();
					foreach (var chunk in GenerateChunks(new ChunkCoordinates(new PlayerLocation(pp.X, pp.Y, pp.Z)), t))
					{
						var c = (ChunkColumn) chunk;
						count++;
						
						chunks.Add(c);
                    //generatedChunks.Add(c);
                    
						//cached.ChunkManager.AddChunk(chunk, new ChunkCoordinates(c.X, c.Z), false);

						progressReport(LoadingState.LoadingChunks, (int)Math.Floor((count / target) * 100));
					}

					count = 0;
					
					foreach (var chunk in chunks)
					{
						//EventDispatcher.DispatchEvent(new ChunkReceivedEvent(new ChunkCoordinates(chunk.X, chunk.Z), chunk)
						//{
						//	DoUpdates = false
						//});
						base.World.ChunkManager.AddChunk(chunk, new ChunkCoordinates(chunk.X, chunk.Z), false);
						progressReport(LoadingState.GeneratingVertices, (int)Math.Floor((count / target) * 100));
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
			ChunkManager.DoMultiPartCalculations = true;
			
			ThreadCancellationTokenSource?.Cancel();
			base.Dispose();
		}
	}
}
