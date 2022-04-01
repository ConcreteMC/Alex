namespace Alex.Blocks.Minecraft
{
	public class Furnace : Block
	{
		public Furnace() : base()
		{
			Solid = true;
			Transparent = false;
			CanInteract = true;
		}

		/// <inheritdoc />
		public override byte Luminance
		{
			get
			{
				if (Lit.GetValue(BlockState))
				{
					return 13;
				}

				return 0;
			}
		}
	}
}