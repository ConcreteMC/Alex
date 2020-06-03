using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.API.Graphics
{
	public sealed class ChunkMesh : IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public BlockShaderVertex[] Vertices { get; set; }
        public Dictionary<RenderStage, int[]> Indexes { get; set; } = new Dictionary<RenderStage, int[]>();
		
		public ChunkMesh(BlockShaderVertex[] entries)
		{
			Vertices = entries;
		}

		public bool Disposed { get; private set; } = false;
		
		public void Dispose()
		{
			Disposed = true;
			
			Vertices = null;
			//SolidIndexes = null;
			//TransparentIndexes = null;
		}
	}
}
