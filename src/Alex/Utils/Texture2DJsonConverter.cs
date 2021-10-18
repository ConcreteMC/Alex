﻿using System;
using System.IO;
using System.Threading;
using Alex.Common.Graphics.GpuResources;
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
			byte[] data = null;
			if (!Program.IsRunningOnStartupThread())
			{
				ManualResetEvent resetEvent = new ManualResetEvent(false);
				

				Alex.Instance.UiTaskManager.Enqueue(
					() =>
					{
						using (MemoryStream ms = new MemoryStream())
						{
							value.SaveAsPng(ms, value.Width, value.Height);
							data = ms.ToArray();
						}

						resetEvent.Set();
					});


				resetEvent.WaitOne();
			}
			else
			{
				using (MemoryStream ms = new MemoryStream())
				{
					value.SaveAsPng(ms, value.Width, value.Height);
					data = ms.ToArray();
				}
			}

			string savedValue = Convert.ToBase64String(data);
			writer.WriteValue(savedValue);
		}

		public override Texture2D ReadJson(JsonReader reader, Type objectType, Texture2D existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{
			try
			{
				if (reader.Value == null)
				{
					return null;
				}
				string base64Value = reader.Value.ToString();

				if (string.IsNullOrWhiteSpace(base64Value))
				{
					return null;
				}

				byte[] data = Convert.FromBase64String(base64Value);
				
				if (!Program.IsRunningOnStartupThread())
				{
					ManualResetEvent resetEvent = new ManualResetEvent(false);

					Texture2D result      = null;

					Alex.Instance.UiTaskManager.Enqueue(
						() =>
						{
							using (MemoryStream stream = new MemoryStream(data))
							{
								result = Texture2D.FromStream(GraphicsDevice, stream);
							}

							resetEvent.Set();
						});

					resetEvent.WaitOne();

					return result;
				}
				else
				{
					using (MemoryStream stream = new MemoryStream(data))
					{
						return Texture2D.FromStream(GraphicsDevice, stream);
					}
				}
			}
			catch
			{
				return null;
			}
		}
	}
}
