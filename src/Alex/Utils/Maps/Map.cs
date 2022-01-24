using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Common.Blocks;
using Alex.Common.Utils.Vectors;
using Alex.Gui.Elements.Map;
using ConcurrentCollections;
using fNbt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = Microsoft.Xna.Framework.Color;
using Image = SixLabors.ImageSharp.Image;

namespace Alex.Utils
{
	public class Map : IMap
	{
		public int Width { get; }
		public int Height { get; }
		public float Scale { get; set; } = 1f;
		public Vector3 Center { get; set; } = Vector3.Zero;
		public float Rotation { get; set; } = 0f;

		private Color[] _colors;
		private readonly ConcurrentHashSet<MapIcon> _markers;

		public bool HasChanges { get; protected set; } = false;

		public Map(int width, int height)
		{
			Width = width;
			Height = height;
			_colors = new Color[width * height];
			_markers = new ConcurrentHashSet<MapIcon>();
		}

		public Map(NbtCompound compound)
		{
			throw new NotImplementedException();
		}

		public Color this[int x, int y]
		{
			get
			{
				return _colors[x + y * Width];
			}
			set
			{
				_colors[x + y * Width] = value;
				HasChanges = true;
			}
		}

		public Image GetImage()
		{
			Image<Rgba32> image = new Image<Rgba32>(Width, Height);

			for (int x = 0; x < Width; x++)
			for (int y = 0; y < Height; y++)
				image[x, y] = new Rgba32(this[x, y].PackedValue);

			return image;
		}

		public virtual uint[] GetData()
		{
			var data = _colors.Select(x => x.PackedValue).ToArray();
			HasChanges = false;

			return data;
			//	return _colors.Select(x =>  MapColor.GetBlockColor(x).PackedValue).ToArray();
		}

		/// <inheritdoc />
		public virtual Texture2D GetTexture(GraphicsDevice device)
		{
			var texture = new Texture2D(device, Width, Height);
			texture.SetData(GetData());

			return texture;
		}

		/// <inheritdoc />
		public IEnumerable<IMapElement> GetSections(ChunkCoordinates center, int radius)
		{
			yield return new WrappedTexture(Center, GetTexture(Alex.Instance.GraphicsDevice));
		}

		/// <inheritdoc />
		public void Add(MapIcon icon)
		{
			if (icon == null)
				return;

			_markers.Add(icon);
		}

		/// <inheritdoc />
		public void Remove(MapIcon icon)
		{
			if (icon == null)
				return;

			_markers.TryRemove(icon);
		}

		/// <inheritdoc />
		public IEnumerable<MapIcon> GetMarkers(ChunkCoordinates center, int radius)
		{
			var markers = _markers;

			if (markers == null || markers.IsEmpty)
				yield break;

			foreach (var icon in markers
				        .Where(
					         x => x.AlwaysShown || Math.Abs(new ChunkCoordinates(x.Position).DistanceTo(center))
						         <= radius).OrderBy(x => x.DrawOrder))
			{
				yield return icon;
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				//		_texture?.Dispose();
				//	_texture = null;
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
		}

		~Map()
		{
			Dispose(false);
		}

		public class WrappedTexture : IMapElement
		{
			/// <inheritdoc />
			public Vector3 Position { get; }

			private Texture2D _texture;

			public WrappedTexture(Vector3 position, Texture2D texture)
			{
				Position = position;
				_texture = texture;
			}

			/// <inheritdoc />
			public Texture2D GetTexture(GraphicsDevice device)
			{
				return _texture;
			}
		}
	}
}