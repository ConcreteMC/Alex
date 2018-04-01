using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Alex.API.Graphics
{
	public sealed class ChunkMesh : IDisposable
	{
		public VertexPositionNormalTextureColor[] SolidVertices { get; }
		public VertexPositionNormalTextureColor[] TransparentVertices { get; }
		public ChunkMesh(VertexPositionNormalTextureColor[] solidEntries, VertexPositionNormalTextureColor[] transparentEntries)
		{
			//var entries = new Dictionary<Vector3, EntryPosition>();

			SolidVertices = solidEntries; //new VertexPositionNormalTextureColor[solidEntries.Sum(x => x.Vertices.Length)];
			TransparentVertices = transparentEntries;//.SelectMany(x => x.Vertices).ToArray();// new VertexPositionNormalTextureColor[transparentEntries.Sum(x => x.Vertices.Length)];

			/*int index = 0;
			for (var i = 0; i < solidEntries.Length; i++)
			{
				var e = solidEntries[i];

				entries.Add(e.Position, new EntryPosition(false, index, e.Vertices.Length));

				for (int x = 0; x < e.Vertices.Length; x++)
				{
					SolidVertices[index++] = e.Vertices[x];
				}
			}

			index = 0;
			for (var i = 0; i < transparentEntries.Length; i++)
			{
				var e = transparentEntries[i];

				entries.Add(e.Position, new EntryPosition(true, index, e.Vertices.Length));

				for (int x = 0; x < e.Vertices.Length; x++)
				{
					TransparentVertices[index++] = e.Vertices[x];
				}
			}

			EntryPositions = entries;*/
		}

	//	public IReadOnlyDictionary<Vector3, EntryPosition> EntryPositions { get; }

		public sealed class Entry
		{
			public VertexPositionNormalTextureColor[] Vertices;
			public Vector3 Position;
			public uint ID;
			public Entry(uint id, VertexPositionNormalTextureColor[] vertices, 
				Vector3 position)
			{
				ID = id;
				Vertices = vertices;
				Position = position;
			}
		}

		public sealed class EntryPosition
		{
			public int Index { get; }
			public int Length { get; }
			public bool Transparent { get; }

			public EntryPosition(bool transparent, int index, int length)
			{
				Transparent = transparent;
				Index = index;
				Length = length;
			}
		}

		public void Dispose()
		{
			
		}
	}
}
