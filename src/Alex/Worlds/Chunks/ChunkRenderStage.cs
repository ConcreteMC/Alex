using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Worlds.Chunks
{
	public class ChunkRenderStage : IDisposable
	{
		private const int DefaultSize = 64;
        
		private  SortedSet<int>                                    AvailableIndices { get; }
		private  ConcurrentDictionary<BlockCoordinates, List<int>> BlockIndices     { get; set; }
		private  PooledVertexBuffer                                Buffer           { get; set; }
		internal BlockShaderVertex[]                               Vertices         { get; set; }
        
		private bool HasChanges { get; set; }
		private bool HasResized { get; set; } = false;

		private object _writeLock = new object();
		public ChunkRenderStage()
		{
			BlockIndices = new ConcurrentDictionary<BlockCoordinates, List<int>>();
			AvailableIndices = new SortedSet<int>();// new LinkedList<KeyValuePair<int, int>>();
		}

		private void Init()
		{
			Vertices = new BlockShaderVertex[DefaultSize];
			for(int i = 0; i < Vertices.Length; i++)
				AvailableIndices.Add(i);
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

				BlockShaderVertex[] newVertices = new BlockShaderVertex[(int) (oldSize + 360)];
				Array.Copy(vertices, newVertices, vertices.Length);
				vertices = newVertices;
				//Array.Resize(ref vertices, oldSize * 2);
				int newSize = vertices.Length;

				Vertices = vertices;
				// Interlocked.Add(ref _totalSize, newSize - oldSize);

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
		}

		private void FreeIndex(int index, int count = 1)
		{
			Set(index, BlockShaderVertex.Default);
			AvailableIndices.Add(index);
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
		
		public void AddVertex(BlockCoordinates blockCoordinates, BlockShaderVertex vertex)
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

			HasChanges = true;
		}

		public void Remove(BlockCoordinates coordinates)
		{
			//lock (_writeLock)
			{
				if (BlockIndices.TryRemove(coordinates, out var indices))
				{
					foreach (var index in indices)
					{
						FreeIndex(index);
					}

					// ApplyIntermediate();

					HasChanges = true;
				}
			}
		}

		public void Apply(GraphicsDevice device = null)
		{
			//lock (_writeLock)
			{
				if (!HasChanges)
					return;

				ApplyIntermediate();

				var vertices = Vertices;
            
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

				Buffer = buffer;
				oldBuffer?.MarkForDisposal();
            
				HasResized = false;
				HasChanges = false;

				//  Ready = true;
			}
		}
        
		public virtual int Render(GraphicsDevice device, Effect effect)
		{
			if (Buffer == null) return 0;
            
			device.SetVertexBuffer(Buffer);
			foreach (var pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				device.DrawPrimitives(PrimitiveType.TriangleList, 0, Buffer.VertexCount / 3);
			}

			return Buffer.VertexCount;
		}

		public void Dispose()
		{
			Buffer?.MarkForDisposal();
			BlockIndices.Clear();
		}
	}
}