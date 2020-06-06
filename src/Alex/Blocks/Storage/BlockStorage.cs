using System;
using System.Collections.Generic;
using Alex.API.Blocks.State;
using Alex.Blocks.State;
using Alex.Networking.Java.Util;
using NLog;

namespace Alex.Blocks.Storage
{
    public class BlockStorage : IDisposable
    {
        private IStorage Storage { get; set; }

        private int _bits;
        private IPallete<BlockState> Pallette { get; set; }

        private static BlockState Air = BlockFactory.GetBlockState("minecraft:air");
        public BlockStorage()
        {
            _bits = 8;
            
            Storage = new FlexibleStorage(_bits, 4096);
            Pallette = new IntIdentityHashBiMap<BlockState>((1 << _bits));

            Pallette.Add(Air);
        }

        public void Set(int x, int y, int z, BlockState state)
        {
            var idx = GetIndex(x, y, z);
            Set(idx, state);
        }

        private void Set(int idx, BlockState state)
        {
            uint i = IdFor(state); //BlockFactory.GetBlockStateId(state);

            Storage[idx] = i;
        }

        private uint IdFor(BlockState state)
        {
            uint i = Pallette.GetId(state);

            if (i == uint.MaxValue)
            {
                i = Pallette.Add(state);

                if (i >= (1 << this._bits))
                {
                    return Resize(_bits + 1, state);
                }
            }

            return i;
        }

        private uint Resize(int bits, BlockState state)
        {
            var oldStorage = Storage;
            
            var oldPallete = Pallette; 
            
            _bits = bits;

            Pallette = new IntIdentityHashBiMap<BlockState>(1 << bits);
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

        public BlockState Get(int x, int y, int z)
        {
            return Get(GetIndex(x, y, z));
        }

        private BlockState Get(int index)
        {
            var result = Pallette.Get(Storage[index]);
            if (result == null)
                return Air;

            return result;
        }

        private static int GetIndex(int x, int y, int z)
        {
            return y << 8 | z << 4 | x;
        }
        
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void Read(MinecraftStream ms)
        {
            var blockCount = ms.ReadShort();
            var bitsPerBlock = (byte) ms.ReadByte();

            int palleteLength = 0;// = ms.ReadVarInt();

            if (bitsPerBlock <= 4)
                bitsPerBlock = 4;
            
            if (bitsPerBlock <= 8)
            {
                _bits = bitsPerBlock;
                
                palleteLength = ms.ReadVarInt();
                
                Pallette = new IntIdentityHashBiMap<BlockState>(palleteLength);
                Pallette.Add(Air);
                
                //else
                //     palleteLength = 

                for (int id = 0; id < palleteLength; id++)
                {
                    uint stateId = (uint) ms.ReadVarInt();
                    BlockState state = BlockFactory.GetBlockState(stateId);
                    Pallette.Put(state, (uint) id);
                    // idToState.Set(id, state);
                    // stateToId.Set(state, id);
                }
            }
            else
            {
                _bits = (int) Math.Ceiling(Math.Log2(BlockFactory.AllBlockstates.Count));
                Pallette = new DirectPallete();
            }

            int length = ms.ReadVarInt();
            long[] dataArray = new long[length];
            for (int i = 0; i < dataArray.Length; i++)
            {
                dataArray[i] = ms.ReadLong();
            }

            Storage = new FlexibleStorage(_bits, dataArray);
            //Storage._data = dataArray;
        }

        public void Dispose()
        {
            
        }
    }
}