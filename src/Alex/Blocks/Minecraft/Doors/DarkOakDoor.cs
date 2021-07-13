using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Doors
{
	public class DarkOakDoor : Door
	{
		public DarkOakDoor() : base(7854)
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Wood;
		}
	}
}
