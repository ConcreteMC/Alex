using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class SoulFire : Fire
	{
		public SoulFire()
		{
			Luminance = 10;
			
			BlockMaterial = Material.Fire;
		}
	}
}