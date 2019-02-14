namespace Alex.Blocks.Minecraft
{
	public class BirchFenceGate : FenceGate
	{
		public BirchFenceGate() : base(7306)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}

	public class BirchFence : Fence
	{
		public BirchFence() : base(7490)
		{
		}
	}
}
