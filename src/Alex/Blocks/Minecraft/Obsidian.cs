namespace Alex.Blocks.Minecraft
{
	public class Obsidian : Block
	{
		public Obsidian() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 50;
		}
	}
}
