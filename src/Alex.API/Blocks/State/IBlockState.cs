using System;
using System.Collections.Generic;
using Alex.API.Blocks.Properties;
using Alex.API.Graphics;
using Alex.API.World;

namespace Alex.API.Blocks.State
{
	public interface IBlockState : IEquatable<IBlockState>
	{
		string Name { get; set; }
		uint ID { get; set; }
		IBlockModel Model { get; set; }
		IBlock Block { get; set; }

		T GetTypedValue<T>(IStateProperty<T> property);

		object GetValue(IStateProperty property);
		IBlockState WithProperty(IStateProperty property, object value);
		IBlockState WithProperty<T>(IStateProperty<T> property, T value);
		IDictionary<IStateProperty, string> ToDictionary();
		IBlockState Clone();

		bool TryGetValue(IStateProperty property, out object value);
		bool TryGetValue(string property, out string value);
	}
}
