using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Alex.Common;
using Alex.Common.Blocks;
using Alex.Common.Graphics;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Alex.Worlds.Chunks
{
    public class ChunkData : IDisposable
    {
        private        ChunkRenderStage[] _stages;
        private static long               _instances = 0;

        private int _x, _z;
        public ChunkData(int x, int y)
        {
            _x = x;
            _z = y;
            
            var availableStages = Enum.GetValues(typeof(RenderStage));
            _stages = new ChunkRenderStage[availableStages.Length];
            
            Interlocked.Increment(ref _instances);
        }

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
            if (Disposed)
            {
                return 0;
            }

            var rStage = _stages[(int) stage];

            if (rStage == null)
            {
                return 0;
            }

            IEffectMatrices em = (IEffectMatrices) effect;
            var originalWorld = em.World;
            try
            {
                em.World = Matrix.CreateTranslation(_x << 4, 0f, _z << 4);
                return rStage.Render(device, effect);
            }
            finally
            {
                em.World = originalWorld;
            }
        }

        public MinifiedBlockShaderVertex[] BuildVertices()
        {
            return _stages.Where(x => x != null).SelectMany(x => x.BuildVertices()).ToArray();
        }

        public void AddVertex(BlockCoordinates blockCoordinates,
            Vector3 position,
            BlockFace face,
            Vector4 textureCoordinates,
            Color color,
            RenderStage stage,
            VertexFlags flags = VertexFlags.Default)
        {
            var stages = _stages;

            if (stages == null) return;
            
            var rStage = stages[(int) stage];

            if (rStage == null)
            {
                rStage = CreateRenderStage(stage);
                stages[(int) stage] = rStage;
            }
           
            rStage.AddVertex(blockCoordinates, position, face, textureCoordinates, color, flags);
        }

        private ChunkRenderStage CreateRenderStage(RenderStage arg)
        {
            return new ChunkRenderStage(arg);
        }

        public void Remove(BlockCoordinates blockCoordinates)
        {
            var stages = _stages;

            if (stages == null) return;
            
            for (var index = 0; index < stages.Length; index++)
            {
                var stage = stages[index];

                if (stage != null)
                {
                    stage.Remove(blockCoordinates);

                    if (stage.IsEmpty)
                    {
                        stage.Dispose();
                        stages[index] = null;
                    }
                }
            }
        }

        public bool Contains(BlockCoordinates coordinates)
        {
            return _stages.Where(x => x != null).Any(x => x.Contains(coordinates));
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

                for (var index = 0; index < stages.Length; index++)
                {
                    var stage = stages[index];
                    stage?.Apply(world, forceUpdate);
                }
            }
            finally
            {
                MovingAverage.ComputeAverage((float) sw.Elapsed.TotalMilliseconds);
            }
        }

        public bool                Disposed { get; private set; } = false;
        public void Dispose()
        {
            if (Disposed)
                return;
            
           // lock (WriteLock)
           {
               try
               {
                   for (var index = 0; index < _stages.Length; index++)
                   {
                       var stage = _stages[index];
                       stage?.Dispose();
                       _stages[index] = null;
                   }
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
}