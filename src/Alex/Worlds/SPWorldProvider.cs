using System;
using System.Collections.Generic;
using System.Threading;
using Alex.API.World;
using Alex.Entities;
using Alex.Utils;
using Alex.Worlds.Generators;
using Microsoft.Xna.Framework;
using MiNET.Blocks;
using MiNET.Utils;

namespace Alex.Worlds
{
	public class SPWorldProvider : WorldProvider
	{
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

					var oldChunks = LoadedChunks.ToArray();

					int count = 0;
					int t = Alex.GameSettings.RenderDistance;
					double radiusSquared = Math.Pow(t, 2);

					List<ChunkCoordinates> newChunkCoordinates = new List<ChunkCoordinates>();

					//Load chunks from the inside out (Playerlocation first, then the chunks around it)
					for (int cx = 0; cx < t * 2; cx++)
					{
						int x = cx / 2;
						if (cx % 2 == 0)
						{
							x = -x;
						}

						for (int cz = 0; cz < t * 2; cz++)
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

							var cc = currentCoordinates + new ChunkCoordinates(x, z);
							newChunkCoordinates.Add(cc);

							if (!LoadedChunks.Contains(cc))
							{
								IChunkColumn chunk =
									Generator.GenerateChunkColumn(cc);

								if (chunk == null) continue;

								base.LoadChunk(chunk, cc.X, cc.Z);
								LoadEntities((ChunkColumn) chunk);

								LoadedChunks.Add(cc);
							}
						}
					}

					foreach (var chunk in oldChunks)
					{
						if (!newChunkCoordinates.Contains((ChunkCoordinates) chunk))
						{
							UnloadChunk(chunk.X, chunk.Z);
							LoadedChunks.Remove(chunk);
						}
					}
				}

				Thread.Sleep(500);
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

		public override void Dispose()
		{
			ThreadCancellationTokenSource?.Cancel();
			base.Dispose();
		}
	}
}
