using System.Collections.Generic;
using Alex.API.Blocks.Properties;
using Alex.API.World;

namespace Alex.API.Blocks.State
{
	public interface IBlockState
	{
		T GetTypedValue<T>(IStateProperty<T> property);

		object GetValue(IStateProperty property);
		IBlockState WithProperty(IStateProperty property, object value);
		IDictionary<IStateProperty, object> ToDictionary();
		IBlock GetBlock();
		void SetBlock(IBlock block);
	}
}
