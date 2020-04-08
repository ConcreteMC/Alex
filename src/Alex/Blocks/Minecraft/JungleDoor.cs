namespace Alex.Blocks.Minecraft
{
	public class JungleDoor : Door
	{
		public JungleDoor() : base(7726)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			BlockMaterial = Material.Wood;
		}
	}
}
