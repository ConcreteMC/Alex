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
        
       // public int[] SolidIndexes { get; set; }
		//public int[] TransparentIndexes { get; set; }
	//	public int[] AnimatedIndexes { get; set; }
		
		public Dictionary<RenderStage, int[]> Indexes { get; set; } = new Dictionary<RenderStage, int[]>();
		
		public ChunkMesh(BlockShaderVertex[] entries)
		{
			Vertices = entries;
			//Indexes = indexes;
			//	SolidIndexes = solidIndexes;
			//	TransparentIndexes = transparentIndexes;
			//	AnimatedIndexes = animatedIndexes;
		}

		public sealed class EntryPosition
		{
			public int Index { get; }
			public int Length { get; }
			public RenderStage Stage { get; }
			//public bool Transparent { get; }
			//public bool Animated { get; }
			public int Storage { get; }
			public EntryPosition(RenderStage stage, int index, int length, int storage)
			{
				//Transparent = transparent;
				//Animated = animated;

				Stage = stage;
				
				Index = index;
				Length = length;
				Storage = storage;
			}
		}

		public void Dispose()
		{
			Vertices = null;
			//SolidIndexes = null;
			//TransparentIndexes = null;
		}
	}
}
