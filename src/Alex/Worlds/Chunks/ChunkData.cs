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
        private const int                DefaultSize = 64;
        public        PooledVertexBuffer Buffer { get; private set; }

        public  BlockShaderVertex[]                                 Vertices         { get; private set; }
        public  ConcurrentDictionary<RenderStage, ChunkRenderStage> RenderStages     { get; set; }
        private ConcurrentDictionary<BlockCoordinates, List<int>>   BlockIndices     { get; set; }
        private SortedSet<int> AvailableIndices { get; } //LinkedList<KeyValuePair<int, int>>                         AvailableIndices { get; }

        private static long _instances = 0;
        private static long _totalSize = 0;
        //private static long AverageSize => _instances > 0 && _totalSize > 0 ? _totalSize / _instances : DefaultSize;
        private static long AverageSize = DefaultSize;
        public ChunkData()
        {
            RenderStages = new ConcurrentDictionary<RenderStage, ChunkRenderStage>();
            BlockIndices = new ConcurrentDictionary<BlockCoordinates, List<int>>();
            AvailableIndices = new SortedSet<int>();// new LinkedList<KeyValuePair<int, int>>();

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
          //  AvailableIndices.AddFirst(new KeyValuePair<int, int>(0, Vertices.Length));
            for(int i = 0; i < Vertices.Length; i++)
                AvailableIndices.Add(i);

            Interlocked.Add(ref _totalSize, Vertices.Length);
        }

        private int GetIndex()
        {
            do
            {
                if (AvailableIndices.Count > 0)
                {
                    var availableIndex = AvailableIndices.Min;
                    AvailableIndices.Remove(availableIndex);

                    return availableIndex;
                }

                var vertices = Vertices;

                int oldSize = vertices.Length;

                if (oldSize == 0)
                    oldSize = DefaultSize;

                BlockShaderVertex[] newVertices = new BlockShaderVertex[oldSize * 2];
                Array.Copy(vertices, newVertices, vertices.Length);
                vertices = newVertices;
                //Array.Resize(ref vertices, oldSize * 2);
                int newSize = vertices.Length;

                Vertices = vertices;
                Interlocked.Add(ref _totalSize, newSize - oldSize);

                for (int i = oldSize; i < newSize; i++)
                    AvailableIndices.Add(i);

                HasResized = true;
                // return ;
            } while (true);
        }

        private LinkedList<int> _intermediateChanges = new LinkedList<int>();

        public void ApplyIntermediate()
        {
            if (Buffer == null || _intermediateChanges.Count == 0)
                return;
            
            var changes = _intermediateChanges.ToArray();    
            _intermediateChanges.Clear();

            Buffer.SetData(changes[0] * BlockShaderVertex.VertexDeclaration.VertexStride, Vertices, changes[0], changes.Length, BlockShaderVertex.VertexDeclaration.VertexStride);
            
            foreach (var stage in RenderStages.Values.ToArray())
            {
                stage.Apply();
            }
        }

        private void FreeIndex(int index, int count = 1)
        {
            Set(index, BlockShaderVertex.Default);
            AvailableIndices.Add(index);

           // foreach (var stage in RenderStages)
              //  stage.Value.Remove(index);

            return;
          /*  var first = AvailableIndices.First;

            bool added = false;
            if (first != null)
            {
                do
                {
                    if (first.Value.Key - 1 == index)
                    {
                        first.Value = new KeyValuePair<int, int>(first.Value.Key - 1, first.Value.Value + 1);
                        added = true;
                        break;
                    }
                    else if ((first.Value.Key + first.Value.Value) + 1 == index)
                    {
                        first.Value = new KeyValuePair<int, int>(first.Value.Key, first.Value.Value + count);
                        added = true;
                        break;
                    }

                    first = first.Next;
                } while (first?.Next != null);
            }

            if (!added)
            {
                AvailableIndices.AddLast(new KeyValuePair<int, int>(index, count - 1));
            }*/
        }

        private void Set(int index, BlockShaderVertex vertex)
        {
            Vertices[index] = vertex;

            return;
            if (Buffer != null && index < Buffer.VertexCount)
            {
                if (_intermediateChanges.Count == 0)
                {
                    _intermediateChanges.AddFirst(index);
                    return;
                }

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
                    
                    v = v.Next;
                }

                ApplyIntermediate();
                _intermediateChanges.AddFirst(index);
            }
        }
        
        public int AddVertex(BlockCoordinates blockCoordinates, BlockShaderVertex vertex)
        {
            if (Vertices == null)
                Init();
            
            int index = GetIndex();
            
            Set(index, vertex);

            BlockIndices.AddOrUpdate(
                blockCoordinates, coordinates => new List<int>() {index}, (coordinates, ints) =>
                {
                    ints.Add(index);

                    return ints;
                });
            //var indices = BlockIndices.GetOrAdd(blockCoordinates, coordinates => new List<int>());//.Add(index);
            //indices.Add(index);
            
            HasChanges = true;

            return index;
        }

        public void AddIndex(BlockCoordinates blockCoordinates, RenderStage stage, int index)
        {
            var rStage = RenderStages.GetOrAdd(stage, CreateRenderStage);
            rStage.Add(blockCoordinates, index);

            HasChanges = true;
        }

        private ChunkRenderStage CreateRenderStage(RenderStage arg)
        {
            return new ChunkRenderStage(Vertices.Length / 3);
        }

        public void Remove(GraphicsDevice device, BlockCoordinates blockCoordinates)
        {
            if (BlockIndices.TryRemove(blockCoordinates, out var indices))
            {
                foreach (var index in indices)
                {
                    FreeIndex(index);
                }

               // ApplyIntermediate();
                
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

            var                vertices    = Vertices;
            
            bool               callSetData = HasResized;
            PooledVertexBuffer oldBuffer   = null;
            PooledVertexBuffer buffer      = Buffer;
            if (buffer == null || buffer.VertexCount < vertices.Length)
            {
                oldBuffer = buffer;

                buffer = GpuResourceManager.GetBuffer(
                    this, device, BlockShaderVertex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);

                callSetData = true;
            }

          //  if (callSetData)
                buffer.SetData(vertices, 0, vertices.Length);
            
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
        private  PooledIndexBuffer                                 IndexBuffer  { get; set; }
        private ConcurrentDictionary<BlockCoordinates, List<int>> BlockIndices { get; set; }

        private bool _requiresUpdate = false;
        private int  _elementCount   = 0;
        
        private object _writeLock = new object();
        public ChunkRenderStage(int size)
        {
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
            lock (_writeLock)
            {
                BlockIndices.AddOrUpdate(
                    coordinates, blockCoordinates => new List<int>() {value}, (blockCoordinates, ints) =>
                    {
                        ints.Add(value);

                        return ints;
                    });

                // Interlocked.Increment(ref _elementCount);

                _requiresUpdate = true;
            }
        }

        public bool Remove(BlockCoordinates coordinates)
        {
            lock (_writeLock)
            {
                if (BlockIndices.TryRemove(coordinates, out var indexes))
                {
                    //   Interlocked.Add(ref _elementCount, -indexes.Count);
                    _requiresUpdate = true;

                    return true;
                }

                return false;
            }
        }

        public void Apply(GraphicsDevice device = null)
        {
            lock (_writeLock)
            {
                if (!_requiresUpdate)
                    return;

                if (device == null)
                    device = IndexBuffer.GraphicsDevice;

                PooledIndexBuffer oldBuffer = null;
                PooledIndexBuffer buffer    = IndexBuffer;

                try
                {
                    var newIndices = new List<int>(BlockIndices.Sum(x => x.Value.Count));

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
                    
                    buffer.SetData<int>(newIndices.ToArray(), 0, newIndices.Count);
                    _elementCount = newIndices.Count;
                }
                finally
                {
                    IndexBuffer = buffer;
                    oldBuffer?.MarkForDisposal();

                    _requiresUpdate = false;
                }
            }
        }
        
        public virtual int Render(GraphicsDevice device, Effect effect)
        {
            if (IndexBuffer == null || _elementCount == 0) return 0;
            device.Indices = IndexBuffer;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _elementCount / 3);
            }

            return IndexBuffer.IndexCount;
        }

        public void Dispose()
        {
            IndexBuffer?.MarkForDisposal();
            BlockIndices.Clear();
        }
    }
}