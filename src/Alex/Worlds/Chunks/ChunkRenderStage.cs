using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.Common;
using Alex.Common.Blocks;
using Alex.Common.Graphics;
using Alex.Common.Graphics.GpuResources;
using Alex.Common.Utils.Vectors;
using Alex.Graphics.Models.Blocks;
using Alex.Utils.Threading;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using NLog;

namespace Alex.Worlds.Chunks
{
	/*public class ChunkRenderStage : IDisposable
	{
		private readonly RenderStage _stage;
		private Dictionary<BlockCoordinates, BlockRecord> BlockIndices     { get; set; }

		public bool                  HasChanges     { get; set; }
		public bool IsEmpty => BlockIndices == null || BlockIndices.Count == 0;
		public int Size { get; set; } = 0;
		public int Position { get; set; } = 0;
		public ChunkRenderStage(RenderStage stage)
		{
			_stage = stage;
			BlockIndices = new Dictionary<BlockCoordinates, BlockRecord>();
		}

		public void Remove(BlockCoordinates coordinates)
		{
			var bi = BlockIndices;

			if (bi == null) return;

			if (bi.TryGetValue(coordinates, out var indices))
			{
				indices.Data.Clear();
				HasChanges = true;
			}
		}

		public bool Contains(BlockCoordinates coordinates)
		{
			var bi = BlockIndices;

			if (bi == null) return false;

			return bi.ContainsKey(coordinates);
		}

		public static int BufferUploads = 0;
		public static int BufferCreations = 0;

		private bool _disposed = false;

		public void Dispose()
		{
			if (_disposed)
				return;

			try
			{
				var keys = BlockIndices.Keys.ToArray();

				foreach (var key in keys)
				{
					Remove(key);
				}

				BlockIndices.Clear();
				BlockIndices = null;
			}
			finally
			{
				_disposed = true;
			}
		}
	}*/
}