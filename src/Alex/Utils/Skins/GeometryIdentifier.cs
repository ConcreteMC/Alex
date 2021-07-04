using System;
using Newtonsoft.Json;

namespace Alex.Utils.Skins
{
	public class GeometryIdentifier : ICloneable
	{
		[JsonProperty("default")]
		public string Default { get; set; }
		
		[JsonProperty("animated_face")]
		public string AnimatedFace { get; set; }
		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}