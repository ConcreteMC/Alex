using System;
using System.Linq;
using Alex.Common.Blocks;
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
		float Scale { get; }
		int CenterX { get; }
		int CenterZ { get; }
	}
	
	public class Map : IMap
	{
		public int Width { get; }
		public int Height { get; }
		public float Scale { get; } = 1f;
		
		public int CenterX { get; set; }
		public int CenterZ { get; set; }

		private int[][] _colors;
		//private Texture2D _texture;
		
		protected int Layers { get; }
		public Map(int width, int height, int layers = 1)
		{
			Layers = layers;
			Width = width;
			Height = height;
			_colors = new int[width * height][];

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

		public uint[] GetData()
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