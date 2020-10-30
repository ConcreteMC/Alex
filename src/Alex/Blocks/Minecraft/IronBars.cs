namespace Alex.Blocks.Minecraft
{
	public class IronBars : Block
	{
		public IronBars() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			Hardness = 5;
			BlockMaterial = Material.Iron;
		}
	}
}
