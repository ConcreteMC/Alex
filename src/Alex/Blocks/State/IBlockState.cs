using System;
using System.Collections.Generic;
using Alex.API.Blocks.Properties;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.Graphics.Models.Blocks;

namespace Alex.API.Blocks.State
{
	public interface IBlockState : IEquatable<IBlockState>
	{
		string Name { get; set; }
		uint ID { get; set; }
		bool Default { get; set; }
		BlockModel Model { get; set; }
		Block Block { get; set; }

		T GetTypedValue<T>(IStateProperty<T> property);

		object GetValue(string property);
		//IBlockState WithProperty(IStateProperty property, object value);
		IBlockState WithProperty(string property, string value, bool prioritize = false, params string[] requiredMatches);
		//IBlockState WithProperty<T>(IStateProperty<T> property, T value);
		IDictionary<string, string> ToDictionary();
		IBlockState Clone();

		//bool TryGetValue(IStateProperty property, out object value);
		bool TryGetValue(string property, out string value);

		bool ExactMatch(IBlockState other);
	}
}
