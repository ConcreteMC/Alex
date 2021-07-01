namespace Alex.Worlds
{
	public class WorldSettings
	{
		public int WorldHeight { get; set; }
		public int MinY { get; set; }

		public WorldSettings(int worldHeight, int minY)
		{
			WorldHeight = worldHeight;
			MinY = minY;
		}
		
		public static readonly WorldSettings Default = new WorldSettings(256, 0);
	}
}