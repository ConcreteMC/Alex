using System.Collections.Generic;
using Alex.Blocks.State;

namespace Alex.Blocks.Storage.Palette
{
    public class SimplePalette : IPallete
    {
        private Dictionary<uint, BlockState> _idToState;
        private Dictionary<BlockState, uint> _stateToId;
       

        private uint _available = 0;
        
        public SimplePalette(int size)
        {
            _idToState = new Dictionary<uint, BlockState>();
            _stateToId = new Dictionary<BlockState, uint>();
        }

        public uint GetId(BlockState state)
        {
            if (_stateToId.TryGetValue(state, out var index))
            {
                return index;
            }

            return uint.MaxValue;
        }

        public uint Add(BlockState state)
        {
            if (_stateToId.TryGetValue(state, out var id))
                return id;

            uint newIndex = _available++;
            _stateToId.TryAdd(state, newIndex);
            _idToState.TryAdd(newIndex, state);

            return newIndex;
        }

        public BlockState Get(uint id)
        {
            if (_idToState.TryGetValue(id, out BlockState state))
                return state;

            return null;
        }

        public void Put(BlockState state, uint key)
        {
            _idToState[key] = state;
            _stateToId[state] = key;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _idToState?.Clear();
            _idToState = null;

            _stateToId?.Clear();
            _stateToId = null;
        }
    }
}