using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Entities;
using Alex.Utils;
using Alex.Worlds.Generators;
using Microsoft.Xna.Framework;
using MiNET.Blocks;
using MiNET.Utils;
using NLog;

namespace Alex.Worlds
{
	public class SPWorldProvider : WorldProvider
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));

		private readonly IWorldGenerator _generator;
		private readonly List<ChunkCoordinates> _loadedChunks = new List<ChunkCoordinates>();
		private ChunkCoordinates PreviousChunkCoordinates { get; set; } = new ChunkCoordinates(int.MaxValue, int.MaxValue);
		private Alex Alex { get; }

		private CancellationTokenSource ThreadCancellationTokenSource;

		public SPWorldProvider(Alex alex, IWorldGenerator worldGenerator)
		{
			Alex = alex;
			_generator = worldGenerator;
		
			ThreadCancellationTokenSource = new CancellationTokenSource();
		}

		private void RunThread()
		{
			while (!ThreadCancellationTokenSource.IsCancellationRequested)
			{
				var pp = base.GetPlayerPosition();
				ChunkCoordinates currentCoordinates =
					new ChunkCoordinates(new PlayerLocation(pp.X, pp.Y, pp.Z));

				if (PreviousChunkCoordinates.DistanceTo(currentCoordinates) >= 1)
				{
					PreviousChunkCoordinates = currentCoordinates;

					foreach(var chunk in GenerateChunks(currentCoordinates, Alex.GameSettings.RenderDistance))
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

			for (int x = -renderDistance; x < renderDistance; x++)
			{
				for (int z = -renderDistance; z < renderDistance; z++)
				{
					var distance = (x * x) + (z * z);
					if (distance > radiusSquared)
					{
						continue;
					}

					var cc = center + new ChunkCoordinates(x, z);
					newChunkCoordinates.Add(cc);

					if (!_loadedChunks.Contains(cc))
					{
						IChunkColumn chunk =
							_generator.GenerateChunkColumn(cc);
						
						if (chunk == null) continue;
						
						_loadedChunks.Add(cc);

						yield return chunk;
					}
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
					if (EntityFactory.TryLoadEntity(nbt, eId, out MiNET.Entities.Entity entity))
					{
						base.SpawnEntity(eId, entity);
					}
				}
			}
		}

		private Thread UpdateThread { get; set; }
		protected override void Initiate()
		{
			lock (genLock)
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
			}

			UpdateThread = new Thread(RunThread)
			{
				IsBackground = true
			};

			UpdateThread.Start();
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
				using (CachedWorld cached = new CachedWorld(Alex))
				{
					int t = Alex.GameSettings.RenderDistance;
					double radiusSquared = Math.Pow(t, 2);

					var target = radiusSquared * 3;
					int count = 0;

					var pp = GetSpawnPoint();
					//Log.Info($"Spawnpoint: {pp}");

					Stopwatch sw = Stopwatch.StartNew();
					List<ChunkColumn> generatedChunks = new List<ChunkColumn>();
					foreach (var chunk in GenerateChunks(new ChunkCoordinates(new PlayerLocation(pp.X, pp.Y, pp.Z)), t))
					{
						var c = (ChunkColumn) chunk;
						generatedChunks.Add(c);

						cached.ChunkManager.AddChunk(chunk, new ChunkCoordinates(c.X, c.Z), false);

						progressReport(LoadingState.LoadingChunks, (int)Math.Floor((count / target) * 100));

						count++;
					}

					var loaded = sw.Elapsed;

					count = 0;

				
					Parallel.ForEach(generatedChunks, (c) =>
					{
						cached.ChunkManager.UpdateChunk(c);

						lock (genLock)
						{
							_preGeneratedChunks.Enqueue(c);
						}

						cached.ChunkManager.RemoveChunk(new ChunkCoordinates(c.X, c.Z), false);

						//		newChunks.TryAdd(new ChunkCoordinates(c.X, c.Z), c);

						progressReport(LoadingState.GeneratingVertices, (int)Math.Floor((count / target) * 100));

						count++;
					});
					sw.Stop();
					Log.Info($"Chunk pre-loading took {sw.Elapsed.TotalMilliseconds}ms (Loading: {loaded.TotalMilliseconds}ms Initializing: {(sw.Elapsed - loaded).TotalMilliseconds}ms)");

					PreviousChunkCoordinates = new ChunkCoordinates(new PlayerLocation(pp.X, pp.Y, pp.Z));
				}

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
