using System;
using Alex.API.Blocks;
using Alex.Blocks.State;
using Alex.ResourcePackLib.Json;

namespace Alex.Blocks.Properties
{
	public class PropertyFace : StateProperty<BlockFace>
	{
		public PropertyFace(string name) : base(name)
		{
			
		}

		public override BlockFace ParseValue(string value)
		{
			if (Enum.TryParse(value, true, out BlockFace result))
			{
				return result;
			}

			return BlockFace.None;
		}
	}
}
