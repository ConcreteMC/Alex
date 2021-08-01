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

		private int[] _colors;
		//private Texture2D _texture;
		public Map(int width, int height)
		{
			Width = width;
			Height = height;
			_colors = new int[width * height];
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
				return _colors[x + y * Width];
			}
			set
			{
				_colors[x + y * Width] = value;
			}
		}

		public Color GetColor(int x, int y)
		{
			return MapColor.GetBlockColor(this[x, y]);
		}

		public Image GetImage()
		{
			Image<Rgba32> image = new Image<Rgba32>(Width, Height);
			for(int x = 0; x < Width; x++)
			for (int y = 0; y < Height; y++)
				image[x, y] = new Rgba32(MapColor.GetBlockColor(this[x, y]).PackedValue);

			return image;
		}
		
		public Color[] GetColors()
		{
			return _colors.Select(MapColor.GetBlockColor).ToArray();
		}

		public uint[] GetData()
		{
			return _colors.Select(x =>  MapColor.GetBlockColor(x).PackedValue).ToArray();
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