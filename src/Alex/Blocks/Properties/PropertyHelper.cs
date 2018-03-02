using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Blocks.State;
using Alex.Blocks.State;

namespace Alex.Blocks.Properties
{
	public abstract class PropertyHelper<T> : IProperty<T> where T : IComparable<T>, IComparable
	{
		public Type ValueType { get; private set; }
		public string Name { get; private set; }

		protected PropertyHelper(string name, Type valueClass)
		{
			this.ValueType = valueClass;
			this.Name = name;
		}

		public Type GetValueType()
		{
			return ValueType;
		}

		public abstract ICollection<T> GetAllowedValues();

		public abstract T ParseValue(string value);

		//public override string ToString()
		//{
		//	return MoreObjects.toStringHelper(this).add("name", this.name).add("clazz", this.valueClass).add("values", this.getAllowedValues()).toString();
		//}
	}
}
