using System.Linq;
using Alex.Common.Utils.Vectors;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;

namespace Alex.Worlds.Lighting
{
	public class SubChunkExplorer
	{
		private IBlockAccess BlockAccess { get; }
		public ChunkColumn CurrentChunk { get; private set; } = null;
		public ChunkSection CurrentSubChunk { get; private set; } = null;
		
		protected int CurrentX { get; set; }
		protected int CurrentY { get; set; }
		protected int CurrentZ { get; set; }
		
		public SubChunkExplorer(IBlockAccess blockAccess)
		{
			BlockAccess = blockAccess;
		}

		public ChunkExplorerStatus MoveTo(int x, int y, int z)
		{
			var newChunkX = x >> 4;
			var newChunkZ = z >> 4;

			if (CurrentChunk == null || CurrentX != newChunkX || CurrentZ != newChunkZ)
			{
				CurrentX = newChunkX;
				CurrentZ = newChunkZ;
				CurrentSubChunk = null;

				CurrentChunk = BlockAccess.GetChunk(new ChunkCoordinates(CurrentX, CurrentZ));

				if (CurrentChunk == null)
					return ChunkExplorerStatus.Invalid;
			}

			var newChunkY = y >> 4;

			if (CurrentSubChunk == null || CurrentY != newChunkY)
			{
				CurrentY = newChunkY;

				if (CurrentY < 0 || CurrentY >= CurrentChunk.Sections.Count(xx => xx != null))
				{
					CurrentSubChunk = null;
					return ChunkExplorerStatus.Invalid;
				}

				CurrentSubChunk = CurrentChunk.GetSection(newChunkY);
				return ChunkExplorerStatus.Moved;
			}

			return ChunkExplorerStatus.Ok;
		}
		
		public ChunkExplorerStatus MoveToChunk(int chunkX, int chunkY, int chunkZ)
		{
			return MoveTo(chunkX << 4, chunkY << 4, chunkZ << 4);
		}
		
		public bool IsValid()
		{
			return CurrentSubChunk != null;
		}

		public void Invalidate()
		{
			CurrentChunk = null;
			CurrentSubChunk = null;
		}

		public enum ChunkExplorerStatus
		{
			Invalid,
			Moved,
			Ok
		}
	}
}