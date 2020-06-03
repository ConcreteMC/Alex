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

		protected StateProperty(string name)
		{
			Name = name;

			//_registeredTypes.TryAdd(name, this);
		}

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
		protected StateProperty(string name) : base(name)
		{

		}

		public abstract TType ParseValue(string value);
	}
}