namespace Alex.Blocks.Minecraft
{
	public class Cobblestone : Block
	{
		public Cobblestone() : base(14)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
