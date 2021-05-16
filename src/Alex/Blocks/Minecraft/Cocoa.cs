namespace Alex.Blocks.Minecraft
{
	public class Cocoa : Block
	{
		public Cocoa() : base()
		{
			Solid = true;
			Transparent = true;
			
			BlockMaterial = Material.Plants.SetHardness(0.2f);
		}
	}
}
