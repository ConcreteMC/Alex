using System;
using Alex.ResourcePackLib.Json.Bedrock.Sound;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters.Bedrock
{
	public class SoundEventConverter : JsonConverter
	{
		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type == JTokenType.Object)
			{
			//	var gradientValue = ((JObject) obj).ToObject<SoundEvent>(serializer);

				return new SoundEvent()
				{
					Sound = ((JObject) obj).GetValue("sound")?.Value<string>() ?? null
				};
			}
			else if (obj.Type == JTokenType.String)
			{
				return new SoundEvent() {Sound = obj.Value<string>()};
			}

			return null;
		}

		/// <inheritdoc />
		public override bool CanConvert(Type objectType)
		{
			return typeof(SoundEvent).IsAssignableFrom(objectType);
		}
	}
}