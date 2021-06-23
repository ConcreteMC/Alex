namespace Alex.Blocks.Minecraft
{
	public class Cactus : AgingPlantBlock
	{
		public Cactus() : base()
		{
			Solid = true;
			Transparent = true;

			//Hardness = 0.4f;
			BlockMaterial = Material.Cactus;
		}
	}
}
