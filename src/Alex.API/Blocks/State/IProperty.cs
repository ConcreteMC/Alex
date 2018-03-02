using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.API.Blocks.State
{
	public interface IProperty<T> where T : IComparable<T>
	{
		string Name { get; }

		Type GetValueType();
		ICollection<T> GetAllowedValues();
		T ParseValue(string value);
	}
}
