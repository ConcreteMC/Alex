using System;
using System.Collections.Generic;
using Alex.Blocks.Properties;
using Alex.Common.Blocks.Properties;

namespace Alex.Blocks.State
{
	public class StatePropertyComparer : EqualityComparer<IStateProperty>
	{
		/// <inheritdoc />
		public override bool Equals(IStateProperty x, IStateProperty y)
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
		public override int GetHashCode(IStateProperty obj)
		{
			return obj.Identifier;
		}
	}
	
	public abstract class StateProperty : StateProperty<object>
	{
		//public static Dictionary<string, StateProperty> _registeredTypes = new Dictionary<string, StateProperty>(StringComparer.InvariantCultureIgnoreCase);
		protected StateProperty(string name) : base(name)
		{
			//_registeredTypes.TryAdd(name, this);
		}
	}

	public class StateProperty<TType> : IStateProperty<TType>
	{
		private TType _value;
		public string Name { get; }
		
		private string _stringValue { get; set; } = string.Empty;
		/// <inheritdoc />
		public string StringValue => _stringValue;

		public TType DefaultValue { get; set; } = default(TType);
		public int Identifier { get; }
		protected StateProperty(string name)
		{
			Name = name;
			Identifier = name.GetHashCode(StringComparison.OrdinalIgnoreCase);
		}

		public TType GetValue()
		{
			return Value;
		}

		public TType GetValue(BlockState blockState)
		{
			return blockState.GetValue(this);
		}

		/// <inheritdoc />
		public TType Value
		{
			get => _value;
			protected internal set
			{
				_value = value;
				_stringValue = StringifyValue(value);
			}
		}

		protected virtual string StringifyValue(TType value)
		{
			return value.ToString();
		}

		public virtual IStateProperty<TType> WithValue(TType value)
		{
			var copy = (StateProperty<TType>)CreateCopy();
			copy.Value = value;

			return copy;
		}

		public virtual TType ParseValue(string value)
		{
			throw new NotImplementedException();
		}
		
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

		protected bool Equals(StateProperty<TType> other)
		{
			return Name == other.Name && Value.Equals(other.Value);
		}
		
		protected bool Equals<T>(StateProperty<T> other)
		{
			return Name == other.Name && Value.Equals(other.Value);
		}
		
		public bool Equals(IStateProperty other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			
			return other.GetHashCode().Equals(GetHashCode());
		}
		
		public virtual string ToFormattedString()
		{
			return $"{Name}={StringValue}";
		}

		public virtual IStateProperty<TType> CreateCopy()
		{
			return new StateProperty<TType>(Name).WithValue(Value);
		}
	}
}