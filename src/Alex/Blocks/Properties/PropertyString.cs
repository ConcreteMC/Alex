using System;
using System.Collections.Generic;
using Alex.Blocks.State;
using Alex.Common.Blocks.Properties;

namespace Alex.Blocks.Properties
{
	public class PropertyString : StateProperty<string>
	{
		private readonly string _defaultValue = "";

		public PropertyString(string name, string defaultValue = "") : base(name)
		{
			_defaultValue = defaultValue;
			Value = defaultValue;
		}

		/// <inheritdoc />
		public override IStateProperty<string> WithValue(string value)
		{
			return new PropertyString(Name, _defaultValue) { Value = value };
		}

		public override string ParseValue(string value)
		{
			return value;
		}
	}

	public class ValidatedPropertyString : StateProperty<string>
	{
		private readonly string _defaultValue = "";
		private IReadOnlyDictionary<string, IStateProperty<string>> _propertyStrings;

		public ValidatedPropertyString(string name, string[] values, string defaultValue = "") : base(name)
		{
			_defaultValue = defaultValue;
			Value = defaultValue;

			var p = new PropertyString(name);
			var properties = new Dictionary<string, IStateProperty<string>>(StringComparer.OrdinalIgnoreCase);

			foreach (var prop in values)
			{
				properties.Add(name, p.WithValue(prop));
			}

			_propertyStrings = properties;
		}

		/// <inheritdoc />
		public override IStateProperty<string> WithValue(string value)
		{
			if (_propertyStrings.TryGetValue(value, out var state))
				return state;

			return new PropertyString(Name, _defaultValue) { Value = value };
		}

		public override string ParseValue(string value)
		{
			return value;
		}
	}
}