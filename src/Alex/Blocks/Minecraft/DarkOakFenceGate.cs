namespace Alex.Blocks.Minecraft
{
	public class FenceGate : Block
	{
		public FenceGate() : this(0)
		{

		}

		public FenceGate(uint id) : base(id)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}

	public class DarkOakFenceGate : FenceGate
	{
		public DarkOakFenceGate() : base(7402)
		{
			
		}
	}
}
