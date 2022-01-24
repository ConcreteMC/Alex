namespace Alex.Blocks.Minecraft.Fences
{
	public class BirchFenceGate : FenceGate
	{
		public BirchFenceGate() : base()
		{
			Solid = true;
			Transparent = true;
		}
	}

	public class BirchFence : Fence
	{
		public BirchFence() : base() { }
	}
}