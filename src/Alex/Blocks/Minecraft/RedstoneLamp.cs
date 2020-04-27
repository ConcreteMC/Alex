namespace Alex.Blocks.Minecraft
{
	public class RedstoneLamp : Block
	{
		public RedstoneLamp() : base(4547)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 0.3f;
		}

		public override int LightValue
		{
			get
			{
				if (BlockState.TryGetValue("lit", out string lit))
				{
					if (lit == "true")
					{
						return 15;
					}
				}

				return 0;
			}
			set
			{
				
			}
		}
	}
}
