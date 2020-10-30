namespace Alex.Blocks.Minecraft
{
	public class Hopper : Block
	{
		public Hopper() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			BlockMaterial = Material.Iron;
			Hardness = 3f;
		}
	}
}
