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

        public VertexPositionNormalTextureColor[] Vertices { get; set; }

        public int[] SolidIndexes { get; set; }
		public int[] TransparentIndexes { get; set; }
		public int[] AnimatedIndexes { get; set; }
		public ChunkMesh(VertexPositionNormalTextureColor[] entries, int[] solidIndexes, int[] transparentIndexes, int[] animatedIndexes)
		{
			Vertices = entries;
			SolidIndexes = solidIndexes;
			TransparentIndexes = transparentIndexes;
			AnimatedIndexes = animatedIndexes;
		}

		public sealed class EntryPosition
		{
			public int Index { get; }
			public int Length { get; }
			public bool Transparent { get; }
			public bool Animated { get; }
			public EntryPosition(bool transparent, bool animated, int index, int length)
			{
				Transparent = transparent;
				Animated = animated;
				
				Index = index;
				Length = length;
			}
		}

		public void Dispose()
		{
			Vertices = null;
			SolidIndexes = null;
			TransparentIndexes = null;
		}
	}
}
