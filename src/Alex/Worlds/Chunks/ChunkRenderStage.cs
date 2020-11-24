using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.API.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Worlds.Chunks
{
	public struct VertexData
	{
		public Vector3 Position;

		public ushort TexCoords;

		public uint Color;

		public byte BlockLight;

		public byte SkyLight;

		public VertexData(Vector3 position, ushort textureCoordinates, uint color, byte blockLight, byte skyLight)
		{
			Position = position;
			TexCoords = textureCoordinates;
			Color = color;
			BlockLight = blockLight;
			SkyLight = skyLight;
		}
	}
	public class ChunkRenderStage : IDisposable
	{
		private static ILogger Log         = LogManager.GetCurrentClassLogger();
		
		private const  int     DefaultSize = 64;

		private static SmartStorage<Vector2>                      TextureStorage { get; } = new SmartStorage<Vector2>();
		//private static SmartStorage<Color>                        ColorStorage { get; } = new SmartStorage<Color>();
		
		//private List<(ushort, ushort)>                         AvailableIndices { get; }
		private Dictionary<BlockCoordinates, List<VertexData>> BlockIndices     { get; set; }
		private PooledVertexBuffer                             Buffer           { get; set; }
		//private VertexData?[]                                  Vertices         { get; set; }

		//private List<Vector3> Positions          { get; set; } = new List<Vector3>();

		private bool                  HasChanges     { get; set; }
		private bool                  HasResized     { get; set; } = false;

		private ChunkData Parent { get; }
		private object    _writeLock   = new object();
		private long      _vertexCount = 0;
		public ChunkRenderStage(ChunkData parent)
		{
			Parent = parent;
			
			//TextureStorage = new SmartStorage<Vector2>();
			BlockIndices = new Dictionary<BlockCoordinates, List<VertexData>>();
			//AvailableIndices = new List<(ushort, ushort)>();// new LinkedList<KeyValuePair<int, int>>();
		}

		private void Init()
		{
			//Vertices = new VertexData?[DefaultSize];
			//TextureCoordinates = new (Vector2, ushort)[0];
			//AvailableIndices.Add((0, (ushort) (Vertices.Length - 1)));
			//for(ushort i = 0; i < Vertices.Length; i++)
			//	AvailableIndices.Add(i);
		}

		/*private ushort GetIndex()
		{
			do
			{
				if (AvailableIndices.Count > 0)
				{
					var availableIndex = AvailableIndices[0];

					ushort result = availableIndex.Item2;
					
					if (availableIndex.Item1 == availableIndex.Item2)
						AvailableIndices.Remove(availableIndex);
					else
					{
						result = availableIndex.Item2;
						availableIndex.Item2 -= 1;
						AvailableIndices[0] = availableIndex;
					}
					//AvailableIndices.Remove(availableIndex);

					return result;
				}

				var vertices = Vertices;

				ushort oldSize = (ushort) vertices.Length;

				//MinifiedBlockShaderVertex[] newVertices = new MinifiedBlockShaderVertex[(int) (oldSize + 36)];
				//Array.Copy(vertices, newVertices, vertices.Length);
				Array.Resize(ref vertices, oldSize + 6);
				//vertices = newVertices;
				
				int newSize = vertices.Length;

				Vertices = vertices;
				// Interlocked.Add(ref _totalSize, newSize - oldSize);

				for (ushort i = oldSize; i < newSize; i++)
					FreeIndex(i, -1);
					 //AvailableIndices.Add(i);

				HasResized = true;
				// return ;
			} while (true);
		}
		
		private void FreeIndex(ushort index, int count = 1)
		{
			//var oldValue = Vertices[index];
			//if (oldValue != null)
			
			Set(index, null);

			if (AvailableIndices.Count > 0)
			{
				for (int i = 0; i < AvailableIndices.Count; i++)
				{
					var ai = AvailableIndices[i];
					if (index == ai.Item2 + 1)
					{
						ai.Item2 = index;
						AvailableIndices[i] = ai;

						return;
					}
				}
				
				for (int i = 0; i < AvailableIndices.Count; i++)
				{
					var ai = AvailableIndices[i];
					if (index == ai.Item1 - 1)
					{
						ai.Item1 = index;
						AvailableIndices[i] = ai;

						return;
					}
				}
			}
			else
			{
				AvailableIndices.Add((index, index));
			}
		}*/

		private void Add(BlockCoordinates block, 
			Vector3? position = null,
			Vector2? textureCoordinates = null,
			Color? color = null,
			byte? blockLight = null,
			byte? skyLight = null)
		{
			var textureIndex = TextureStorage.GetIndex(textureCoordinates.Value);

			if (textureIndex == -1)
			{
				textureIndex = TextureStorage.Add(textureCoordinates.Value);
			}

			TextureStorage.IncreaseUsage(textureIndex);

			var vertexData = new VertexData(
				position.Value, (ushort) textureIndex, color.Value.PackedValue, (byte) blockLight.Value,
				skyLight.Value);
				
			Interlocked.Increment(ref _vertexCount);
			
			if (BlockIndices.TryGetValue(block, out var list))
			{
				list.Add(vertexData);
			}
			else
			{
				BlockIndices.Add(block, new List<VertexData>()
				{
					vertexData
				});
			}
			
			/*var oldValue     = Vertices[index];

			if (oldValue.HasValue)
			{
				TextureStorage.DecrementUsage(oldValue.Value.TexCoords);
				Interlocked.Decrement(ref _vertexCount);
			}

			if (position.HasValue)
			{
				var textureIndex = TextureStorage.GetIndex(textureCoordinates.Value);

				if (textureIndex == -1)
				{
					textureIndex = TextureStorage.Add(textureCoordinates.Value);
				}

				TextureStorage.IncreaseUsage(textureIndex);

				Vertices[index] = new VertexData(
					position.Value, (ushort) textureIndex, color.Value.PackedValue, (byte) blockLight.Value,
					skyLight.Value);
				
				Interlocked.Increment(ref _vertexCount);
			}
			else
			{
				Vertices[index] = null;
			}*/
		}
		
		public void AddVertex(BlockCoordinates blockCoordinates, 
			Vector3 position,
			Vector2 textureCoordinates,
			Color color,
			byte blockLight,
			byte skyLight)
		{
			lock (_writeLock)
			{
				/*if (Vertices == null)
					Init();

				ushort index = GetIndex();
*/
				Add(blockCoordinates, position, textureCoordinates, color, blockLight, skyLight);

				/*if (BlockIndices.TryGetValue(blockCoordinates, out var list))
				{
					list.Add(index);
				}
				else
				{
					BlockIndices.Add(blockCoordinates, new List<ushort>()
					{
						index
					});
				}*/
				/*BlockIndices.AddOrUpdate(
					blockCoordinates, coordinates => new List<ushort>() {index}, (coordinates, ints) =>
					{
						ints.Add(index);

						return ints;
					});*/

				HasChanges = true;
			}
		}

		public void Remove(BlockCoordinates coordinates)
		{
			lock (_writeLock)
			{
				if (BlockIndices.Remove(coordinates, out var indices))
				{
					foreach (var vertex in indices)
					{
						TextureStorage.DecrementUsage(vertex.TexCoords);
						Interlocked.Decrement(ref _vertexCount);
						//FreeIndex(index);
					}

					// ApplyIntermediate();

					HasChanges = true;
				}
			}
		}

		public bool Contains(BlockCoordinates coordinates)
		{
			lock (_writeLock)
			{
				return BlockIndices.ContainsKey(coordinates);
			}
		}

		internal MinifiedBlockShaderVertex[] BuildVertices()
		{
			var realVertices = new List<MinifiedBlockShaderVertex>((int) _vertexCount);

			foreach (var block in BlockIndices)
			{
				foreach (var vertex in block.Value)
				{
					/*var v = Vertices[index];
					if (!v.HasValue)
						continue;*/

					//var vertex = v.Value;
				
					realVertices.Add(new MinifiedBlockShaderVertex(
						vertex.Position, TextureStorage[vertex.TexCoords], new Color(vertex.Color))
					{
						SkyLight = vertex.SkyLight,
						BlockLight = vertex.BlockLight
					});
				}
			}

			return realVertices.ToArray();
		}

		private int NextPowerOf2(int x)
		{
			double nextnum = Math.Ceiling(Math.Log2(x));
			var    result  = Math.Pow(2, nextnum);

			return (int) result;
		}

		private bool _previousKeepInMemory = false;
		public void Apply(GraphicsDevice device = null, bool keepInMemory = true)
		{
			lock (_writeLock)
			{
				if (!HasChanges && _previousKeepInMemory)
					return;

				_previousKeepInMemory = keepInMemory;

				/*var vertices = Vertices;

				var blockIndices = BlockIndices.ToArray();
			
				List<VertexData?>            v       = new List<VertexData?>((int) Interlocked.Read(ref _vertexCount));
				foreach (var block in blockIndices)
				{
					var          startIndex = v.Count;

					for (var i = 0; i < block.Value.Count; i++)
					{
						var index = block.Value[i];
						v.Add(vertices[index]);

						block.Value[i] = ((ushort) (startIndex + i));
					}

					BlockIndices[block.Key] = block.Value;
				}

				vertices = v.ToArray();
				//BlockIndices = indices;
				Vertices = vertices;*/

				var realVertices = BuildVertices();
				
				//AvailableIndices.Clear();
				
				/*if (realVertices.Length < Vertices.Length)
				{
					var difference = Vertices.Length - realVertices.Length;

					if (difference >= 256)
					{
						Log.Warn(
							$"Vertices array is to big! Required={realVertices.Length} Size={Vertices.Length} Difference={difference})");
					}
				}*/
            
				bool               callSetData = HasResized;
				PooledVertexBuffer oldBuffer   = null;
				PooledVertexBuffer buffer      = Buffer;

				if (buffer != null && buffer.VertexCount - realVertices.Length >= 256)
				{
					if (GpuResourceManager.TryGetRecycledBuffer(
						this, device, MinifiedBlockShaderVertex.VertexDeclaration, realVertices.Length,
						BufferUsage.WriteOnly, out var b))
					{
						oldBuffer = buffer;
						buffer = b;

						callSetData = true;
					}
				}
				
				if (buffer == null || buffer.VertexCount < realVertices.Length)
				{
					oldBuffer = buffer;

					buffer = GpuResourceManager.GetBuffer(
						this, device, MinifiedBlockShaderVertex.VertexDeclaration, realVertices.Length, BufferUsage.WriteOnly);

					callSetData = true;
				}

				//  if (callSetData)
				buffer.SetData(realVertices, 0, realVertices.Length);

				Buffer = buffer;
				oldBuffer?.MarkForDisposal();
            
				HasResized = false;
				HasChanges = false;

				//  Ready = true;

				if (!keepInMemory)
				{
					//AvailableIndices.Clear();
					//BlockIndices.Clear();
					//Vertices = new VertexData?[DefaultSize];
				}
			}
		}
        
		public virtual int Render(GraphicsDevice device, Effect effect)
		{
			if (Buffer == null) return 0;
            
			device.SetVertexBuffer(Buffer);
			foreach (var pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				device.DrawPrimitives(PrimitiveType.TriangleList, 0, (int) _vertexCount / 3);
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