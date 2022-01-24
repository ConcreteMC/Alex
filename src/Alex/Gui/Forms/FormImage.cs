using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using Alex.Common.Utils;
using RocketUI;
using NLog;
using SixLabors.ImageSharp.PixelFormats;

namespace Alex.Gui.Forms
{
	public class FormImage : RocketControl
	{
		//TODO: Get rid of static cache instance.
		private static ConcurrentDictionary<string, byte[]> _cache = new ConcurrentDictionary<string, byte[]>();
		private static ILogger Log = LogManager.GetCurrentClassLogger();

		public string Image { get; set; } = null;

		public FormImage(string url)
		{
			Image = url;

			ThreadPool.QueueUserWorkItem(
				(o) =>
				{
					try
					{
						byte[] imageData = _cache.GetOrAdd(
							Image, (path) =>
							{
								using (WebClient wc = new WebClient())
								{
									var data = wc.DownloadData(path);

									return data;
								}
							});

						var image = SixLabors.ImageSharp.Image.Load<Rgba32>(imageData);
						TextureUtils.BitmapToTexture2DAsync(
							this, Alex.Instance.GraphicsDevice, image, texture =>
							{
								Background = (TextureSlice2D)texture;
								image.Dispose();
							}, $"FormImage - {url}");
					}
					catch (Exception ex)
					{
						Log.Error(ex, $"Could not convert image!");
					}
				});
		}
	}
}