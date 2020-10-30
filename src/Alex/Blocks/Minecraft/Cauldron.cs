namespace Alex.Blocks.Minecraft
{
	public class Cauldron : Block
	{
		public Cauldron() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			BlockMaterial = Material.Iron;
		}
	}
}
