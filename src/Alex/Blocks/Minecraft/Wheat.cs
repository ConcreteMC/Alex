using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common.Blocks.Properties;

namespace Alex.Blocks.Minecraft
{
	public class Wheat : AgingPlantBlock
	{
		public Wheat() : base()
		{
			Solid = false;
			Transparent = true;

			IsFullCube = false;
		}
	}

	public class AgingPlantBlock : Block
	{
		public override bool TryGetStateProperty(string prop, out IStateProperty stateProperty)
		{
			switch (prop)
			{
				case "age":
					stateProperty = new PropertyInt("age");

					return true;
			}

			return base.TryGetStateProperty(prop, out stateProperty);
		}
	}
}