namespace Alex.Blocks.Minecraft
{
	public class Obsidian : Block
	{
		public Obsidian() : base(1039)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 50;
		}
	}
}
