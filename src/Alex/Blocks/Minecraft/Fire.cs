namespace Alex.Blocks.Minecraft
{
	public class Fire : Block
	{
		public Fire() : base()
		{
			Solid = false;
			Transparent = true;

			LightValue = 15;

			BlockMaterial = Material.Fire;
		}
	}
}
