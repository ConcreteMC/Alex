using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Entities;
using Alex.Utils;
using Alex.Worlds.Generators;
using log4net;
using Microsoft.Xna.Framework;
using MiNET.Blocks;
using MiNET.Utils;

namespace Alex.Worlds
{
	public class SPWorldProvider : WorldProvider
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(SPWorldProvider));
		private IWorldGenerator Generator;
		private List<ChunkCoordinates> LoadedChunks = new List<ChunkCoordinates>();
		private ChunkCoordinates PreviousChunkCoordinates { get; set; } = new ChunkCoordinates(int.MaxValue, int.MaxValue);
		private Alex Alex { get; }

		private CancellationTokenSource ThreadCancellationTokenSource;

		public SPWorldProvider(Alex alex, IWorldGenerator worldGenerator)
		{
			Alex = alex;
			Generator = worldGenerator;
		
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
			var oldChunks = LoadedChunks.ToArray();

			int count = 0;
			double radiusSquared = Math.Pow(renderDistance, 2);

			List<ChunkCoordinates> newChunkCoordinates = new List<ChunkCoordinates>();

			//Load chunks from the inside out (Playerlocation first, then the chunks around it)
			for (int cx = 0; cx < renderDistance * 2; cx++)
			{
				int x = cx / 2;
				if (cx % 2 == 0)
				{
					x = -x;
				}

				for (int cz = 0; cz < renderDistance * 2; cz++)
				{
					int z = cz / 2;
					if (cz % 2 == 0)
					{
						z = -z;
					}

					var distance = (x * x) + (z * z);
					if (distance > radiusSquared)
					{
						continue;
					}

					var cc = center + new ChunkCoordinates(x, z);
					newChunkCoordinates.Add(cc);

					if (!LoadedChunks.Contains(cc))
					{
						IChunkColumn chunk =
							Generator.GenerateChunkColumn(cc);

						if (chunk == null) continue;

						LoadedChunks.Add(cc);

						yield return chunk;
					}
				}
			}

			foreach (var chunk in oldChunks)
			{
				if (!newChunkCoordinates.Contains((ChunkCoordinates)chunk))
				{
					UnloadChunk(chunk.X, chunk.Z);
					LoadedChunks.Remove(chunk);
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
			while (_preGeneratedChunks.TryDequeue(out ChunkColumn chunk))
			{
				base.LoadChunk(chunk, chunk.X, chunk.Z, false);
				LoadEntities(chunk);
			}

			_preGeneratedChunks = null;

			UpdateThread = new Thread(RunThread)
			{
				IsBackground = true
			};

			UpdateThread.Start();
		}

		public override Vector3 GetSpawnPoint()
		{
			return Generator.GetSpawnPoint();
		}

		private Queue<ChunkColumn> _preGeneratedChunks = new Queue<ChunkColumn>();
		
		public override Task Load(ProgressReport progressReport)
		{
			return Task.Run(() =>
			{
				using (CachedWorld cached = new CachedWorld(Alex))
				{
					int t = Alex.GameSettings.RenderDistance;
					double radiusSquared = Math.Pow(t, 2);

					var target = radiusSquared * 3;
					int count = 0;
					int percentage = 0;

					var pp = GetSpawnPoint();
					List<ChunkColumn> generatedChunks = new List<ChunkColumn>();
					foreach (var chunk in GenerateChunks(new ChunkCoordinates(new PlayerLocation(pp.X, pp.Y, pp.Z)), t))
					{
						var c = (ChunkColumn) chunk;
						generatedChunks.Add(c);

						cached.ChunkManager.AddChunk(chunk, new ChunkCoordinates(c.X, c.Z), false);
						percentage = (int) Math.Floor((count / target) * 100);
						progressReport(LoadingState.LoadingChunks, percentage);

						count++;
					}

					count = 0;

					Parallel.ForEach(generatedChunks, (c) =>
					{
						cached.ChunkManager.UpdateChunk(c);
						_preGeneratedChunks.Enqueue(c);
						percentage = (int)Math.Floor((count / target) * 100);
						progressReport(LoadingState.GeneratingVertices, percentage);

						count++;

						//Log.Info($"Init: {count}/{target}");
					});
				}
			});
		}

		public override void Dispose()
		{
			ThreadCancellationTokenSource?.Cancel();
			base.Dispose();
		}
	}
}
