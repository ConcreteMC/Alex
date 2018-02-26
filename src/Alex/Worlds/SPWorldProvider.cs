using System;
using System.Collections.Generic;
using System.Threading;
using Alex.API.World;
using Alex.Rendering;
using Alex.Rendering.Camera;
using Alex.Worlds.Generators;
using Microsoft.Xna.Framework;
using MiNET.Utils;

namespace Alex.Worlds
{
	public class SPWorldProvider : WorldProvider
	{
		private IWorldGenerator Generator;
		private List<ChunkCoordinates> LoadedChunks { get; } = new List<ChunkCoordinates>();
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
					for (int x = -t; x < t; x++)
					{
						for (int z = -t; z < t; z++)
						{
							var distance = (x * x) + (z * z);
							if (distance > radiusSquared)
							{
								continue;
							}

							var cc = currentCoordinates + new ChunkCoordinates(x, z);
							newChunkCoordinates.Add(cc);

							if (!LoadedChunks.Contains(cc))
							{
								var chunk =
									Generator.GenerateChunkColumn(cc);

								if (chunk == null) continue;

								base.LoadChunk(chunk, cc.X, cc.Z);

								LoadedChunks.Add(cc);	
							}
						}
					}

					foreach (var chunk in oldChunks)
					{
						if (!newChunkCoordinates.Contains(chunk))
						{
							UnloadChunk(chunk.X, chunk.Z);
							LoadedChunks.Remove(chunk);
						}
					}
				}

				Thread.Sleep(500);
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
