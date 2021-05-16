namespace Alex.Blocks.Minecraft
{
	public class EndStone : Block
	{
		public EndStone() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Stone;
		}
	}
}
