namespace Alex.Blocks.Minecraft
{
	public class MobSpawner : Block
	{
		public MobSpawner() : base(1557)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			LightValue = 1;
			
			Hardness = 5;
		}
	}
}
