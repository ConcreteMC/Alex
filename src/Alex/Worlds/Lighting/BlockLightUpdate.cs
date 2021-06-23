using System;
using Alex.Common.Utils.Vectors;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;

namespace Alex.Worlds.Lighting
{
	public class BlockLightUpdate : LightUpdate
	{
		/// <inheritdoc />
		public BlockLightUpdate(SubChunkExplorer chunkExplorer) : base(chunkExplorer) { }

		/// <inheritdoc />
		protected override LightArray GetCurrentLightArray()
		{
			return base.SubChunkExplorer.CurrentSubChunk.BlockLight;
		}

		/// <inheritdoc />
		public override void RecalculateNode(int x, int y, int z)
		{
			if(SubChunkExplorer.MoveTo(x, y, z) != SubChunkExplorer.ChunkExplorerStatus.Invalid){
				var block = SubChunkExplorer.CurrentChunk.GetBlockState(x & 0xf, y & 0xff, z & 0xf).Block;
				SetAndUpdateLight(x, y, z, (byte) Math.Max(block.LightValue, GetHighestAdjacentLight(x, y, z) - 1));
			}
		}

		/// <inheritdoc />
		public override int RecalculateChunk(int chunkX, int chunkZ)
		{
			if (this.SubChunkExplorer.MoveToChunk(chunkX, 0, chunkZ) == SubChunkExplorer.ChunkExplorerStatus.Invalid)
			{
				return 0;
			}

			var chunk = SubChunkExplorer.CurrentChunk;
			//chunk.r
			int lightSources = 0;

			//for (var index = 0; index < chunk.Sections.Length; index++)
			//{
			//	var subchunk = chunk.Sections[index];

			//	if (subchunk == null)
				//	break;
				
				//subchunk.ResetLight(true, false);
				//subchunk.RemoveInvalidBlocks();
				//var subchunkY = 
				//subchunk.BlockLight.Reset(0);

				/*bool breakLoop = false;
				for (int y = 0; y < 16; y++)
				{
					for (int x = 0; x < 16; x++)
					{
						for (int z = 0; z < 16; z++)
						{
							if (subchunk.Get(x, y, z).Block.LightValue > 0)
							{
								lightSources += ScanForLightEmittingBlocks(
									subchunk, (chunkX << 4), (index << 4), (chunkZ << 4));
								breakLoop = true;
								break;
							}
						}
						if (breakLoop) break;
					}
					if (breakLoop) break;
				}*/
			//}

			var chunkpos = new BlockCoordinates(chunk.X << 4, 0, chunk.Z << 4);
			foreach (var lightSource in chunk.GetLightSources())
			{
				var ls = chunkpos + lightSource;
				SetAndUpdateLight(ls.X, ls.Y, ls.Z, chunk.GetBlockState(lightSource.X, lightSource.Y, lightSource.Z).Block.LightValue);
				lightSources++;
			}
			
			return lightSources;
		}

		private int ScanForLightEmittingBlocks(ChunkSection section, int baseX, int baseY, int baseZ)
		{
			int lightSources = 0;

			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					for (int y = 0; y < 16; y++)
					{
						var light = section.Get(x, y, z).Block.LightValue;
						if (light > 0)
						{
							SetAndUpdateLight(baseX + x, baseY + y, baseZ + z, light);
							lightSources++;
						}
					}
				}
			}

			return lightSources;
		}
	}
}