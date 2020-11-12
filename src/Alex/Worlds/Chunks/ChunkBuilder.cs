using System.Collections.Generic;
using Alex.API.Graphics;
using Alex.API.Utils;

namespace Alex.Worlds.Chunks
{
	public class ChunkBuilder
	{
		private List<BlockShaderVertex> _vertices;
		public ChunkBuilder()
		{
			_vertices = new List<BlockShaderVertex>();
		}

		public void AddVertex(BlockCoordinates blockCoordinates, BlockShaderVertex vertex)
		{
			
		}
	}
}