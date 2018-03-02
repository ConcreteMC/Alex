using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Blocks.State;
using Alex.API.World;

namespace Alex.Blocks.State {

	public abstract class BlockStateBase : IBlockState
	{
		private Dictionary<object, object> Values { get; } = new Dictionary<object, object>();
		private Dictionary<string, object> Keys { get; } = new Dictionary<string, object>();
		public ICollection<IProperty<T>> GetPropertyKeys<T>() where T : IComparable<T>
		{
			return Keys.Values.Cast<IProperty<T>>().ToArray();
		}

		public T GetValue<T>(IProperty<T> property) where T : IComparable<T>
		{
			if (Values.TryGetValue(property, out object v))
			{
				return (T) v;
			}

			return default(T);
		}

		public IBlockState WithProperty<T, TValue>(IProperty<T> property, TValue value) where T : IComparable<T> where TValue : T
		{
			if (Values.ContainsKey(property.Name))
			{
				Values[property.Name] = value;
				return this;
			}

			Values.TryAdd(property.Name, value);
			return this;
		}

		public IBlockState CycleProperty<T>(IProperty<T> property) where T : IComparable<T>
		{
			throw new NotImplementedException();
		}

		public IReadOnlyDictionary<IProperty<TKey>, IComparable<TValue>> GetProperties<TKey, TValue>() where TKey : IComparable<TKey>
		{
			throw new NotImplementedException();
		}

		public abstract IBlock GetBlock();
	}
}
