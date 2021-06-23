using Alex.Blocks.Properties;
using Alex.Blocks.State;

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
		public override bool TryGetStateProperty(string prop, out StateProperty stateProperty)
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
