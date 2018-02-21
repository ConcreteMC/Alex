namespace Alex.Data.Blocks
{
	public class Block
	{
		public uint BlockStateID { get; }

		public int BlockId { get; }
		public byte Metadata { get; }
		public bool Solid { get; set; }
		public bool Transparent { get; set; }
		public bool Renderable { get; set; }
		public bool HasHitbox { get; set; }
		public float Drag { get; set; }

		protected Block(byte blockId, byte metadata) : this(GetBlockStateID(blockId, metadata))
		{

		}

		public Block(uint blockStateId)
		{
			BlockStateID = blockStateId;

			Solid = true;
			Transparent = false;
			Renderable = true;
			HasHitbox = true;

			BlockId = (int)(blockStateId >> 4);
			Metadata = (byte)(blockStateId & 0x0F);
		}

		public string DisplayName { get; set; } = null;
		public override string ToString()
		{
			return DisplayName ?? GetType().Name;
		}

		public static uint GetBlockStateID(int id, byte meta)
		{
			return (uint)(id << 4 | meta);
		}
	}
}
