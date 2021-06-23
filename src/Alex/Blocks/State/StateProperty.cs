using System;
using System.Collections.Generic;
using Alex.Blocks.Properties;
using Alex.Common.Blocks.Properties;

namespace Alex.Blocks.State
{
	public class StatePropertyComparer : EqualityComparer<StateProperty>
	{
		/// <inheritdoc />
		public override bool Equals(StateProperty? x, StateProperty? y)
		{
			if (x == null && y == null)
				return true;

			if (x != null && y != null)
			{
				return x.Identifier == y.Identifier;
			}

			return false;
		}

		/// <inheritdoc />
		public override int GetHashCode(StateProperty obj)
		{
			return obj.Identifier;
		}
	}
	
	public abstract class StateProperty : IStateProperty
	{
		//public static Dictionary<string, StateProperty> _registeredTypes = new Dictionary<string, StateProperty>(StringComparer.InvariantCultureIgnoreCase);
		public string Name { get; }

		/// <inheritdoc />
		public virtual string StringValue => Value.ToString();
		
		public object Value { get; set; }

		public readonly int Identifier;
		protected StateProperty(string name)
		{
			Name = name;
			Identifier = name.GetHashCode(StringComparison.InvariantCultureIgnoreCase);

			//_registeredTypes.TryAdd(name, this);
		}

		public abstract StateProperty WithValue(object value);

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			
			return obj.GetHashCode().Equals(GetHashCode());
		}
		
		public override int GetHashCode()
		{
			return HashCode.Combine(GetType().Name, Name, Value);
		}

		protected bool Equals(StateProperty other)
		{
			return Name == other.Name && Value == other.Value;
		}
		
		public bool Equals(IStateProperty other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			
			return other.GetHashCode().Equals(GetHashCode());
			if (other.GetType() != this.GetType()) return false;

			if (other is StateProperty sp)
				return Equals(sp);
			
			return false;
			//return other.GetHashCode().Equals(GetHashCode()) && Value.Equals(other);
		}
		
		public virtual string ToFormattedString()
		{
			return $"{Name}={Value}";
		}
	}

	public abstract class StateProperty<TType> : StateProperty, IStateProperty<TType>
	{
		public StateProperty(string name) : base(name)
		{

		}

		/// <inheritdoc />
		public new TType Value
		{
			get
			{
				return (TType)base.Value;
			}
			set
			{
				base.Value = value;
			}
		}

		/// <inheritdoc />
		public override StateProperty WithValue(object value)
		{
			if (value is TType t)
			{
				return WithValue(t);
			}

			return WithValue(ParseValue(value.ToString()));
		}

		protected abstract StateProperty<TType> WithValue(TType value);

		public abstract TType ParseValue(string value);
	}
}