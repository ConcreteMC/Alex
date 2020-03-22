using System;
using System.Drawing;
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

namespace Alex.Gui.Forms.Bedrock
{
    public class FormImage : GuiControl
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();
        
        public string Image { get; set; } = null;
        
        public FormImage(string url)
        {
            Image = url;
            
            Alex.Instance.ThreadPool.QueueUserWorkItem(() =>
            {
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        byte[] imageData = wc.DownloadData(Image);
                        using (MemoryStream ms = new MemoryStream(imageData))
                        {
                            Bitmap bmp = new Bitmap(ms);
                            
                            Alex.Instance.UIThreadQueue.Enqueue(() =>
                            {
                                try
                                {
                                    Background =
                                        (TextureSlice2D) TextureUtils.BitmapToTexture2D(
                                            Alex.Instance.GraphicsDevice,
                                            bmp);
                                }
                                catch (Exception ex)
                                {
                                    Log.Error(ex, "Could not get form image.");
                                }
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex,$"Could not convert image!");
                }
            });
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
        }
    }
}