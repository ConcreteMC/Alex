using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock
{
	public class VersionedResource<T>
	{
		[JsonProperty("format_version")] public FormatVersion FormatVersion { get; set; }

		public Dictionary<string, T> Values { get; }

		public VersionedResource()
		{
			Values = new Dictionary<string, T>();
		}

		public bool TryAdd(string key, T value)
		{
			return Values.TryAdd(key, value);
		}
	}
}