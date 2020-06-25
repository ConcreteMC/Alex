using System.Buffers;

namespace Alex.Blocks.Storage
{
    public class QuickStorage : IStorage
    {
        //private static ArrayPool<uint> _arrayPool = ArrayPool<uint>.Shared;
        
        private uint[] Data { get; }
        private int MaxSize { get; }
        public QuickStorage(int bits, int size)
        {
            MaxSize = size;
            Data = new uint[size]; //_arrayPool.Rent(size);// new uint[size];
        }
        
        public uint this[int index]
        {
            get => Data[index];
            set => Data[index] = value;
        }

        public int Length => Data.Length;

        public void Dispose()
        {
            //_arrayPool.Return(Data);
        }
    }
}