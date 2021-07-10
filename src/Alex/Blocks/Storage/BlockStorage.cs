using System;
using System.Collections.Generic;
using Alex.Blocks.State;
using Alex.Blocks.Storage.Palette;
using Alex.Networking.Java.Util;
using NLog;

namespace Alex.Blocks.Storage
{
    public class BlockStorage : IDisposable
    {
        private IStorage Storage { get; set; }

        private int _bits;
        private IPallete Pallette { get; set; }

        private BlockState Air { get; }
        private object _lock = new object();
        public BlockStorage()
        {
            _bits = 8;
            Air = BlockFactory.GetBlockState("minecraft:air");
            
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
          //  lock (_lock)
            {
                uint i = IdFor(state); //BlockFactory.GetBlockStateId(state);

                Storage[idx] = i;
            }
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

                        try
                        {
                            Storage = new FlexibleStorage(newBits, 4096);

                            for (int s = 0; s < 4096; s++)
                            {
                                Storage[s] = old[s];
                            }
                        }
                        finally
                        {
                            old?.Dispose();
                        }
                    }
                    else
                    {
                        var bits = (int) Math.Ceiling(Math.Log2(BlockFactory.AllBlockstates.Count));
                        var oldPalette = Pallette;
                        var oldStorage = Storage;

                        try
                        {
                            Pallette = new DirectPallete();
                            Storage = new FlexibleStorage(bits, 4096);

                            for (int s = 0; s < 4096; s++)
                            {
                                var oldValue = oldStorage[s];
                                var newValue = oldPalette.GetId(oldPalette.Get(oldValue));
                                Storage[s] = newValue;
                                //data.set(i, newValue);
                            }
                        }
                        finally
                        {
                            oldPalette?.Dispose();
                            oldStorage?.Dispose();
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
           // lock (_lock)
            {
                var result = Pallette.Get(Storage[index]);

                if (result == null)
                    return Air;

                return result;
            }
        }

        private static int GetIndex(int x, int y, int z)
        {
            return y << 8 | z << 4 | x;
        }
        
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void Read(MinecraftStream ms)
        {
            var blockCount = ms.ReadShort();
            int bitsPerBlock = (byte) ms.ReadByte();

            int palleteLength = 0; // = ms.ReadVarInt();

            if (bitsPerBlock <= 4)
                bitsPerBlock = 4;

            var oldStorage = Storage;
            var oldPalette = Pallette;
            
            try
            {
                if (bitsPerBlock <= 8)
                {
                    _bits = bitsPerBlock;

                    palleteLength = ms.ReadVarInt();

                    Pallette = new IntIdentityHashBiMap(palleteLength + 1);
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
                    _bits = bitsPerBlock = (int) Math.Ceiling(Math.Log2(BlockFactory.AllBlockstates.Count));
                    Pallette = new DirectPallete();
                }
                
                int length = ms.ReadVarInt();
                long[] dataArray = new long[length];

                for (int i = 0; i < dataArray.Length; i++)
                {
                    dataArray[i] = ms.ReadLong();
                }
                
                Storage = new FlexibleStorage(bitsPerBlock, 4096);
                var valueMask = (uint) ((1L << bitsPerBlock) - 1);

                int bitOffset = 0;

                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        for (int x = 0; x < 16; x++)
                        {
                            if (64 - (bitOffset % 64) < bitsPerBlock)
                            {
                                bitOffset += 64 - (bitOffset % 64);
                            }

                            int startLongIndex = bitOffset / 64;
                            int end_long_index = startLongIndex;
                            int startOffset = bitOffset % 64;
                            bitOffset += bitsPerBlock;

                            uint rawId;

                            if (startLongIndex == end_long_index)
                            {
                                rawId = (uint) (dataArray[startLongIndex] >> startOffset);
                            }
                            else
                            {
                                int endOffset = 64 - startOffset;

                                rawId = (uint) (dataArray[startLongIndex] >> startOffset
                                                | dataArray[end_long_index] << endOffset);
                            }

                            rawId &= valueMask;

                            Storage[(((y * 16) + z) * 16) + x] = rawId;
                        }
                    }
                }

                /*   for (int index = 0; index < 4096; index++)
                   {
                       var state = index / (64 / bitsPerBlock);
   
                       var data = dataArray[state];
   
                       var shiftedData = data >> (index % (64 / bitsPerBlock) * bitsPerBlock);
   
                       Storage[index] = (uint) (shiftedData & valueMask);
                   }*/
                //Storage = storage;
            }
            finally
            {
                oldPalette?.Dispose();
                oldStorage?.Dispose();
            }
        }

        public void Dispose()
        {
            Storage?.Dispose();
            Pallette?.Dispose();
        }
    }
}