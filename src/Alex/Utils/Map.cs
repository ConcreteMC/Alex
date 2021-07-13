using System;
using System.Linq;
using Alex.Common.Blocks;
using fNbt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Alex.Utils
{
	public class Map
	{
		public int Width { get; }
		public int Height { get; }
		public byte Scale { get; } = 3;
		
		public int CenterX { get; set; }
		public int CenterZ { get; set; }

		private IMapColor[] _colors;
		//private Texture2D _texture;
		public Map(int width, int height)
		{
			Width = width;
			Height = height;
			_colors = new IMapColor[width * height];

			//_texture = new Texture2D(Alex.Instance.GraphicsDevice, width, height);
		}

		public Map(NbtCompound compound)
		{
			throw new NotImplementedException();
		}

		public IMapColor this[int x, int y]
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

		public uint[] GetData()
		{
			return _colors.Select(x => x.BaseColor.PackedValue).ToArray();
		}
	}
}