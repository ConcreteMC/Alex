namespace Alex.Blocks.Minecraft
{
	public class TrappedChest : Block
	{
		public TrappedChest() : base(5490)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			CanInteract = true;
		}
	}
}
