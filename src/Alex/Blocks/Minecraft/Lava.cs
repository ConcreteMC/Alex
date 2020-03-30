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
			LightOpacity = 1;
			Animated = true;
			
			BlockMaterial = Material.Lava;
			//BlockModel = BlockFactory.StationairyLavaModel;

			//	BlockMaterial = Material.Lava;
		}
	}
}
