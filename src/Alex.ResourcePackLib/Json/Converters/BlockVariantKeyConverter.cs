using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Alex.ResourcePackLib.Json.BlockStates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters
{
	public class BlockVariantKeyTypeConverter : TypeConverter
	{
		/// <inheritdoc />
		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			return value is string str ? new BlockVariantKey(str) : base.ConvertFrom(context, culture, value);
		}

		/// <inheritdoc />
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		{
			return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
		}
	}

	public class BlockVariantKeyConverter : JsonConverter
	{
		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override object? ReadJson(JsonReader reader,
			Type objectType,
			object? existingValue,
			JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type == JTokenType.String)
			{
				var stringValue = obj.Value<string>();

				return new BlockVariantKey(stringValue);
			}

			return null;
		}

		/// <inheritdoc />
		public override bool CanConvert(Type objectType)
		{
			return typeof(BlockVariantKey).IsAssignableFrom(objectType);
		}

		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override bool CanRead => true;
	}
}