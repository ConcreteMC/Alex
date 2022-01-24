using System;
using Newtonsoft.Json;

namespace Alex.Utils.Skins
{
	public class SkinResourcePatch : ICloneable
	{
		[JsonProperty("geometry")] public GeometryIdentifier Geometry { get; set; }

		public object Clone()
		{
			var cloned = new SkinResourcePatch();
			cloned.Geometry = (GeometryIdentifier)Geometry?.Clone();

			return cloned;
		}
	}
}