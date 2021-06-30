namespace Alex.Blocks.Minecraft.Doors
{
	public class SpruceDoor : Door
	{
		public SpruceDoor() : base(7598)
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Wood;
		}
	}
}
