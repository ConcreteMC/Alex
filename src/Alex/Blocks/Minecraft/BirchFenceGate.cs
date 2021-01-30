namespace Alex.Blocks.Minecraft
{
	public class BirchFenceGate : FenceGate
	{
		public BirchFenceGate() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}

	public class BirchFence : Fence
	{
		public BirchFence() : base()
		{
		}
	}
}
