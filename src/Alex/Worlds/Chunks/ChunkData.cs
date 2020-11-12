using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.API;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Worlds.Chunks
{
    internal class ChunkData : IDisposable
    {
        public PooledVertexBuffer Buffer      { get; private set; }
        public ChunkCoordinates   Coordinates { get; set; }
        public object             WriteLock   { get; } = new object();
        
        public  BlockShaderVertex[]                               Vertices     { get; set; }
        public  ConcurrentDictionary<RenderStage, ChunkRenderStage>         RenderStages { get; set; }
        private ConcurrentDictionary<BlockCoordinates, List<int>> BlockIndices { get; set; }
        private ConcurrentQueue<int> AvailableIndices { get; }
        
        public ChunkData()
        {
            RenderStages = new ConcurrentDictionary<RenderStage, ChunkRenderStage>();
            BlockIndices = new ConcurrentDictionary<BlockCoordinates, List<int>>();
            AvailableIndices = new ConcurrentQueue<int>();
            
            Vertices = new BlockShaderVertex[NextPowerOf2(4096 * 6)];
            for(int i = 0; i < Vertices.Length; i++)
                AvailableIndices.Enqueue(i);
        }

        public bool Ready { get; private set; } = false;
        
        private bool HasChanges { get; set; }
        private bool HasResized { get; set; } = false;
        
        private static int NextPowerOf2(int n)
        {
            // decrement n (to handle the case when n itself
            // is a power of 2)
            n -= 1;

            // do till only one bit is left
            while ((n & n - 1) != 0)
                n &= n - 1;	// unset rightmost bit

            // n is now a power of two (less than n)

            // return next power of 2
            return n << 1;
        }
        
        private int GetIndex()
        {
            int availableIndex = -1;

            if (AvailableIndices.TryDequeue(out availableIndex))
                return availableIndex;

            var vertices = Vertices;
            
            int oldSize  = vertices.Length;
            Array.Resize(ref vertices, NextPowerOf2(oldSize + 1));
            int newSize = vertices.Length;
            
            Vertices = vertices;
            
            for(int i = oldSize + 1; i < newSize; i++)
                AvailableIndices.Enqueue(i);

            HasResized = true;
            return oldSize;
        }

        private LinkedList<int> _intermediateChanges = new LinkedList<int>();

        public void ApplyIntermediate()
        {
            if (Buffer == null || _intermediateChanges.Count == 0)
                return;
            
            var changes = _intermediateChanges.ToArray();    
            _intermediateChanges.Clear();
            
            BlockShaderVertex[] vertices = new BlockShaderVertex[changes.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = Vertices[changes[i]];
            }
            
            Buffer.SetData(vertices, changes[0], vertices.Length);
        }
        
        private void Set(int index, BlockShaderVertex vertex)
        {
            Vertices[index] = vertex;
            
            if (Buffer != null && index < Buffer.VertexCount)
            {
                var v = _intermediateChanges.First;
                while (v?.Next != null)
                {
                    if (index == v.Value + 1)
                    {
                        _intermediateChanges.AddAfter(v, index);

                        return;
                    }
                    else if (index == v.Value - 1)
                    {
                        _intermediateChanges.AddBefore(v, index);

                        return;
                    }
                    else
                    {
                        v = v.Next;
                    }
                }
                /*var lastValue = _intermediateChanges.Last();

                if (index == lastValue + 1)
                {
                    _intermediateChanges.AddLast(index);
                    return;
                }
                
                var firstValue = _intermediateChanges.Last();
                if (index == firstValue + 1)
                {
                    _intermediateChanges.AddFirst(index);

                    return;
                }
                
                if (index == firstValue - 1)
                {
                    _intermediateChanges.AddFirst(index);

                    return;
                }*/
                
                ApplyIntermediate();
            }
        }
        
        public int AddVertex(BlockCoordinates blockCoordinates, BlockShaderVertex vertex)
        {
            int index = GetIndex();
            
            Set(index, vertex);
            BlockIndices.GetOrAdd(blockCoordinates, coordinates => new List<int>()).Add(index);

            HasChanges = true;

            return index;
        }

        public void AddIndex(BlockCoordinates blockCoordinates, RenderStage stage, int index)
        {
            var rStage = RenderStages.GetOrAdd(stage, renderStage => new ChunkRenderStage(this, Vertices.Length));
            rStage.Add(blockCoordinates, index);
        }

        public void Remove(BlockCoordinates blockCoordinates)
        {
            if (BlockIndices.TryRemove(blockCoordinates, out var indices))
            {
                foreach (var index in indices.OrderBy(x => x))
                {
                    //Vertices[index] = BlockShaderVertex.Default;
                    Set(index, BlockShaderVertex.Default);
                    AvailableIndices.Enqueue(index);
                }

                foreach (var stage in RenderStages.Values.ToArray())
                {
                    stage.Remove(blockCoordinates);
                }

                HasChanges = true;
            }
        }

        public void ApplyChanges(GraphicsDevice device)
        {
            if (!HasChanges)
                return;

            ApplyIntermediate();
            
            bool               callSetData = HasResized;
            PooledVertexBuffer oldBuffer   = null;
            PooledVertexBuffer buffer      = Buffer;
            if (buffer == null || buffer.VertexCount < Vertices.Length)
            {
                oldBuffer = buffer;

                buffer = GpuResourceManager.GetBuffer(
                    this, device, BlockShaderVertex.VertexDeclaration, Vertices.Length, BufferUsage.WriteOnly);

                callSetData = true;
            }
            
            if (callSetData)
                buffer.SetData(Vertices);
            
            foreach (var renderstage in RenderStages)
            {
                renderstage.Value.Apply(device);
            }
            
            Buffer = buffer;
            oldBuffer?.MarkForDisposal();
            
            HasResized = false;
            HasChanges = false;

            Ready = true;
        }

        public bool Disposed { get; private set; } = false;
        public void Dispose()
        {
           // lock (WriteLock)
            {
                Buffer?.MarkForDisposal();

                foreach (var stage in RenderStages)
                {
                    stage.Value.Dispose();
                }

                RenderStages.Clear();

                Disposed = true;
            }
        }
    }

    internal class ChunkRenderStage : IDisposable
    {
        private ChunkData Parent { get; }
        public PooledIndexBuffer IndexBuffer { get; set; }
        
        public  int[]                                             Indices      { get; }
        private ConcurrentDictionary<BlockCoordinates, List<int>> BlockIndices { get; set; }
        public ChunkRenderStage(ChunkData parent, int size)
        {
            Parent = parent;
            Indices = new int[size];
            BlockIndices = new ConcurrentDictionary<BlockCoordinates, List<int>>();
        }

        public void Add(BlockCoordinates coordinates, int value)
        {
            BlockIndices.GetOrAdd(coordinates, blockCoordinates => new List<int>()).Add(value);
        }

        public void Remove(BlockCoordinates coordinates)
        {
            BlockIndices.TryRemove(coordinates, out var _);
        }
        
        public void Apply(GraphicsDevice device)
        {
            PooledIndexBuffer oldBuffer  = null;
            PooledIndexBuffer buffer     = IndexBuffer;
            List<int>         newIndices = new List<int>();

            foreach (var bl in BlockIndices)
            {
                newIndices.AddRange(bl.Value);
            }
            
            if (buffer == null || buffer.IndexCount < newIndices.Count)
            {
                oldBuffer = buffer;

                buffer = GpuResourceManager.GetIndexBuffer(
                    this, device, IndexElementSize.ThirtyTwoBits, newIndices.Count, BufferUsage.WriteOnly);
            }

            buffer.SetData<int>(newIndices.ToArray());

            IndexBuffer = buffer;
            oldBuffer?.MarkForDisposal();
        }
        
        public virtual int Render(GraphicsDevice device, Effect effect)
        {
            if (IndexBuffer == null || Parent.Buffer == null) return 0;
            device.SetVertexBuffer(Parent.Buffer);
            device.Indices = IndexBuffer;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexBuffer.IndexCount / 3);
            }

            return IndexBuffer.IndexCount;
        }

        public void Dispose()
        {
            IndexBuffer?.MarkForDisposal();
            //Buffer?.MarkForDisposal();
        }
    }
}