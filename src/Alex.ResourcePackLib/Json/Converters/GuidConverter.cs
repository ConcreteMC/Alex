using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters
{
	public class GuidConverter : JsonConverter
	{
		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type == JTokenType.String)
			{
				var guidRaw = obj.Value<string>();

				if (Guid.TryParse(guidRaw, out Guid result))
				{
					return result;
				}
				else
				{
					return Guid.Empty;
				}
			}

			return null;
		}

		/// <inheritdoc />
		public override bool CanConvert(Type objectType)
		{
			return typeof(Guid).IsAssignableFrom(objectType);
		}
	}
}