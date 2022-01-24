using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Doors
{
	public class BirchDoor : Door
	{
		public BirchDoor() : base(7662)
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Wood;
		}
	}
}