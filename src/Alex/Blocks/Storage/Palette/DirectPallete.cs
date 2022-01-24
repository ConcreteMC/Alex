using System;
using Alex.Blocks.State;

namespace Alex.Blocks.Storage.Palette
{
	public interface IHasKey
	{
		uint Id { get; }
	}

	public class DirectPallete<TValue> : IPallete<TValue> where TValue : class, IHasKey
	{
		private readonly Func<uint, TValue> _getById;

		public DirectPallete(Func<uint, TValue> getById)
		{
			_getById = getById;
		}

		public uint GetId(TValue state)
		{
			return state.Id;
		}

		public uint Add(TValue state)
		{
			throw new System.NotImplementedException();
		}

		public TValue Get(uint id)
		{
			return _getById(id); // BlockFactory.GetBlockState(id);
		}

		public void Put(TValue objectIn, uint intKey) { }

		/// <inheritdoc />
		public void Dispose() { }
	}
}