namespace Alex.Blocks.Minecraft
{
	public class SlimeBlock : Block
	{
		public SlimeBlock() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = true;

			BlockMaterial = Material.Slime;
		}
	}
}
