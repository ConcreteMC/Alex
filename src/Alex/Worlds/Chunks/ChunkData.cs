using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.API;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Worlds.Chunks
{
    public class ChunkData : IDisposable
    {
        public PooledVertexBuffer Buffer      { get; private set; }

        public  BlockShaderVertex[]                                 Vertices         { get; private set; }
        public  ConcurrentDictionary<RenderStage, ChunkRenderStage> RenderStages     { get; set; }
        private ConcurrentDictionary<BlockCoordinates, List<int>>   BlockIndices     { get; set; }
        private SortedSet<int>                                         AvailableIndices { get; }

        private static long _instances = 0;
        private static long _totalSize = 0;
        private static long AverageSize => _instances > 0 && _totalSize > 0 ? _totalSize / _instances : 0;
        public ChunkData()
        {
            RenderStages = new ConcurrentDictionary<RenderStage, ChunkRenderStage>();
            BlockIndices = new ConcurrentDictionary<BlockCoordinates, List<int>>();
            AvailableIndices = new SortedSet<int>();

            Interlocked.Increment(ref _instances);
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

        private void Init()
        {
            Vertices = new BlockShaderVertex[AverageSize];
            for(int i = 0; i < Vertices.Length; i++)
                AvailableIndices.Add(i);

            Interlocked.Add(ref _totalSize, Vertices.Length);
        }
        
        private int GetIndex()
        {
            int availableIndex = -1;

            if (AvailableIndices.Count > 0)
            {
                availableIndex = AvailableIndices.Min;
                AvailableIndices.Remove(availableIndex);
                
                return availableIndex;
            }

            var vertices = Vertices;
            
            int oldSize  = vertices.Length;

            if (oldSize == 0)
                oldSize = 1024;
            
            Array.Resize(ref vertices, NextPowerOf2(oldSize + 1));
            int newSize = vertices.Length;
            
            Vertices = vertices;
            Interlocked.Add(ref _totalSize, newSize - oldSize);
            
            for(int i = oldSize + 1; i < newSize; i++)
                AvailableIndices.Add(i);

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
            
            Buffer.SetData(changes[0] * BlockShaderVertex.VertexDeclaration.VertexStride, vertices, 0, vertices.Length, Buffer.VertexDeclaration.VertexStride);
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

                ApplyIntermediate();
            }
        }
        
        public int AddVertex(BlockCoordinates blockCoordinates, BlockShaderVertex vertex)
        {
            if (Vertices == null)
                Init();
            
            int index = GetIndex();
            
            Set(index, vertex);
            BlockIndices.GetOrAdd(blockCoordinates, coordinates => new List<int>()).Add(index);

            HasChanges = true;

            return index;
        }

        public void AddIndex(BlockCoordinates blockCoordinates, RenderStage stage, int index)
        {
            var rStage = RenderStages.GetOrAdd(stage, renderStage => new ChunkRenderStage(this, Vertices.Length / 3));
            rStage.Add(blockCoordinates, index);

            HasChanges = true;
        }

        public void Remove(GraphicsDevice device, BlockCoordinates blockCoordinates)
        {
            if (BlockIndices.TryRemove(blockCoordinates, out var indices))
            {
                foreach (var index in indices.OrderBy(x => x))
                {
                    //Vertices[index] = BlockShaderVertex.Default;
                    Set(index, BlockShaderVertex.Default);
                    AvailableIndices.Add(index);
                }

                ApplyIntermediate();
                
                foreach (var stage in RenderStages.Values.ToArray())
                {
                    stage.Remove(blockCoordinates);
                       // stage.Apply(device);
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
               int size = Vertices != null ? Vertices.Length : 0;
                Buffer?.MarkForDisposal();

                foreach (var stage in RenderStages)
                {
                    stage.Value.Dispose();
                }

                RenderStages.Clear();
                BlockIndices.Clear();
                AvailableIndices.Clear();
                Vertices = null;
                
                Disposed = true;
                Interlocked.Decrement(ref _instances);
                Interlocked.Add(ref _totalSize, -size);
            }
        }
    }

    public class ChunkRenderStage : IDisposable
    {
        private ChunkData Parent { get; }
        public PooledIndexBuffer IndexBuffer { get; set; }
        
        //public  int[]                                             Indices      { get; }
        private ConcurrentDictionary<BlockCoordinates, List<int>> BlockIndices { get; set; }
        
        private bool _requiresUpdate = false;
        public ChunkRenderStage(ChunkData parent, int size)
        {
            Parent = parent;
           // Indices = new int[size];
            BlockIndices = new ConcurrentDictionary<BlockCoordinates, List<int>>();
        }

        public IEnumerable<int> GetIndexes()
        {
            foreach (var block in BlockIndices)
            {
                foreach (var i in block.Value)
                {
                    yield return i;
                }
            }
        }
        
        public void Add(BlockCoordinates coordinates, int value)
        {
            BlockIndices.GetOrAdd(coordinates, blockCoordinates => new List<int>()).Add(value);

            _requiresUpdate = true;
        }

        public bool Remove(BlockCoordinates coordinates)
        {
            if (BlockIndices.TryRemove(coordinates, out var _))
            {
                _requiresUpdate = true;

                return true;
            }

            return false;
        }
        
        public void Apply(GraphicsDevice device)
        {
            if (!_requiresUpdate)
                return;

            PooledIndexBuffer oldBuffer = null;
            PooledIndexBuffer buffer    = IndexBuffer;
            try
            {
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
            }
            finally
            {
                IndexBuffer = buffer;
                oldBuffer?.MarkForDisposal();
                
                _requiresUpdate = false;
            }
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