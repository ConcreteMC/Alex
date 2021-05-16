namespace Alex.Blocks.Minecraft
{
	public class RedstoneOre : Block
	{
		public RedstoneOre() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Ore;
		}
		
		public override byte LightValue {
			get
			{
				if (BlockState.GetTypedValue(Lit))
				{
					return 9;
				}

				return 0;
			} 
		}
	}
}
