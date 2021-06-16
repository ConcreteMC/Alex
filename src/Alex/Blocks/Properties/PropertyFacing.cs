using System;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.ResourcePackLib.Json;

namespace Alex.Blocks.Properties
{
	public class PropertyFace : StateProperty<BlockFace>
	{
		public PropertyFace(string name) : base(name)
		{
			
		}

		/// <inheritdoc />
		protected override StateProperty<BlockFace> WithValue(BlockFace value)
		{
			return new PropertyFace(Name) {Value = value};
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
