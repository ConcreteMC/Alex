using System;
using Alex.ResourcePackLib.Json.Fonts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Alex.ResourcePackLib.Json.Converters
{
	public class FontDefinitionConverter : JsonConverter
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(FontDefinitionConverter));

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override object ReadJson(JsonReader reader,
			Type objectType,
			object? existingValue,
			JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type != JTokenType.Object)
				return null;

			FontDefinition fontDefinition = null;

			//var jObject = (JObject)obj;

			//	return Decode180(jObject, serializer);
			var jObject = (JObject)obj;

			if (jObject.TryGetValue("type", StringComparison.InvariantCultureIgnoreCase, out var definitionType))
			{
				switch (definitionType.Value<string>())
				{
					case "bitmap":
						fontDefinition = jObject.ToObject<BitmapFontDefinition>();

						break;

					case "legacy_unicode":
						fontDefinition = jObject.ToObject<LegacyFontDefinition>();

						break;

					default:
						Log.Warn($"Unknown font definition type: {definitionType.Value<string>()}");

						return null;
				}
			}

			return fontDefinition;
		}

		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override bool CanConvert(Type objectType)
		{
			return typeof(FontDefinition).IsAssignableFrom(objectType);
		}
	}
}