using System.Collections.Generic;
using Alex.Blocks.State;

namespace Alex.Blocks.Storage
{
    public class SimplePalette : IPallete
    {
        private Dictionary<uint, BlockState> IdToState { get; }
        private Dictionary<BlockState, uint> StateToId { get; }
       

        private uint _available = 0;
        
        public SimplePalette(int size)
        {
            IdToState = new Dictionary<uint, BlockState>();
            StateToId = new Dictionary<BlockState, uint>();
        }

        public uint GetId(BlockState state)
        {
            if (StateToId.TryGetValue(state, out var index))
            {
                return index;
            }

            return uint.MaxValue;
        }

        public uint Add(BlockState state)
        {
            if (StateToId.TryGetValue(state, out var id))
                return id;

            uint newIndex = _available++;
            StateToId.TryAdd(state, newIndex);
            IdToState.TryAdd(newIndex, state);

            return newIndex;
        }

        public BlockState Get(uint id)
        {
            if (IdToState.TryGetValue(id, out BlockState state))
                return state;

            return null;
        }

        public void Put(BlockState state, uint key)
        {
            IdToState[key] = state;
            StateToId[state] = key;
        }
    }
}