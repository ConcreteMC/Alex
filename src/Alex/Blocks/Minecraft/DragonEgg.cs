namespace Alex.Blocks.Minecraft
{
	public class DragonEgg : Block
	{
		public DragonEgg() : base(4545)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			LightValue = 1;

			BlockMaterial = Material.DragonEgg;
		}
	}
}
