namespace Alex.Blocks.Minecraft
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
