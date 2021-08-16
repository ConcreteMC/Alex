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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = Microsoft.Xna.Framework.Color;

namespace Alex.Utils
{
	public interface IMap : IDisposable
	{
		int Width { get; }
		int Height { get; }
		float Scale { get; }
		Vector3 Center { get; }
		float Rotation { get; }

		uint[] GetData();
		Texture2D GetTexture(GraphicsDevice device, Vector3 center);
		
		void Add(MapIcon icon);
		void Remove(MapIcon icon);
		IEnumerable<MapIcon> GetMarkers(ChunkCoordinates center, int radius);

		//void Move(Vector2 amount);
	}
	
	public class Map : IMap
	{
		public int Width { get; }
		public int Height { get; }
		public float Scale { get; set; } = 1f;
		public Vector3 Center { get; set; } = Vector3.Zero;
		public float Rotation { get; set; } = 0f;
		
		//public int CenterX { get; set; }
	//	public int CenterZ { get; set; }

		private int[][] _colors;
		//private Texture2D _texture;
		
		private readonly ConcurrentHashSet<MapIcon> _markers;
		protected int Layers { get; }
		public Map(int width, int height, int layers = 1)
		{
			Layers = layers;
			Width = width;
			Height = height;
			_colors = new int[width * height][];
			_markers = new ConcurrentHashSet<MapIcon>();
			
			for (int i = 0; i < _colors.Length; i++)
			{
				_colors[i] = new int[layers];
			}
			//_texture = new Texture2D(Alex.Instance.GraphicsDevice, width, height);
		}

		public Map(NbtCompound compound)
		{
			throw new NotImplementedException();
		}

		public int this[int x, int y]
		{
			get
			{
				return _colors[x + y * Width][0];
			}
			set
			{
				_colors[x + y * Width][0] = value;
			}
		}
		
		public int this[int x, int y, int layer]
		{
			get
			{
				return _colors[x + y * Width][layer];
			}
			set
			{
				_colors[x + y * Width][layer] = value;
			}
		}

		public Color GetColor(int x, int y)
		{
			return MapColor.GetBlockColor(this[x, y]);
		}
		
		public Color GetColor(int x, int y, int layer)
		{
			return MapColor.GetBlockColor(this[x, y, layer]);
		}

		public Image GetImage()
		{
			Image<Rgba32> image = new Image<Rgba32>(Width, Height);
			for(int x = 0; x < Width; x++)
			for (int y = 0; y < Height; y++)
				image[x, y] = new Rgba32(MapColor.GetBlockColor(this[x, y]).PackedValue);

			return image;
		}
		
		//public Color[] GetColors()
		//{
		//	return _colors.Select(MapColor.GetBlockColor).ToArray();
		//}

		public virtual uint[] GetData()
		{
			Color[] colors = new Color[Width * Height];

			for (int c = 0; c < _colors.Length; c++)
			{
				var layerData = _colors[c];
				var color = MapColor.GetBlockColor(layerData[0]);

				if (layerData.Length > 1)
				{
					for (int i = layerData.Length; i > 0; --i)
					{
						
					}
				}

				colors[c] = color;
			}

			return colors.Select(x => x.PackedValue).ToArray();
		//	return _colors.Select(x =>  MapColor.GetBlockColor(x).PackedValue).ToArray();
		}

		/// <inheritdoc />
		public virtual Texture2D GetTexture(GraphicsDevice device, Vector3 center)
		{
			var texture = new Texture2D(device, Width, Height);
			texture.SetData(GetData());

			return texture;	
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
           
			foreach (var icon in markers.Where(x => x.AlwaysShown || new ChunkCoordinates(x.Position).DistanceTo(center) <= radius).OrderBy(x => x.DrawOrder))
			{
				yield return icon;
			}
		}

		private void Dispose(bool disposing)
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
	}
}