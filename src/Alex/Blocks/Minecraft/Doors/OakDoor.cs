using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Doors
{
	public class OakDoor : Door
	{
		public OakDoor() : base(3028)
		{
			Solid = true;
			Transparent = true;
			IsFullCube = false;

			BlockMaterial = Material.Wood;
		}
	}
}