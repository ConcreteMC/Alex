using System;
using Microsoft.Xna.Framework;

namespace Alex.CoreRT.API.World
{
	public abstract class WorldProvider : IDisposable
	{
		public delegate void ChunkReceived(IChunkColumn chunkColumn, int x, int z);

		private ChunkReceived ChunkCallback;

		public delegate void ChunkUnload(int x, int z);

		private ChunkUnload ChunkUnloadCallback;

		public delegate Vector3 RequestPlayerPosition();

		private RequestPlayerPosition requestPlayerPositionMethod = null;
		protected WorldProvider()
		{
			//ChunkCallback = chunkReceivedCallback;
			//ChunkUnloadCallback = unloadCallback;
		}

		protected void LoadChunk(IChunkColumn chunk, int x, int z)
		{
			ChunkCallback?.Invoke(chunk, x, z);
		}

		protected void UnloadChunk(int x, int z)
		{
			ChunkUnloadCallback?.Invoke(x, z);
		}

		protected Vector3 GetPlayerPosition()
		{
			if (requestPlayerPositionMethod == null) return Vector3.Zero;
			return requestPlayerPositionMethod.Invoke();
		}

		public abstract Vector3 GetSpawnPoint();

		protected abstract void Initiate();

		public void Init(ChunkReceived chunkLoad, ChunkUnload chunkUnload, RequestPlayerPosition playerPositionProvider)
		{
			ChunkCallback = chunkLoad;
			ChunkUnloadCallback = chunkUnload;
			requestPlayerPositionMethod = playerPositionProvider;

			Initiate();
		}

		public virtual void Dispose()
		{

		}
	}
}
