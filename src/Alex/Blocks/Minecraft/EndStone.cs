namespace Alex.Blocks.Minecraft
{
	public class EndStone : Block
	{
		public EndStone() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 3;
			BlockMaterial = Material.Stone;
		}
	}
}
