using Alex.Blocks.Properties;
using Alex.Blocks.State;

namespace Alex.Blocks.Minecraft
{
	public class IronBars : Block
	{
		public IronBars() : base()
		{
			Solid = true;
			Transparent = true;
			
			BlockMaterial = Material.Iron.Clone().SetHardness(5);
		}
		
		public override bool TryGetStateProperty(string prop, out StateProperty stateProperty)
		{
			switch (prop)
			{
				case "up":
				case "north":
				case "east":
				case "south":
				case "west":
					stateProperty = new PropertyBool(prop, "true", "false");
					return true;
			}
			return base.TryGetStateProperty(prop, out stateProperty);
		}
	}
}
