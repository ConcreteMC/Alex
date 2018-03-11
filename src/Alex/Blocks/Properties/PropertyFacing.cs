using System;
using Alex.Blocks.State;
using Alex.ResourcePackLib.Json;

namespace Alex.Blocks.Properties
{
	public class PropertyFace : StateProperty<MiNET.BlockFace>
	{
		public PropertyFace(string name) : base(name)
		{
			
		}

		public override MiNET.BlockFace ParseValue(string value)
		{
			if (Enum.TryParse(value, true, out MiNET.BlockFace result))
			{
				return result;
			}

			return MiNET.BlockFace.None;
		}

		public override string ToString(MiNET.BlockFace v)
		{
			return v.ToString().ToLowerInvariant();
		}

		public override object[] GetValidValues()
		{
			return (object[]) Enum.GetValues(typeof(MiNET.BlockFace));
		}
	}
}
