namespace Alex.Blocks.Minecraft
{
	public class OakDoor : Door
	{
		public OakDoor() : base(3028)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;
			
			BlockMaterial = Material.Wood;
		}
	}
}
