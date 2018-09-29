using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
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

			//BlockModel = BlockFactory.StationairyLavaModel;

		//	BlockMaterial = Material.Lava;
		}
	}
}
