namespace Alex.Blocks.Minecraft
{
	public class SpruceDoor : Door
	{
		public SpruceDoor() : base(7598)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			BlockMaterial = Material.Wood;
		}
	}
}
