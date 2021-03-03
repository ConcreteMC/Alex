using System;
using Alex.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	internal class VersionedResourceConverter<T> : JsonConverter<VersionedResource<T>>
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(VersionedResourceConverter<T>));
		
		private string ValuesProperty { get; }
		public VersionedResourceConverter(string valuesProperty)
		{
			ValuesProperty = valuesProperty;
		}

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, VersionedResource<T> value, JsonSerializer serializer)
		{
			
		}

		/// <inheritdoc />
		public override VersionedResource<T> ReadJson(JsonReader reader,
			Type objectType,
			VersionedResource<T> existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type != JTokenType.Object) 
				return null;

			string              formatVersion = "1.8.0";
			var                 jObject       = (JObject)obj;
			VersionedResource<T> result        = new VersionedResource<T>();
			if (jObject.TryGetValue(
				"format_version", StringComparison.InvariantCultureIgnoreCase, out var versionToken))
			{
				string format = versionToken.Value<string>();
				formatVersion = format;
			}

			result.FormatVersion = FormatVersionHelpers.FromString(formatVersion);
			
			if (jObject.TryGetValue(ValuesProperty, out var values))
			{
				if (values.Type == JTokenType.Object)
				{
					foreach (var property in (JObject)values)
					{
						if (!result.TryAdd(property.Key, property.Value.ToObject<T>(serializer)))
						{
							Log.Warn($"Duplicate key: {property.Key}");
						}
					}
				}
			}
			
			return result;
		}

		/// <inheritdoc />
		public override bool CanWrite { get; } = false;

		/// <inheritdoc />
		//	public override bool CanConvert(Type objectType)
		//	{
		//		return typeof(VersionedResource<T>).IsAssignableFrom(objectType);
		//	}
	}
}