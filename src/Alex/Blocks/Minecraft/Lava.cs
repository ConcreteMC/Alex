namespace Alex.Blocks.Minecraft
{
	public class Lava : Block
	{
		public Lava() : base()
		{
			Solid = false;
			Transparent = true;
			HasHitbox = false;

			LightValue = 15;
			LightOpacity = 1;

			BlockMaterial = Material.Lava;
			//BlockModel = BlockFactory.StationairyLavaModel;

			//	BlockMaterial = Material.Lava;
		}
	}
}
