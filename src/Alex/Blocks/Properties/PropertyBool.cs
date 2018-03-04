using System;
using Alex.Blocks.State;

namespace Alex.Blocks.Properties
{
	public class PropertyBool : StateProperty<bool>
	{
		public PropertyBool(string name) : base(name)
		{
		
		}

		public override bool ParseValue(string value)
		{
			if (bool.TryParse(value, out var result))
			{
				return result;
			}

			return false;
		}
	}
}
