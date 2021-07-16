using System;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Blocks.Properties;
using Alex.Common.Utils;
using Alex.ResourcePackLib.Json;

namespace Alex.Blocks.Properties
{
	public class PropertyFace : StateProperty<BlockFace>
	{
		public PropertyFace(string name) : base(name)
		{
			
		}
		
		public override string StringValue => Value.ToString().ToLower();

		/// <inheritdoc />
		public override IStateProperty<BlockFace> WithValue(BlockFace value)
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

		/// <inheritdoc />
		public override string ToFormattedString()
		{
			TextColor color = TextColor.White;

			switch (Value)
			{
				case BlockFace.Down:
					color = TextColor.DarkGray;
					break;

				case BlockFace.Up:
					color = TextColor.White;
					break;

				case BlockFace.East:
					color = TextColor.BrightGreen;
					break;

				case BlockFace.West:
					color = TextColor.Red;
					break;

				case BlockFace.North:
					color = TextColor.Blue;
					break;

				case BlockFace.South:
					color = TextColor.Yellow;
					break;

				case BlockFace.None:
					break;
			}
			return $"{Name}={color.ToString()}{StringValue}";
		}
	}
}
