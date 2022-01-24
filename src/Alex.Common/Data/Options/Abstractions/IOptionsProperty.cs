using System;

namespace Alex.Common.Data.Options
{
	public interface IOptionsProperty
	{
		void ResetToDefault();

		object GetValue();

		void SetValue(object obj);

		Type PropertyType { get; }
	}
}