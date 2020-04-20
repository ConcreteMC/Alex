using System.Collections.Generic;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using NLog;
using System.Collections.Concurrent;
using SixLabors.ImageSharp.PixelFormats;

namespace Alex.Gui.Forms.Bedrock
{
    public class FormImage : GuiControl
    {
        //TODO: Get rid of static cache instance.
        private static ConcurrentDictionary<string, byte[]> _cache = new ConcurrentDictionary<string, byte[]>();
        private static ILogger Log = LogManager.GetCurrentClassLogger();
        
        public string Image { get; set; } = null;
        
        public FormImage(string url)
        {
            Image = url;
            
            Alex.Instance.ThreadPool.QueueUserWorkItem(() =>
            {
                try
                {
                    byte[] imageData = _cache.GetOrAdd(Image, (path) =>
                    {
                        using (WebClient wc = new WebClient())
                        {
                            var data = wc.DownloadData(path);

                            return data;
                        }
                    });

                    Background = (TextureSlice2D)TextureUtils.BitmapToTexture2D(Alex.Instance.GraphicsDevice, SixLabors.ImageSharp.Image.Load<Rgba32>(imageData));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Could not convert image!");
                }
            });
        }
    }
}