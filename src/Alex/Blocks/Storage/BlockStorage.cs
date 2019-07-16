using System.Collections.Generic;
using Alex.API.Blocks.State;
using Alex.Blocks.State;

namespace Alex.Blocks.Storage
{
    public class BlockStorage
    {
        private FlexibleStorage Storage { get; set; }

        private int _bits;
        private IntIdentityHashBiMap<IBlockState> Pallette { get; set; }

        private static IBlockState Air = BlockFactory.GetBlockState("minecraft:air");
        public BlockStorage()
        {
            _bits = 8;
            
            Storage = new FlexibleStorage(_bits, 4096);
            Pallette = new IntIdentityHashBiMap<IBlockState>((1 << _bits));

            Pallette.Add(Air);
        }

        public void Set(int x, int y, int z, IBlockState state)
        {
            var idx = GetIndex(x, y, z);
            Set(idx, state);
        }

        private void Set(int idx, IBlockState state)
        {
            uint i = IdFor(state); //BlockFactory.GetBlockStateId(state);

            Storage[idx] = i;
        }

        private uint IdFor(IBlockState state)
        {
            uint i = Pallette.GetId(state);

            if (i == uint.MaxValue)
            {
                i = Pallette.Add(state);

                if (i >= 1 << this._bits)
                {
                    return Resize(_bits + 1, state);
                }
            }

            return i;
        }

        private uint Resize(int bits, IBlockState state)
        {
            var oldStorage = Storage;
            
            var oldPallete = Pallette; 
            
            _bits = bits;

            Pallette = new IntIdentityHashBiMap<IBlockState>(1 << bits);
            Storage = new FlexibleStorage(bits, 4096);
            IdFor(Air);

            for (int i = 0; i < oldStorage.Length; i++)
            {
                var oldEntry = oldPallete.Get(oldStorage[i]);
                if (oldEntry != null)
                {
                    Set(i, oldEntry);
                }
            }

            return IdFor(state);
        }

        public IBlockState Get(int x, int y, int z)
        {
            return Get(GetIndex(x, y, z));
        }

        private IBlockState Get(int index)
        {
            return Pallette.Get(Storage[index]);
        }

        private static int GetIndex(int x, int y, int z)
        {
            return y << 8 | z << 4 | x;
        }
    }
}