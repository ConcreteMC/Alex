namespace Alex.Blocks.Minecraft
{
	public class BirchDoor : Door
	{
		public BirchDoor() : base(7662)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			BlockMaterial = Material.Wood;
		}
	}
}
