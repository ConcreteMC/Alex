using System;
using System.Collections.Generic;
using Alex.Blocks.State;
using Alex.Networking.Java.Util;
using NLog;

namespace Alex.Blocks.Storage
{
    public class BlockStorage : IDisposable
    {
        private IStorage Storage { get; set; }

        private int _bits;
        private IPallete Pallette { get; set; }

        private static BlockState Air = BlockFactory.GetBlockState("minecraft:air");
        public BlockStorage()
        {
            _bits = 8;
            
            Storage = new FlexibleStorage(_bits, 4096);
            Pallette = new IntIdentityHashBiMap((1 << _bits));

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
                    var newBits = _bits + 1;
                    if (newBits < 8)
                    {
                        var old = Storage;
                        Storage = new FlexibleStorage(newBits, 4096);
                        for (int s = 0; s < 4096; s++)
                        {
                            Storage[s] = old[s];
                        }
                    }
                    else
                    {
                        var bits = (int) Math.Ceiling(Math.Log2(BlockFactory.AllBlockstates.Count));
                        var oldPalette = Pallette;
                        var oldStorage = Storage;
                        Pallette = new DirectPallete();
                        Storage = new FlexibleStorage(bits, 4096);
                        for (int s = 0; s < 4096; s++)
                        {
                            var oldValue = oldStorage[s];
                            var newValue = oldPalette.GetId(oldPalette.Get(oldValue));
                            Storage[s] = newValue;
                            //data.set(i, newValue);
                        }

                        return Pallette.GetId(state);
                    }
                    //return Resize(_bits + 1, state);
                }
            }

            return i;
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
                
                Pallette = new IntIdentityHashBiMap(palleteLength);
                Pallette.Add(Air);

                for (int id = 0; id < palleteLength; id++)
                {
                    uint stateId = (uint) ms.ReadVarInt();
                    BlockState state = BlockFactory.GetBlockState(stateId);
                    Pallette.Put(state, (uint) id);
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

            Storage = new FlexibleStorage(_bits, 4096);
            var valueMask = (uint) ((1L << _bits) - 1);
            for(int index = 0; index < 4096; index++)
            {
                var state = index / (64 / _bits);
                var data = dataArray[state];

                var shiftedData = data >> (index % (64 / _bits) * _bits);

                Storage[index] = (uint) (shiftedData & valueMask);
            }
        }

        public void Dispose()
        {
            
        }
    }
}