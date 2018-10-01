using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Alex.Utils
{
	public class SkinUtils
	{
		public static bool TryGetSkin(string json, GraphicsDevice graphics, out Texture2D texture, out bool isSlim)
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
							texture = Texture2D.FromStream(graphics, ms);
						}

						isSlim = (r.textures.SKIN.metadata?.model == "slim");

						return true;
					}
				}
			}
			catch
			{
				
			}

			texture = null;
			return false;
		}
		public static bool TryGetSkin(Uri skinUri, GraphicsDevice graphics, out Texture2D texture)
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
					texture = Texture2D.FromStream(graphics, ms);
				}

				return true;
			}
			catch
			{

			}

			texture = null;
			return false;
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
