using Alex.Blocks.Properties;

namespace Alex.Blocks.Minecraft
{
	public class Furnace : Block
	{
		public Furnace() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			Animated = true;
			
			Hardness = 3.5f;
		}
		
		/// <inheritdoc />
		public override byte LightValue {
			get
			{
				if (BlockState.GetTypedValue(Lit))
				{
					return 13;
				}

				return 0;
			} 
		}
	}
}
