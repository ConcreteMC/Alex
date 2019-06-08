using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.API.Graphics
{
	public sealed class ChunkMesh : IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public VertexPositionNormalTextureColor[] Vertices { get; set; }
		public VertexPositionNormalTextureColor[] TransparentVertices { get; }
		
		public int[] SolidIndexes { get; set; }
		public int[] TransparentIndexes { get; set; }
		
		public ChunkMesh(VertexPositionNormalTextureColor[] entries, VertexPositionNormalTextureColor[] transparentEntries/*, 
			IDictionary<Vector3, EntryPosition> positions*/, int[] solidIndexes, int[] transparentIndexes)
		{
			//var entries = new Dictionary<Vector3, EntryPosition>();
			TransparentVertices = transparentEntries;
			Vertices = entries; //new VertexPositionNormalTextureColor[solidEntries.Sum(x => x.Vertices.Length)];
		//	TransparentVertices = transparentEntries;//.SelectMany(x => x.Vertices).ToArray();// new VertexPositionNormalTextureColor[transparentEntries.Sum(x => x.Vertices.Length)];
			SolidIndexes = solidIndexes;
			TransparentIndexes = transparentIndexes;
			/*int index = 0;
			for (var i = 0; i < solidEntries.Length; i++)
			{
				var e = solidEntries[i];

				entries.Add(e.RenderPosition, new EntryPosition(false, index, e.Vertices.Length));

				for (int x = 0; x < e.Vertices.Length; x++)
				{
					SolidVertices[index++] = e.Vertices[x];
				}
			}

			index = 0;
			for (var i = 0; i < transparentEntries.Length; i++)
			{
				var e = transparentEntries[i];

				entries.Add(e.RenderPosition, new EntryPosition(true, index, e.Vertices.Length));

				for (int x = 0; x < e.Vertices.Length; x++)
				{
					TransparentVertices[index++] = e.Vertices[x];
				}
			}

			EntryPositions = entries;*/

			//EntryPositions = new ReadOnlyDictionary<Vector3, EntryPosition>(positions);
		}

		public void Compress()
		{
			List<CompressionGroup> compressionGroups = new List<CompressionGroup>();

			var vertices = Vertices.ToList();

			var matchingTextureCoordinates = vertices.GroupBy(x => x.TexCoords).ToArray();
			foreach (var group in matchingTextureCoordinates)
			{
				var texCoords = group.Key;
				var matchedByColor = group.GroupBy(x => x.Color).ToArray();

				foreach (var match in matchedByColor)
				{
					List<CompressionItem> items = new List<CompressionItem>();
					foreach (var item in match)
					{
						var index = vertices.IndexOf(item);
						items.Add(new CompressionItem(index, item.Position));
					}

					compressionGroups.Add(new CompressionGroup(texCoords, match.Key, items.ToArray()));
				}
			}

			CompressionGroup[] groups = compressionGroups.ToArray();
        }

		private class CompressionGroup
		{
			public Vector2 TextureCoordinates { get; }
			public Color Color { get; }
			public CompressionItem[] Items { get; }
			public CompressionGroup(Vector2 textureCoordinates, Color color, CompressionItem[] items)
			{
				Color = color;
				TextureCoordinates = textureCoordinates;
				Items = items;
			}
		}

		private struct CompressionItem
		{
			public int Index { get; set; }
			public Vector3 Value { get; set; }

			public CompressionItem(int index, Vector3 value)
			{
				Index = index;
				Value = value;
			}
		}

		//public IReadOnlyDictionary<Vector3, EntryPosition> EntryPositions { get; }

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
			Vertices = null;
			SolidIndexes = null;
			TransparentIndexes = null;
		}
	}
}
