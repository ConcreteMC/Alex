using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.Blocks.Properties
{
	public class PropertyBool : PropertyHelper<bool>
	{
		private bool[] AllowedValues = new[] {true, false};

		protected PropertyBool(String name) : base(name, typeof(bool))
		{
		
		}

		public override ICollection<bool> GetAllowedValues()
		{
			return this.AllowedValues;
		}

		public static PropertyBool Create(string name)
		{
			return new PropertyBool(name);
		}

		public override bool ParseValue(string value)
		{
			if (bool.TryParse(value, out bool result))
			{
				return result;
			}

			return false;
		}

		public bool Equals(Object p_equals_1_)
		{
			if (this == p_equals_1_)
			{
				return true;
			}
			else if (p_equals_1_ is PropertyBool && base.Equals(p_equals_1_))
			{
				PropertyBool propertybool = (PropertyBool) p_equals_1_;
				return this.AllowedValues.Equals(propertybool.AllowedValues);
			}
			else
			{
				return false;
			}
		}
	}
}
