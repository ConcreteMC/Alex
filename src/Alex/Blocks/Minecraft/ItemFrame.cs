using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common.Blocks.Properties;

namespace Alex.Blocks.Minecraft
{
	public class ItemFrame : Block
	{
		public static readonly PropertyBool MAP = new PropertyBool("map");
		public bool HasMap => MAP.GetValue(BlockState);// BlockState.GetTypedValue(MAP);
		
		public ItemFrame()
		{
			Transparent = true;
			BlockMaterial = Material.Wood;
		}

		/// <inheritdoc />
		public override bool TryGetStateProperty(string prop, out IStateProperty stateProperty)
		{
			if (prop == "map")
			{
				stateProperty = MAP.WithValue(HasMap);
				return true;
			}
			return base.TryGetStateProperty(prop, out stateProperty);
		}
	}
}