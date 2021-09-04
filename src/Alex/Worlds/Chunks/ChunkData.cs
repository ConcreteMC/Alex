using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Alex.Common;
using Alex.Common.Blocks;
using Alex.Common.Graphics;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Utils.Threading;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Alex.Worlds.Chunks
{
    public record StageData(DynamicIndexBuffer Buffer, List<int> Indexes, int IndexCount);
    
    public class ChunkData : IDisposable
    {
        private        StageData[] _stages;
        private static long               _instances = 0;
        
        private int _x, _z;
        public ChunkData(int x, int y)
        {
            _x = x;
            _z = y;

            var availableStages = Enum.GetValues(typeof(RenderStage));
            _stages = new StageData[availableStages.Length];
            
            Interlocked.Increment(ref _instances);
        }

        private DynamicVertexBuffer Buffer { get; set; }

        private bool _rendered = false;
        public bool Rendered
        {
            get
            {
                return _rendered;
            }
            set
            {
                _rendered = value;
            }
        }

        public int Draw(GraphicsDevice device, RenderStage stage, Effect effect)
        {
            if (Disposed || !_rendered)
            {
                return 0;
            }

            var rStage = _stages[(int) stage];

            if (rStage == null || rStage.Indexes == null || rStage.Buffer == null)
            {
                return 0;
            }

            if (Buffer == null) return 0;

            IEffectMatrices em = (IEffectMatrices) effect;
            var originalWorld = em.World;
            try
            {
              //  device.SetVertexBuffers(new VertexBufferBinding(Buffer, 0), new VertexBufferBinding(Lighting, 0, 1));
                device.SetVertexBuffer(Buffer);
                device.Indices = rStage.Buffer;
                
                em.World = Matrix.CreateTranslation(_x << 4, 0f, _z << 4);
                
                int count = 0;
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, rStage.IndexCount);
                    count++;
                }

                return count;
            }
            finally
            {
                em.World = originalWorld;
            }
        }
        
        public MinifiedBlockShaderVertex[] BuildVertices()
        {
            BlockCoordinates[] blockCoordinates;
            Vector3[] positions;
            Vector4[] textureCoordinates;
            BlockFace[] faces;
            Color[] colors;
            RenderStage[] renderStages;
            lock (_dataLock)
            {
                blockCoordinates = _blockCoordinates.ToArray();
                positions = _positions.ToArray();
                textureCoordinates = _textureCoordinates.ToArray();
                faces = _faces.ToArray();
                colors = _colors.ToArray();
                renderStages = _vertexStages.ToArray();
            }
            
            List<MinifiedBlockShaderVertex> vertices = new List<MinifiedBlockShaderVertex>();
            for (int i = 0; i < positions.Length; i++)
            {
                var blockPos = blockCoordinates[i];
                var stage = renderStages[i];
                var position = positions[i];
                var textureCoordinate = textureCoordinates[i];
                var face = faces[i];
                var color = colors[i];
                var modified = new Vector3(blockPos.X, blockPos.Y, blockPos.Z);
                
                vertices.Add(
                    new MinifiedBlockShaderVertex(
                        modified + position, face, textureCoordinate, color,
                        0, 0));
            }

            return vertices.ToArray();
        }

        private Collection<BlockCoordinates> _blockCoordinates = new Collection<BlockCoordinates>();
        private Collection<Vector3> _positions = new Collection<Vector3>();
        private Collection<BlockFace> _faces = new Collection<BlockFace>();
        private Collection<Vector4> _textureCoordinates = new Collection<Vector4>();
        private Collection<Color> _colors = new Collection<Color>();
        private Collection<RenderStage> _vertexStages = new Collection<RenderStage>();

        private object _dataLock = new object();
        public void AddVertex(BlockCoordinates blockCoordinates,
            Vector3 position,
            BlockFace face,
            Vector4 textureCoordinates,
            Color color,
            RenderStage stage,
            VertexFlags flags = VertexFlags.Default)
        {
            lock (_dataLock)
            {
                _blockCoordinates.Add(blockCoordinates);
                _positions.Add(position);
                _faces.Add(face);
                _textureCoordinates.Add(textureCoordinates);
                _colors.Add(color);
                _vertexStages.Add(stage);
            }
        }

        public void Remove(BlockCoordinates blockCoordinates)
        {
            lock (_dataLock)
            {
                int index;

                do
                {
                    index = _blockCoordinates.IndexOf(blockCoordinates);
                    if (index == -1)
                        return;
                    
                    _blockCoordinates.RemoveAt(index);
                    _positions.RemoveAt(index);
                    _faces.RemoveAt(index);
                    _textureCoordinates.RemoveAt(index);
                    _colors.RemoveAt(index);
                  //  _lighting.RemoveAt(index);
                    _vertexStages.RemoveAt(index);
                    
                } while (index >= 0);
            }
        }

        public static float AverageUploadTime => MovingAverage.Average;
        public static float MaxUploadTime => MovingAverage.Maximum;
        public static float MinUploadTime => MovingAverage.Minimum;
        
        private static readonly MovingAverage MovingAverage = new MovingAverage();

        public void ApplyChanges(IBlockAccess world, bool forceUpdate = false)
        {
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                var stages = _stages;

                if (stages == null)
                    return;

                BlockCoordinates[] blockCoordinates;
                Vector3[] positions;
                Vector4[] textureCoordinates;
                BlockFace[] faces;
                Color[] colors;
                RenderStage[] renderStages;
                lock (_dataLock)
                {
                    blockCoordinates = _blockCoordinates.ToArray();
                    _blockCoordinates.Clear();
                    
                    positions = _positions.ToArray();
                    _positions.Clear();
                    
                    textureCoordinates = _textureCoordinates.ToArray();
                    _textureCoordinates.Clear();
                    
                    faces = _faces.ToArray();
                    _faces.Clear();
                    
                    colors = _colors.ToArray();
                    _colors.Clear();
                    
                    renderStages = _vertexStages.ToArray();
                    _vertexStages.Clear();
                }

                StageData[] newStages = new StageData[stages.Length];

                for (int i = 0; i < stages.Length; i++)
                {
                    newStages[i] = new StageData(stages[i]?.Buffer, new List<int>(), 0);
                }

                List<MinifiedBlockShaderVertex> vertices = new List<MinifiedBlockShaderVertex>();

                for (int i = 0; i < positions.Length; i++)
                {
                    var blockPos = blockCoordinates[i];
                    var stage = renderStages[i];
                    var position = positions[i];
                    var textureCoordinate = textureCoordinates[i];
                    var face = faces[i];
                    var color = colors[i];

                    var modified = new Vector3(blockPos.X, blockPos.Y, blockPos.Z);
                    
                    var rStage = newStages[(int)stage];
                    
                    var index = vertices.Count;
                    rStage.Indexes.Add(index);
                    //vertex.Index = indexPosition;

                    Vector3 lightProbe = modified;
                    lightProbe += (face.GetVector3());

                    byte blockLight = 0;
                    byte skyLight = 0;

                    if (world != null)
                    {
                        world.GetLight(lightProbe, out blockLight, out skyLight);
                    }
                    
                    vertices.Add(
                        new MinifiedBlockShaderVertex(
                            modified + position, face, textureCoordinate, color, blockLight, skyLight));

                    newStages[(int)stage] = rStage;
                }
                
                var realVertices = vertices.ToArray();
                DynamicVertexBuffer buffer = Buffer;
                
                if (realVertices.Length == 0)
                {
                    return;
                }

                for (var index = 0; index < newStages.Length; index++)
                {
                    var stage = newStages[index];

                    if (stage == null || stage.Indexes.Count == 0)
                        continue;

                    var indexBuffer = stage.Buffer;

                    DynamicIndexBuffer oldIndexBuffer = null;

                    var indexCount = stage.Indexes.Count;
                    if (indexBuffer == null || indexBuffer.IndexCount < indexCount)
                    {
                        oldIndexBuffer = indexBuffer;

                        indexBuffer = new DynamicIndexBuffer(
                            Alex.Instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indexCount * 2,
                            BufferUsage.WriteOnly);
                    }

                    indexBuffer.SetData(stage.Indexes.ToArray(), 0, indexCount, SetDataOptions.None);
                    oldIndexBuffer?.Dispose();

                    _stages[index] = new StageData(indexBuffer, stage.Indexes, indexCount);
                }

              var verticeCount = realVertices.Length;

                while (verticeCount % 3 != 0) //Make sure we have a valid triangle list.
                {
                    verticeCount--;
                }

                DynamicVertexBuffer oldBuffer = null;
                if (buffer == null || buffer.VertexCount < verticeCount)
                {
                    oldBuffer = buffer;

                    buffer = new DynamicVertexBuffer(
                        Alex.Instance.GraphicsDevice, MinifiedBlockShaderVertex.VertexDeclaration, verticeCount,
                        BufferUsage.WriteOnly);
                    
                    Interlocked.Increment(ref BufferCreations);
                }

                buffer.SetData(realVertices, 0, realVertices.Length, SetDataOptions.None);
                Buffer = buffer;
                oldBuffer?.Dispose();
                Interlocked.Increment(ref BufferUploads);
            }
            finally
            {
                MovingAverage.ComputeAverage((float)sw.Elapsed.TotalMilliseconds);
            }
        }

        public static int BufferUploads = 0;
        public static int BufferCreations = 0;
        private void UpdateAction(object state)
        {
            var data = (Tuple<MinifiedBlockShaderVertex[], LightingVertex[]>)state;
            var realVertices = data.Item1;
            var size = realVertices.Length;
            
           
        }

        public bool Disposed { get; private set; } = false;

        public void Dispose()
        {
            if (Disposed)
                return;

            try
            {
                for (var index = 0; index < _stages.Length; index++)
                {
                    var stage = _stages[index];
                    stage?.Buffer?.Dispose();
                   // stage?.Dispose();
                    _stages[index] = null;
                }

                Buffer?.Dispose();
                Buffer = null;
                
             //   Lighting?.Dispose();
             //   Lighting = null;
            }
            finally
            {
                _stages = null;

                //  Disposed = true;
                Interlocked.Decrement(ref _instances);
                Disposed = true;
            }
        }
    }
}