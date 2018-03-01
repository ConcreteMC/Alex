namespace Alex.Blocks
{
	public class Sand : Block
	{
		public Sand(byte meta = 0) : base(12, meta)
		{
			if (meta == 1)
			{
			}
			else
			{
			}

			Solid = true;
			Transparent = false;
		}
	}
}
