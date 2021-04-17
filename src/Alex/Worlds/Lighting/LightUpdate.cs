using System.Collections.Generic;
using Alex.Worlds.Abstraction;
using MiNET.Utils.Vectors;

namespace Alex.Worlds.Lighting
{
	public abstract class LightUpdate
	{
		private IBlockAccess Level { get; }
		
		private Queue<BlockCoordinates> SpreadQueue { get; }
		private Queue<BlockCoordinates> RemovalQueue { get; }

		
		public LightUpdate(IBlockAccess level)
		{
			Level = level;
			SpreadQueue = new Queue<BlockCoordinates>();
			RemovalQueue = new Queue<BlockCoordinates>();
		}
		
		protected abstract int GetLight(int x, int y, int z);

		protected abstract void SetLight(int x, int y, int z, int level);

		private void PrepareNodes()
		{
			
		}

		public void Execute()
		{
			PrepareNodes();
			
			//while(!)
		}

		private class BlockEntry
		{
			
		}
	}
}