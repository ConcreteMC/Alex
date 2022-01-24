using System;
using Alex.Common.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters
{
	public class ResourceLocationConverter : JsonConverter<ResourceLocation>
	{
		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, ResourceLocation? value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override ResourceLocation? ReadJson(JsonReader reader,
			Type objectType,
			ResourceLocation? existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type == JTokenType.String)
			{
				return new ResourceLocation(obj.Value<string>());
			}

			return null;
			var str = reader.ReadAsString();

			return new ResourceLocation(str);
		}
	}
}