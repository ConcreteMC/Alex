using System;
using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Textures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.BlockStates
{
	public sealed class MultiPartRule
	{
		[JsonIgnore] public Dictionary<string, string> KeyValues = new Dictionary<string, string>();

		[JsonConverter(typeof(SingleOrArrayConverter<MultiPartRule>))]
		public MultiPartRule[] Or = null;

		[JsonConverter(typeof(SingleOrArrayConverter<MultiPartRule>))]
		public MultiPartRule[] And = null;

		public bool HasOrContition => Or != null && Or.Length >= 1;

		public bool HasAndContition => And != null && And.Length >= 1;
	}
}