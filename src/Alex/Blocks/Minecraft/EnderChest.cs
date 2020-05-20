namespace Alex.Blocks.Minecraft
{
	public class EnderChest : Block
	{
		public EnderChest() : base(4642)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			LightValue = 7;
			
			Hardness = 22.5f;
			
			CanInteract = true;
		}
	}
}
