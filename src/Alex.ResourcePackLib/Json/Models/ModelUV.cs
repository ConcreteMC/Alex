﻿using Alex.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Models
{
	[JsonConverter(typeof(BlockModelElementFaceUVConverter))]
	public class ModelUV
	{
		public int X1 { get; set; } = 0;
		public int Y1 { get; set; } = 0;
		public int X2 { get; set; } = 16;
		public int Y2 { get; set; } = 16;

		public ModelUV() { }

		public ModelUV(int x1, int y1, int x2, int y2)
		{
			X1 = x1;
			Y1 = y1;
			X2 = x2;
			Y2 = y2;
		}

		public ModelUV Clone()
		{
			return new ModelUV(X1, Y1, X2, Y2);
		}
	}
}