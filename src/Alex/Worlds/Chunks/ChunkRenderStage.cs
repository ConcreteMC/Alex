using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Graphics.GpuResources;
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
		private ManagedVertexBuffer                             Buffer           { get; set; }
		
		private bool                  HasChanges     { get; set; }

		private long      _vertexCount = 0;

		private object _writeLock = new object();
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
			lock (_writeLock)
			{
				var bi = BlockIndices;

				if (bi == null) return;
				
				var vertexData = new VertexData(
					position, face, new Microsoft.Xna.Framework.Graphics.PackedVector.Short4(textureCoordinates.X, textureCoordinates.Y, textureCoordinates.Z, textureCoordinates.W), color.PackedValue);
				
				Interlocked.Increment(ref _vertexCount);

				var list = bi.GetOrAdd(
					blockCoordinates, coordinates => new List<VertexData>(6 * 6));
				list.Add(vertexData);

				HasChanges = true;
			}
		}

		public void Remove(BlockCoordinates coordinates)
		{
			lock (_writeLock)
			{
				var bi = BlockIndices;

				if (bi == null) return;

				if (bi.TryRemove(coordinates, out var indices))
				{
					Interlocked.Add(ref _vertexCount, -indices.Count);
					HasChanges = true;
				}
			}
		}

		public bool Contains(BlockCoordinates coordinates)
		{
			lock (_writeLock)
			{
				var bi = BlockIndices;

				if (bi == null) return false;

				return bi.ContainsKey(coordinates);
			}
		}

		private const int MaxArraySize = 16 * 16 * 256 * (6 * 6);
		internal MinifiedBlockShaderVertex[] BuildVertices(IBlockAccess world)
		{
			lock (_writeLock)
			{
				var bi = BlockIndices;

				if (bi == null) return null;
				
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
					var v3 = new Vector3(block.Key.X, block.Key.Y, block.Key.Z);
					foreach (var vertex in block.Value)
					{
						var p = v3 + vertex.Position;
						//var offset = vertex.Face.GetVector3();
						
						//BlockModel.GetLight(
						//	world, new BlockCoordinates(v3) + vertex.Face.GetBlockCoordinates(), out byte blockLight, out byte skyLight, false);
						
						world.GetLight(new BlockCoordinates(v3) + vertex.Face.GetBlockCoordinates(), out var blockLight, out var skyLight);
						
						vertices[index] = new MinifiedBlockShaderVertex(
							p, vertex.Face, vertex.TexCoords.ToVector4(), new Color(vertex.Color),
							blockLight, skyLight);
						
						index++;
					}
				}
				
				return vertices;
			}
		}

		private bool _previousKeepInMemory     = false;
		private int  _primitiveCount = 0;

		private object _applyLock = new object();
		public void Apply(IBlockAccess world,
			GraphicsDevice device = null,
			bool keepInMemory = true,
			bool force = false)
		{
			MinifiedBlockShaderVertex[] realVertices;

			lock (_writeLock)
			{
				if (!HasChanges && !force)
					return;

				_previousKeepInMemory = keepInMemory;

				realVertices = BuildVertices(world);

				if (realVertices == null)
					return;
				
				HasChanges = false;
			
				var size = realVertices.Length;

				try
				{
					ManagedVertexBuffer buffer = Buffer;

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

					ManagedVertexBuffer oldBuffer = null;

					if (buffer == null || buffer.VertexCount < size)
					{
						oldBuffer = buffer;

						buffer = GpuResourceManager.GetBuffer(
							this, device, MinifiedBlockShaderVertex.VertexDeclaration, size, BufferUsage.WriteOnly);
					}

					buffer.SetData(realVertices, 0, size);

					Buffer = buffer;

					if (oldBuffer != buffer)
						oldBuffer?.ReturnResource(this);
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
			lock (_writeLock)
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

					Buffer?.ReturnResource(this);
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
}