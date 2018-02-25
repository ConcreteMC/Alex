using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ResourcePackLib.Json.Converters;

namespace ResourcePackLib.Json.Models
{
	[JsonConverter(typeof(BlockModelElementFaceUVConverter))]
	public class BlockModelElementFaceUV
	{

		public int X1 { get; set; } = 0;
		public int Y1 { get; set; } = 0;
		public int X2 { get; set; } = 16;
		public int Y2 { get; set; } = 16;

		public BlockModelElementFaceUV() { }
		public BlockModelElementFaceUV(int x1, int y1, int x2, int y2)
		{
			X1 = x1;
			Y1 = y1;
			X2 = x2;
			Y2 = y2;
		}

	}
}
