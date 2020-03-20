namespace Alex.Blocks.Minecraft
{
	public class DarkOakDoor : Door
	{
		public DarkOakDoor() : base(7854)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			BlockMaterial = Material.Wood;
		}
	}
}
