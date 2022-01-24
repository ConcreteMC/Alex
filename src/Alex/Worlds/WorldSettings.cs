using System;

namespace Alex.Worlds
{
	public class WorldSettings
	{
		public int WorldHeight { get; set; }
		public int MinY { get; set; }

		public int TotalHeight { get; }

		public WorldSettings(int worldHeight, int minY)
		{
			WorldHeight = worldHeight;
			MinY = minY;

			TotalHeight = Math.Abs(minY) + worldHeight;
		}

		public static readonly WorldSettings Default = new WorldSettings(256, 0);
	}
}