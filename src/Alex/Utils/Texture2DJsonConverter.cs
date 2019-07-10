using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Alex.Utils
{
	public class Texture2DJsonConverter : JsonConverter<Texture2D>
	{
		private GraphicsDevice GraphicsDevice;
		public Texture2DJsonConverter(GraphicsDevice device)
		{
			GraphicsDevice = device;
		}

		public override void WriteJson(JsonWriter writer, Texture2D value, JsonSerializer serializer)
		{
			byte[] data;
			using (MemoryStream ms = new MemoryStream())
			{
				value.SaveAsPng(ms, value.Width, value.Height);
				data = ms.ToArray();
			}

			string savedValue = Convert.ToBase64String(data);
			writer.WriteValue(savedValue);
		}

		public override Texture2D ReadJson(JsonReader reader, Type objectType, Texture2D existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{
			try
			{
				Texture2D result;
				string base64Value = reader.Value.ToString();

				if (string.IsNullOrWhiteSpace(base64Value))
				{
					return null;
				}

				byte[] data = Convert.FromBase64String(base64Value);
				using (MemoryStream stream = new MemoryStream(data))
				{
					result = Texture2D.FromStream(GraphicsDevice, stream);
				}

				return result;
			}
			catch
			{
				return null;
			}
		}
	}
}
