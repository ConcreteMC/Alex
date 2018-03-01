namespace Alex.Blocks
{
	public class WoodenDoor : Block
	{
		public bool IsUpper => (Metadata & 0x08) == 0x08;
		public bool IsOpen => (Metadata & 0x04) == 0x04;
		public WoodenDoor(byte meta) : this(64, meta)
		{

		}

		public WoodenDoor(byte blockId, byte meta) : base(blockId, meta)
		{
			Transparent = true;
		}
	}
}
