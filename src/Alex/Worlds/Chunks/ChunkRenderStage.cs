using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.Common;
using Alex.Common.Blocks;
using Alex.Common.Graphics;
using Alex.Common.Graphics.GpuResources;
using Alex.Common.Utils.Vectors;
using Alex.Graphics.Models.Blocks;
using Alex.Utils.Threading;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using NLog;

namespace Alex.Worlds.Chunks
{
	public class ChunkRenderStage : IDisposable
	{
		private readonly RenderStage _stage;
		private static ILogger Log         = LogManager.GetCurrentClassLogger();
		private Dictionary<BlockCoordinates, List<VertexData>> BlockIndices     { get; set; }
		private VertexBuffer                             Buffer           { get; set; }

		private bool                  HasChanges     { get; set; }

		private long      _vertexCount = 0;
		private int  _primitiveCount = 0;
		
		public bool IsEmpty => BlockIndices == null || BlockIndices.Count == 0;

		private object _writeLock = new object();
		public ChunkRenderStage(RenderStage stage)
		{
			_stage = stage;
			BlockIndices = new Dictionary<BlockCoordinates, List<VertexData>>();
		}
		
		public void AddVertex(BlockCoordinates blockCoordinates, 
			Vector3 position,
			BlockFace face,
			Vector4 textureCoordinates,
			Color color,
			VertexFlags flags = VertexFlags.Default)
		{
			lock (_writeLock)
			{
				var bi = BlockIndices;

				if (bi == null) return;
				
				var vertexData = new VertexData(
					position,
					face, 
					textureCoordinates, 
					color.PackedValue,
					flags
				);
				
				Interlocked.Increment(ref _vertexCount);

				List<VertexData> list;
				if (!bi.TryGetValue(blockCoordinates, out list))
				{
					list = new List<VertexData>();
					bi.Add(blockCoordinates, list);
				}
				//var list = bi.GetOrAdd(
				//	blockCoordinates, coordinates => new List<VertexData>(6 * 6));
				list.Add(vertexData);

				HasChanges = true;
			}
		}
		
		public void Remove(BlockCoordinates coordinates)
		{
			//lock (_writeLock)
			{
				var bi = BlockIndices;

				if (bi == null) return;

				if (bi.TryGetValue(coordinates, out var indices))
				{
					indices.Clear();
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
		internal IEnumerable<MinifiedBlockShaderVertex> BuildVertices()
		{
			lock (_writeLock)
			{
				var blockIndices = BlockIndices;

				if (blockIndices == null) yield break;
				
				foreach (var block in blockIndices)
				{
					var v3 = new Vector3(block.Key.X, block.Key.Y, block.Key.Z);
					foreach (var vertex in block.Value)
					{
						var p = v3 + vertex.Position;

						var textureCoords = vertex.TexCoords;
						yield return new MinifiedBlockShaderVertex(
							p, vertex.IsSolid ? vertex.Face : BlockFace.None,textureCoords.ToVector4(), new Color(vertex.Color),
							0, 0);
					}
				}
			}
		}

		private ManagedTask _previousManagedTask = null;

		public void Apply(IBlockAccess world, bool force = false)
		{
			MinifiedBlockShaderVertex[] realVertices;
			var previousTask = _previousManagedTask;


			if (!HasChanges && !force)
				return;

			HasChanges = false;

			realVertices = BuildVertices().ToArray();

			for (int i = 0; i < realVertices.Length; i++)
			{
				var vertex = realVertices[i];
				var lightProbe = vertex.Position;

				BlockFace face = (BlockFace) vertex.Normal;
				if (face != BlockFace.None)
				{
					lightProbe += face.GetVector3();
				}
						
				world.GetLight(lightProbe, out var blockLight, out var skyLight);
				realVertices[i].Lighting = new Short2(skyLight, blockLight);
			}

			if (previousTask != null && previousTask.State == TaskState.Enqueued)
			{
				previousTask.Data = realVertices;
			}
			else
			{
				previousTask?.Cancel();
				_previousManagedTask = Alex.Instance.UiTaskManager.Enqueue(UpdateAction, realVertices, TaskSetup);
			}
		}

		private void TaskSetup(ManagedTask task)
		{
			task.Tag = $"RenderStage Update: {_stage}";
		}

		private void UpdateAction(ManagedTask task, object state)
		{
			var realVertices = (MinifiedBlockShaderVertex[]) state;
			var size = realVertices.Length;

			try
			{
				VertexBuffer buffer = Buffer;

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

				VertexBuffer oldBuffer = null;

				if (buffer == null || buffer.VertexCount < size)
				{
					oldBuffer = buffer;

				//	buffer = GpuResourceManager.GetBuffer(this, Alex.Instance.GraphicsDevice, MinifiedBlockShaderVertex.VertexDeclaration, size, BufferUsage.WriteOnly);
					buffer = new VertexBuffer(
						Alex.Instance.GraphicsDevice, MinifiedBlockShaderVertex.VertexDeclaration, size,
						BufferUsage.WriteOnly);
				}

				buffer.SetData(realVertices, 0, size);

				Buffer = buffer;

				oldBuffer?.Dispose();
				
				//if (oldBuffer != null && oldBuffer.PoolId != buffer.PoolId) oldBuffer?.ReturnResource(this);
			}
			finally
			{
				//	Pool.Return(realVertices, true);
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
			//lock (_writeLock)
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

					Buffer?.Dispose();
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