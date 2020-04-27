namespace Alex.Blocks.Minecraft
{
	public class EndStone : Block
	{
		public EndStone() : base(4544)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 3;
			BlockMaterial = Material.Stone;
		}
	}
}
