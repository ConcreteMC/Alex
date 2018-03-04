using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.ResourcePackLib.Json;
using Alex.Utils;

namespace Alex.Blocks.Properties
{
	public class PropertyFace : PropertyHelper<string>
	{
		private BlockFace[] AllowedValues;

		protected PropertyFace(string name) : base(name, typeof(BlockFace))
		{
			AllowedValues = (BlockFace[]) Enum.GetValues(typeof(BlockFace));
		}

		public override ICollection<string> GetAllowedValues()
		{
			return this.AllowedValues.Cast<string>().ToArray();
		}

		public static PropertyFace Create(string name)
		{
			return new PropertyFace(name);
		}

		public override string ParseValue(string value)
		{
			return value;
			/*if (Enum.TryParse(value, out string result))
			{
				return result;
			}

			return ""; BlockFace.None;*/
		}
	}
}
