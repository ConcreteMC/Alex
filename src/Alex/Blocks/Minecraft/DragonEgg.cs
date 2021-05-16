namespace Alex.Blocks.Minecraft
{
	public class DragonEgg : Block
	{
		public DragonEgg() : base()
		{
			Solid = true;
			Transparent = true;
			LightValue = 1;

			BlockMaterial = Material.DragonEgg;
		}
	}
}
