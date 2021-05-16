namespace Alex.Blocks.Minecraft
{
	public class IronBars : Block
	{
		public IronBars() : base()
		{
			Solid = true;
			Transparent = true;
			
			BlockMaterial = Material.Iron.Clone().SetHardness(5);
		}
	}
}
