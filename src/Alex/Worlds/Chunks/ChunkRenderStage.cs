using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.Utils.Collections;
using Alex.API.Utils.Vectors;
using Alex.Graphics.Models.Blocks;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using NLog;

namespace Alex.Worlds.Chunks
{
	public class ChunkRenderStage : IDisposable
	{
		private static ILogger Log         = LogManager.GetCurrentClassLogger();
		private ConcurrentDictionary<BlockCoordinates, List<VertexData>> BlockIndices     { get; set; }
		private PooledVertexBuffer                             Buffer           { get; set; }
		
		private bool                  HasChanges     { get; set; }

		private long      _vertexCount = 0;
		
		public ChunkRenderStage()
		{
			BlockIndices = new ConcurrentDictionary<BlockCoordinates, List<VertexData>>();
		}
		
		public void AddVertex(BlockCoordinates blockCoordinates, 
			Vector3 position,
			BlockFace face,
			Vector4 textureCoordinates,
			Color color)
		{
			//lock (_writeLock)
			{
				var vertexData = new VertexData(
					position, face, textureCoordinates, color.PackedValue);
				
				Interlocked.Increment(ref _vertexCount);

				var list = BlockIndices.GetOrAdd(
					blockCoordinates, coordinates => new List<VertexData>());
				list.Add(vertexData);

				HasChanges = true;
			}
		}

		public void Remove(BlockCoordinates coordinates)
		{
		//	lock (_writeLock)
			{
				if (BlockIndices.TryRemove(coordinates, out var indices))
				{
					Interlocked.Add(ref _vertexCount, -indices.Count);
					HasChanges = true;
				}
			}
		}

		public bool Contains(BlockCoordinates coordinates)
		{
		//	lock (_writeLock)
			{
				return BlockIndices.ContainsKey(coordinates);
			}
		}

		private const int MaxArraySize = 16 * 16 * 256 * (6 * 6);
		internal MinifiedBlockShaderVertex[] BuildVertices(IBlockAccess world)
		{
			//lock (_writeLock)
			{
				var blockIndices = BlockIndices;
				var size = blockIndices.Sum(x => x.Value.Count);
				//length = size;

				if (size > MaxArraySize)
				{
					Log.Warn($"Array size exceeded max pool size. Found {size}, limit: {MaxArraySize}");
				}
				//var vertices = Pool.Rent(size);
				var vertices = new MinifiedBlockShaderVertex[size];

				int index = 0;
				foreach (var block in blockIndices)
				{
					foreach (var vertex in block.Value)
					{
						var p = vertex.Position;
						var offset = vertex.Face.GetVector3();
						
						BlockModel.GetLight(
							world, p, out byte blockLight, out byte skyLight, true);
						
						vertices[index] = new MinifiedBlockShaderVertex(
							vertex.Position, offset, vertex.TexCoords, new Color(vertex.Color),
							blockLight, skyLight);
						
						index++;
					}
				}
				
				return vertices;
			}
		}

		private bool _previousKeepInMemory     = false;
		private int  _primitiveCount = 0;
		public void Apply(IBlockAccess world, GraphicsDevice device = null, bool keepInMemory = true, bool force = false)
		{
			//lock (_writeLock)
			{
				if (!HasChanges && !force)
					return;

				_previousKeepInMemory = keepInMemory;

				var realVertices = BuildVertices(world);
				HasChanges = false;
				
				var size = realVertices.Length;

				try
				{
					if (realVertices.Length == 0)
					{
						_primitiveCount = 0;

						return;
					}

					var verticeCount = realVertices.Length;

					while (verticeCount % 3 != 0) //Make sure we have a valid triangle list.
					{
						verticeCount--;
					}

					_primitiveCount = verticeCount / 3;

					PooledVertexBuffer oldBuffer = null;
					PooledVertexBuffer buffer = Buffer;

					if (buffer == null || buffer.VertexCount < size)
					{
						oldBuffer = buffer;

						buffer = GpuResourceManager.GetBuffer(
							this, device, MinifiedBlockShaderVertex.VertexDeclaration, size,
							BufferUsage.WriteOnly);
					}

					buffer.SetData(realVertices, 0, size);

					Buffer = buffer;
					
					if (oldBuffer != buffer)
						oldBuffer?.MarkForDisposal();
				}
				finally
				{
				//	Pool.Return(realVertices, true);
				}
			}
		}
        
		public virtual int Render(GraphicsDevice device, Effect effect)
		{
			var primitives = _primitiveCount;

			if (Buffer == null || primitives == 0) return 0;

			int count = 0;
			device.SetVertexBuffer(Buffer);
			foreach (var pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				device.DrawPrimitives(PrimitiveType.TriangleList, 0, primitives);

				count++;
			}

			return count;
		}

		private bool _disposed = false;
		public void Dispose()
		{
			if (_disposed)
				return;

			try
			{
				var keys = BlockIndices.Keys.ToArray();

				foreach (var key in keys)
				{
					Remove(key);
				}

				BlockIndices.Clear();

				Buffer?.MarkForDisposal();
				Buffer = null;
				BlockIndices = null;
			}
			finally
			{
				_disposed = true;
			}
		}
	}
}