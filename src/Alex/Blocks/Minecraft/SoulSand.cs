namespace Alex.Blocks.Minecraft
{
	public class SoulSand : Block
	{
		public SoulSand() : base(3404)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 0.5f;
		}
	}
}
