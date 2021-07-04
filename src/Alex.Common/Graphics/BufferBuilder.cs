using Alex.Common.Graphics.GpuResources;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Common.Graphics
{
    public class BufferBuilder<TVertexType> where TVertexType : struct, IVertexType
    {
        public int IndexSize { get; }
        public int DataSize { get; }
        public int Size { get; }

        public VertexBuffer VertexBuffer { get; }
        public IndexBuffer IndexBuffer { get; }

        private short[] Index { get; }
        private TVertexType[] Data { get; }
        private GraphicsDevice GraphicsDevice { get; }

        public BufferBuilder(GraphicsDevice graphicsDevice, int size)
        {
            GraphicsDevice = graphicsDevice;

            Size = size;
            IndexSize = Size * 6;
            DataSize = Size * 4;

            Index = new short[IndexSize];
            Data = new TVertexType[DataSize];
            
            IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, IndexSize, BufferUsage.WriteOnly);
            VertexBuffer = new VertexBuffer(graphicsDevice, typeof(TVertexType), DataSize, BufferUsage.WriteOnly);

        }

        public void Build()
        {
            BuildIndex();
            VertexBuffer.SetData<TVertexType>(Data);
            IndexBuffer.SetData<short>(Index);
        }

        public void Bind()
        {
            GraphicsDevice.SetVertexBuffer(VertexBuffer);
            GraphicsDevice.Indices = IndexBuffer;
        }

        public void BuildIndex()
        {
            for (int i = 0; i < Size; i++)
            {
                var indexerIndex = i * 6;
                var dataIndex    = i * 4;
                
                Index[indexerIndex + 0] = (short) (dataIndex + 0);
                Index[indexerIndex + 2] = (short) (dataIndex + 1);
                Index[indexerIndex + 1] = (short) (dataIndex + 2);
					
                Index[indexerIndex + 3] = (short) (dataIndex + 2);
                Index[indexerIndex + 5] = (short) (dataIndex + 3);
                Index[indexerIndex + 4] = (short) (dataIndex + 0);
            }
        }
        
        public ref TVertexType this[int index, int subIndex] => ref Data[index * 4 + subIndex];
    }
}
