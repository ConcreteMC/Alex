using System;
using System.Collections.Generic;
using System.Threading;
using Alex.API.World;
using Alex.Rendering;
using Alex.Rendering.Camera;
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
		private Camera Cam { get; }
		public SPWorldProvider(Alex alex, Camera camera, ChunkReceived chunkReceivedCallback, ChunkUnload unload, IWorldGenerator worldGenerator) : base(chunkReceivedCallback, unload)
		{
			Alex = alex;
			Generator = worldGenerator;
			Cam = camera;
			ThreadCancellationTokenSource = new CancellationTokenSource();

			var thread = new Thread(RunThread)
			{
				IsBackground = true
			};

			thread.Start();
		}

		private void RunThread()
		{
			while (!ThreadCancellationTokenSource.IsCancellationRequested)
			{
				ChunkCoordinates currentCoordinates =
					new ChunkCoordinates(new PlayerLocation(Cam.Position.X, Cam.Position.Y, Cam.Position.Z));

				if (PreviousChunkCoordinates.DistanceTo(currentCoordinates) >= 1)
				{
					PreviousChunkCoordinates = currentCoordinates;

					var oldChunks = LoadedChunks.ToArray();

					int t = Alex.GameSettings.RenderDistance;

					List<ChunkCoordinates> newChunkCoordinates = new List<ChunkCoordinates>();
					for (int x = -t; x < t; x++)
					{
						for (int z = -t; z < t; z++)
						{
							var cc = currentCoordinates + new ChunkCoordinates(x, z);
							if (!LoadedChunks.Contains(cc))
							{
								var chunk =
									Generator.GenerateChunkColumn(cc);

								if (chunk == null) continue;

								base.LoadChunk(chunk, cc.X, cc.Z);
								//World.ChunkManager.AddChunk(chunk,
								//	new Vector3(cc.X, 0, cc.Z), true);

								LoadedChunks.Add(cc);
								newChunkCoordinates.Add(cc);
							}
						}
					}

					foreach (var chunk in oldChunks)
					{
						if (!newChunkCoordinates.Contains(chunk) && currentCoordinates.DistanceTo(chunk) > t + 1)
						{
							//World.ChunkManager.RemoveChunk(new Vector3(chunk.X, 0, chunk.Z));
							UnloadChunk(chunk.X, chunk.Z);
							LoadedChunks.Remove(chunk);
						}
					}
				}

				Thread.Sleep(500);
			}
		}
	}
}
