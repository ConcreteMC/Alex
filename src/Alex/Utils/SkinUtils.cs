using System;
using System.Drawing;
using System.IO;
using System.Net;
using Alex.API.Graphics;
using Alex.Blocks.Minecraft;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils.Skins;
using Newtonsoft.Json;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Alex.Utils
{
	public static class SkinUtils
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();
		public static bool TryGetSkin(string json, GraphicsDevice graphics, out PooledTexture2D texture, out bool isSlim)
		{
			isSlim = false;
			try
			{
				TexturesResponse r = JsonConvert.DeserializeObject<TexturesResponse>(json);
				if (r != null)
				{
					string url = r.textures?.SKIN?.url;
					if (url != null)
					{
						byte[] data;
						using (WebClient wc = new WebClient())
						{
							data = wc.DownloadData(url);
						}

						using (MemoryStream ms = new MemoryStream(data))
						{
							texture = GpuResourceManager.GetTexture2D("SkinUtils", graphics, ms);// Texture2D.FromStream(graphics, ms);
						}

						isSlim = (r.textures.SKIN.metadata?.model == "slim");

						return true;
					}
				}
			}
			catch(Exception ex)
			{
				Log.Warn(ex, $"Could not retrieve skin: {ex.ToString()}");
			}

			texture = null;
			return false;
		}
		
		public static bool TryGetSkin(Uri skinUri, GraphicsDevice graphics, out PooledTexture2D texture)
		{
			try
			{
				byte[] data;
				using (WebClient wc = new WebClient())
				{
					data = wc.DownloadData(skinUri);
				}

				using (MemoryStream ms = new MemoryStream(data))
				{
					texture = GpuResourceManager.GetTexture2D("SkinUtils", graphics, ms);
				}

				return true;
			}
			catch(Exception ex)
			{
				Log.Warn(ex, $"Could not retrieve skin: {ex.ToString()}");
			}

			texture = null;
			return false;
		}

		public static bool TryGetBitmap(this Skin skin, out Image<Rgba32> result)
		{
			try
			{
				var bytes = skin.Data;

				/*int width = 64;
				var height = bytes.Length == 64 * 32 * 4 ? 32 : 64;*/
				int width = skin.Width;
				int height = skin.Height;

				Image<Rgba32> bitmap = new Image<Rgba32>(width, height);

				int i = 0;
				for (int y = 0; y < bitmap.Height; y++)
				{
					for (int x = 0; x < bitmap.Width; x++)
					{
						byte r = bytes[i++];
						byte g = bytes[i++];
						byte b = bytes[i++];
						byte a = bytes[i++];

                        bitmap[x, y] = new Rgba32(r, g, b, a);
                        //Color color = Color.FromArgb(a, r, g, b);
                        //bitmap.SetPixel(x, y, color);
                    }
				}

				result = bitmap;
				return true;
			}
			catch
			{
				result = null;
				return false;
			}
		}

		public class SkinMetadata
		{
			public string model { get; set; }
		}

		public class SKIN
		{
			public string url { get; set; }
			public SkinMetadata metadata { get; set; } = null;
		}

		public class Textures
		{
			public SKIN SKIN { get; set; }
			public SKIN CAPE { get; set; }
		}

		public class TexturesResponse
		{
			public long timestamp { get; set; }
			public string profileId { get; set; }
			public string profileName { get; set; }
			public Textures textures { get; set; }
		}
	}
}
