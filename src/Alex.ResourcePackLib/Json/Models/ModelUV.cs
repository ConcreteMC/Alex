using Alex.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Models
{
	[JsonConverter(typeof(BlockModelElementFaceUVConverter))]
	public class ModelUV
	{

		public float X1 { get; set; } = 0;
		public float Y1 { get; set; } = 0;
		public float X2 { get; set; } = 16;
		public float Y2 { get; set; } = 16;

		public ModelUV() { }
		public ModelUV(float x1, float y1, float x2, float y2)
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
