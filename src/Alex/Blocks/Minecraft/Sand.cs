namespace Alex.Blocks.Minecraft
{
	public class Sand : Block
	{
		public Sand() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			BlockMaterial = Material.Sand;
			Hardness = 0.5f;
		}
	}
}
