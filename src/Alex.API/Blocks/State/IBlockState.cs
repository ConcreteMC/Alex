using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.World;

namespace Alex.API.Blocks.State
{
	public interface IBlockState
	{
		ICollection<IProperty<T>> GetPropertyKeys<T>() where T : IComparable<T>;

		T GetValue<T>(IProperty<T> property) where T : IComparable<T>;

		IBlockState WithProperty<T, TValue>(IProperty<T> property, TValue value) where TValue : T where T : IComparable<T>;

		IBlockState CycleProperty<T>(IProperty<T> property) where T : IComparable<T>;

		IReadOnlyDictionary<IProperty<TKey>, IComparable<TValue>> GetProperties<TKey, TValue>() where TKey : IComparable<TKey>;

		IBlock GetBlock();
	}
}
