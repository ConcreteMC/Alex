namespace Alex.Blocks
{
	public class Door : Block
	{
		public bool IsUpper => (Metadata & 0x08) == 0x08;
		public bool IsOpen => (Metadata & 0x04) == 0x04;

		protected Door(byte blockId, byte metadata) : base(blockId, metadata)
		{
			Transparent = true;
			if (IsUpper)
			{

			}
		}
	}
}
