using MiNET.Utils;

namespace Alex.API.World
{
	public class WorldProvider
	{
		public delegate void ChunkReceived(IChunkColumn chunkColumn, int x, int z);

		private ChunkReceived ChunkCallback;

		public delegate void ChunkUnload(int x, int z);

		private ChunkUnload ChunkUnloadCallback;
		public WorldProvider(ChunkReceived chunkReceivedCallback, ChunkUnload unloadCallback)
		{
			ChunkCallback = chunkReceivedCallback;
			ChunkUnloadCallback = unloadCallback;
		}

		protected void LoadChunk(IChunkColumn chunk, int x, int z)
		{
			ChunkCallback?.Invoke(chunk, x, z);
		}

		protected void UnloadChunk(int x, int z)
		{
			ChunkUnloadCallback?.Invoke(x, z);
		}
	}
}
