using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Alex.Common.Utils;
using Microsoft.Xna.Framework;
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
		}

		private bool _didLoad = false;
		private SemaphoreSlim _loadingLock = new SemaphoreSlim(1);
		/// <inheritdoc />
		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			base.OnDraw(graphics, gameTime);

			if (!_didLoad)
			{
				_ = LoadAsync();
			}
		}

		private async Task LoadAsync()
		{
			if (!await _loadingLock.WaitAsync(0))
				return;

			try
			{
				var path = Image;
				if (!_cache.TryGetValue(path, out var data))
				{
					HttpClient httpClient = new HttpClient();
					data = await httpClient.GetByteArrayAsync(path);
					_cache.TryAdd(path, data);
				}

				if (data == null)
					return;

				using (MemoryStream ms = new MemoryStream(data))
				{
					var image = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(ms);

					TextureUtils.BitmapToTexture2DAsync(
						this, Alex.Instance.GraphicsDevice, image, texture =>
						{
							Background = (TextureSlice2D) texture;
							image?.Dispose();
						}, $"FormImage - {path}");
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Could not convert image!");
			}
			finally
			{
				_didLoad = true;
				_loadingLock.Release();
			}
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_loadingLock?.Dispose();
				_loadingLock = null;
			}
			
			base.Dispose(disposing);
		}

		public static void ClearCache()
		{
			_cache?.Clear();
		}
	}
}