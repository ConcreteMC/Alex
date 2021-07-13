using Alex.Blocks.Materials;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common.Blocks.Properties;

namespace Alex.Blocks.Minecraft
{
	public class IronBars : Block
	{
		public IronBars() : base()
		{
			Solid = true;
			Transparent = true;
			
			BlockMaterial = Material.Metal.Clone().WithHardness(5);
		}
		
		public override bool TryGetStateProperty(string prop, out IStateProperty stateProperty)
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
