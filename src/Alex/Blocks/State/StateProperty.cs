using System;
using System.Collections.Generic;
using Alex.API.Blocks.Properties;
using Alex.Blocks.Properties;

namespace Alex.Blocks.State
{
	public abstract class StateProperty : IStateProperty
	{
		public static Dictionary<string, StateProperty> _registeredTypes = new Dictionary<string, StateProperty>(StringComparer.InvariantCultureIgnoreCase);
		public string Name { get; }
		public Type PropertyType { get; }
		public object DefaultValue { get; set; }

		protected StateProperty(string name, Type propertyType)
		{
			Name = name;
			PropertyType = propertyType;

			_registeredTypes.TryAdd(name, this);
		}

		public abstract object ValueFromString(string value);
		public abstract object[] GetValidValues();

		public static StateProperty Parse(string name)
		{
			if (_registeredTypes.TryGetValue(name, out StateProperty property))
			{
				return property;
			}

			return new UnknownProperty(name);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((StateProperty)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Name != null ? Name.GetHashCode() : 0) * 397) /*^ (PropertyType != null ? PropertyType.GetHashCode() : 0)*/;
			}
		}

		public bool Equals(IStateProperty other)
		{
			return other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase);/*&&
			       other.PropertyType.IsEquivalentTo(PropertyType);*/
		}
	}

	public abstract class StateProperty<TType> : StateProperty, IStateProperty<TType>
	{
		protected StateProperty(string name) : base(name, typeof(TType))
		{

		}

		public abstract TType ParseValue(string value);
		public abstract string ToString(TType v);
		public TType GetDefaultValue()
		{
			if (DefaultValue is TType typedDefault)
			{
				return typedDefault;
			}

			return default(TType);
		}

		public override object ValueFromString(string value)
		{
			return ParseValue(value);
		}
	}
}