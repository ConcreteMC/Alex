using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.API;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.Utils.Vectors;
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

        public MinifiedBlockShaderVertex[] BuildVertices(IBlockAccess blockAccess)
        {
            return _stages.Where(x => x != null).SelectMany(x => x.BuildVertices(blockAccess)).ToArray();
        }

        public void AddVertex(BlockCoordinates blockCoordinates,
            Vector3 position,
            BlockFace face,
            Vector4 textureCoordinates,
            Color color,
            RenderStage stage)
        {
            var rStage = _stages[(int) stage];

            if (rStage == null)
            {
                rStage = CreateRenderStage(stage);
                _stages[(int) stage] = rStage;
            }
           
            rStage.AddVertex(blockCoordinates, position, face, textureCoordinates, color);
        }

        private ChunkRenderStage CreateRenderStage(RenderStage arg)
        {
            return new ChunkRenderStage();
        }

        public void Remove(BlockCoordinates blockCoordinates)
        {
            for (var index = 0; index < _stages.Length; index++)
            {
                var stage = _stages[index];

                stage?.Remove(blockCoordinates);
            }
        }

        public bool Contains(BlockCoordinates coordinates)
        {
            return _stages.Where(x => x != null).Any(x => x.Contains(coordinates));
        }

        public void ApplyChanges(IBlockAccess world, GraphicsDevice device, bool keepInMemory, bool forceUpdate = false)
        {
            for (var index = 0; index < _stages.Length; index++)
            {
                var stage = _stages[index];
                stage?.Apply(world, device, keepInMemory, forceUpdate);
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