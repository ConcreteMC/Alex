using System;

namespace Alex.Blocks.Storage.Palette
{
	public interface IPalette<TValue> : IDisposable where TValue : class
	{
		uint GetId(TValue state);

		uint Add(TValue state);

		TValue Get(uint id);

		void Put(TValue objectIn, uint intKey);
	}
}