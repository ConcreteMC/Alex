using Alex.Blocks.Minecraft;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common.Blocks.Properties;

namespace Alex.Blocks
{
	public class OpenableBlockBase : Block
	{
		public static PropertyBool OPEN => new PropertyBool("open", "true", "false");

		public bool IsPowered => RedstoneBase.POWERED.GetValue(BlockState);// BlockState.GetTypedValue(RedstoneBase.POWERED);
		public bool IsOpen => OPEN.GetValue(BlockState);

		protected OpenableBlockBase()
		{
			
		}
		
		/// <inheritdoc />
		public override bool TryGetStateProperty(string prop, out IStateProperty stateProperty)
		{
			switch (prop)
			{
				case "open":
					stateProperty = OPEN;
					return true;
				case "powered":
					stateProperty = RedstoneBase.POWERED;
					return true;
			}
			return base.TryGetStateProperty(prop, out stateProperty);
		}
	}
}