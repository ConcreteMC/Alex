namespace Alex.Blocks.Minecraft
{
	public class Sandstone : Block
	{
		public Sandstone() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			BlockMaterial = Material.Stone;
			Hardness = 0.8f;
		}
	}
}
