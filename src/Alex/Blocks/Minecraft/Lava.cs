namespace Alex.Blocks.Minecraft
{
	public class Lava : Block
	{
		public Lava() : base(50)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = true;
			HasHitbox = false;

			LightValue = 15;
			LightOpacity = 255;
			Animated = true;
			//BlockModel = BlockFactory.StationairyLavaModel;

			//	BlockMaterial = Material.Lava;
		}
	}
}
