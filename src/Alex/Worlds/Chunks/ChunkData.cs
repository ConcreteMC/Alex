using System;
using System.Buffers;
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
using MiNET.Blocks;
using NLog;

namespace Alex.Worlds.Chunks
{
	public class StageData
	{
		public IndexBuffer Buffer { get; }
		public List<int> Indexes { get; }
		public int IndexCount { get; }

		public StageData(IndexBuffer buffer, List<int> indexes, int indexCount)
		{
			Buffer = buffer;
			Indexes = indexes;
			IndexCount = indexCount;
		}
	}

	public class ChunkData : IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChunkData));

		public static ChunkData Create(int x, int y)
		{
			return new ChunkData(x, y);
		}

		private StageData[] _stages;

		private StageData[] Stages
		{
			get
			{
				return _stages;
			}
			set
			{
				if (Disposed)
				{
					if (value != null)
					{
						foreach (var s in value)
						{
							s?.Buffer?.Dispose();
							s?.Indexes?.Clear();
						}
					}

					_stages = null;

					return;
				}

				_stages = value;
			}
		}

		private int _x, _z;

		public ChunkData(int x, int y)
		{
			_x = x;
			_z = y;

			var availableStages = Enum.GetValues(typeof(RenderStage));
			_stages = new StageData[availableStages.Length];
		}

		private DynamicVertexBuffer Buffer
		{
			get => _buffer;
			set
			{
				if (Disposed)
				{
					value?.Dispose();

					return;
				}

				_buffer = value;
			}
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
			if (Disposed || !_rendered)
			{
				return 0;
			}

			var rStage = Stages[(int)stage];

			if (rStage == null || rStage.Buffer == null || rStage.IndexCount <= 0)
			{
				return 0;
			}

			if (Buffer == null) return 0;

			IEffectMatrices em = (IEffectMatrices)effect;
			var originalWorld = em.World;

			try
			{
				device.SetVertexBuffer(Buffer);
				device.Indices = rStage.Buffer;

				em.World = Matrix.CreateTranslation(_x << 4, 0f, _z << 4);

				int count = 0;

				foreach (var pass in effect.CurrentTechnique.Passes)
				{
					pass.Apply();
					device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, rStage.IndexCount / 3);
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
			LinkedList<VertexData> data;

			lock (_dataLock)
			{
				data = _vertexDatas;
				_vertexDatas = new LinkedList<VertexData>();
			}


			List<MinifiedBlockShaderVertex> vertices = new List<MinifiedBlockShaderVertex>();

			foreach (var vertex in data)
			{
				vertices.Add(
					new MinifiedBlockShaderVertex(
						vertex.Position, vertex.Face, vertex.TexCoords.ToVector4(), new Color(vertex.Color), 0, 0));
			}

			return vertices.ToArray();
		}

		private LinkedList<VertexData> _vertexDatas = new LinkedList<VertexData>();

		private object _dataLock = new object();
		private DynamicVertexBuffer _buffer;

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
				var v3 = new Vector3(blockCoordinates.X, blockCoordinates.Y, blockCoordinates.Z);
				Vector3 lightProbe = v3 + position;

				if ((flags & VertexFlags.Solid) != 0)
				{
					lightProbe += (face.GetVector3());
				}

				_vertexDatas?.AddLast(
					new VertexData(
						v3 + position, face, textureCoordinates, color.PackedValue, flags, stage, lightProbe));
			}
		}

		public static float AverageUploadTime => MovingAverage.Average;
		public static float MaxUploadTime => MovingAverage.Maximum;
		public static float MinUploadTime => MovingAverage.Minimum;

		private static readonly MovingAverage MovingAverage = new MovingAverage();

		private static ArrayPool<MinifiedBlockShaderVertex> VertexArrayPool =
			ArrayPool<MinifiedBlockShaderVertex>.Create();

		public void ApplyChanges(IBlockAccess world, bool forceUpdate = false)
		{
			Stopwatch sw = Stopwatch.StartNew();

			List<IDisposable> toDisposeOff = new List<IDisposable>();

			MinifiedBlockShaderVertex[] vertices = null;

			try
			{
				var stages = Stages;

				if (stages == null)
					return;

				LinkedList<VertexData> data;

				lock (_dataLock)
				{
					data = _vertexDatas;
					_vertexDatas = new LinkedList<VertexData>();
				}

				if (data.Count == 0)
					return;

				StageData[] newStages = new StageData[stages.Length];

				for (int i = 0; i < stages.Length; i++)
				{
					newStages[i] = new StageData(stages[i]?.Buffer, new List<int>(stages[i]?.IndexCount ?? 0), 0);
				}

				vertices = new MinifiedBlockShaderVertex[data.Count];

				int idx = 0;

				foreach (var vertex in data)
				{
					var stage = vertex.Stage;
					var rStage = newStages[(int)stage];

					var index = idx;
					rStage.Indexes.Add(index);

					var lightProbe = vertex.LightPosition;

					byte blockLight = 0;
					byte skyLight = 0;

					if (world != null)
					{
						world.GetLight(lightProbe, out blockLight, out skyLight);
					}

					vertices[idx] = new MinifiedBlockShaderVertex(
						vertex.Position, vertex.Face, vertex.TexCoords.ToVector4(), new Color(vertex.Color), blockLight,
						skyLight);

					newStages[(int)stage] = rStage;
					idx++;
				}


				DynamicVertexBuffer buffer = Buffer;

				for (var index = 0; index < newStages.Length; index++)
				{
					var stage = newStages[index];

					if (stage == null || stage.Indexes.Count == 0)
						continue;

					var indexBuffer = stage.Buffer;

					var indexCount = stage.Indexes.Count;

					if (indexBuffer == null || indexBuffer.IndexCount < indexCount)
					{
						toDisposeOff.Add(indexBuffer);

						indexBuffer = new IndexBuffer(
							Alex.Instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indexCount,
							BufferUsage.WriteOnly);
					}

					indexBuffer.SetData(stage.Indexes.ToArray(), 0, indexCount);

					newStages[index] = new StageData(indexBuffer, null, indexCount);
				}

				var verticeCount = data.Count;

				while (verticeCount % 3 != 0) //Make sure we have a valid triangle list.
				{
					verticeCount--;
				}

				if (buffer == null || buffer.VertexCount < verticeCount)
				{
					toDisposeOff.Add(buffer);

					buffer = new DynamicVertexBuffer(
						Alex.Instance.GraphicsDevice, MinifiedBlockShaderVertex.VertexDeclaration, verticeCount,
						BufferUsage.WriteOnly);

					Interlocked.Increment(ref BufferCreations);
				}

				buffer.SetData(vertices, 0, data.Count);
				Stages = newStages;
				Buffer = buffer;

				Interlocked.Increment(ref BufferUploads);
			}
			finally
			{
				//  if (vertices != null)
				//       VertexArrayPool.Return(vertices);

				foreach (var toDispose in toDisposeOff)
				{
					toDispose?.Dispose();
				}

				MovingAverage.ComputeAverage((float)sw.Elapsed.TotalMilliseconds);
			}
		}

		public static int BufferUploads = 0;
		public static int BufferCreations = 0;
		public bool Disposed { get; private set; } = false;

		private void Dispose(bool disposing)
		{
			if (Disposed)
				return;

			Disposed = true;

			if (!disposing)
			{
				Log.Warn($"Disposing in destructor!");
			}


			var stages = _stages;

			if (stages != null)
			{
				_stages = null;

				for (var index = 0; index < stages.Length; index++)
				{
					var stage = stages[index];
					stage?.Buffer?.Dispose();
					stage?.Indexes?.Clear();
					stages[index] = null;
				}
			}

			Buffer?.Dispose();
			Buffer = null;

			if (_vertexDatas != null)
			{
				lock (_dataLock)
				{
					_vertexDatas?.Clear();
					_vertexDatas = null;
				}
			}

			if (disposing)
			{
				GC.SuppressFinalize(this);
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		~ChunkData()
		{
			Dispose(false);
		}
	}
}