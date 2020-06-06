using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Alex.API.Graphics;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Image = SixLabors.ImageSharp.Image;
using Point = SixLabors.Primitives.Point;
using Rectangle = System.Drawing.Rectangle;

namespace Alex.API.Utils
{
	public static class TextureUtils
	{
		public static Bitmap Texture2DToBitmap(Texture2D texture)
		{
            //argb
            //abgr
            //texture.Fo
           //     rmat
            //uint[] imgData = new uint[texture.Height * texture.Width];
            uint[] imgData = new uint[texture.Height * texture.Width];
            texture.GetData(imgData);
            
			Bitmap bmp = new Bitmap(texture.Width, texture.Height, PixelFormat.Format32bppArgb);
			unsafe
			{
				BitmapData origdata =
					bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

                uint* targetData = (uint*)origdata.Scan0;
				for (int i = 0; i < imgData.Length; i++)
				{
					var val = imgData[i];
                    targetData[i] = (val & 0x000000FF) << 16 | (val & 0xFF000000) | (val & 0x00FF0000) >> 16 |
                                    (val & 0x0000FF00);
                    // rgba;// (rgba << (3 * 8)) | (rgba >> 8);
                }

               // IntPtr safePtr = origdata.Scan0;
               // System.Runtime.InteropServices.Marshal.Copy(imgData, 0, safePtr, imgData.Length);

                bmp.UnlockBits(origdata);
			}

			return bmp;
		}

        public static PooledTexture2D BitmapToTexture2D(GraphicsDevice device, Image<Rgba32> bmp)
        {
	        return BitmapToTexture2D(device, bmp, out _);
        }
        
        public static PooledTexture2D BitmapToTexture2D(GraphicsDevice device, Image<Rgba32> image, out long byteSize)
        {
	        var bmp = image;//.CloneAs<Rgba32>();
	        var pixels = bmp.GetPixelSpan();
	        var colorData = pixels.ToArray().Select(x => x.Rgba).ToArray();

	        PooledTexture2D texture = GpuResourceManager.GetTexture2D("Image converter", device, image.Width, image.Height);
	        texture.SetData(colorData);

	        byteSize = texture.MemoryUsage;
	        return texture;
	        /*for (int x = 0; x < bmp.Width; x++)
	        {
		        for (int y = 0; y < bmp.Height; y++)
		        {
			        
		        }
	        }
	        using (MemoryStream ms = new MemoryStream())
	        {
		        bmp.SaveAsPng(ms);
		        
		        ms.Position = 0;
		        byteSize = ms.Length;
		        return GpuResourceManager.GetTexture2D("Alex.Api.Utils.TextureUtils.ImageToTexture2D", device, ms);
	        }*/
        }

		public static Texture2D ImageToTexture2D(GraphicsDevice device, byte[] bmp)
		{
			//using (MemoryStream s = new MemoryStream(bmp))
			{
				var image = Image<Rgba32>.Load(bmp);
				return BitmapToTexture2D(device, image);
			}
		}

        public static Texture2D Slice(this Image<Rgba32> bmp, GraphicsDevice graphics, Rectangle region)
        {
            return BitmapToTexture2D(graphics, bmp.Clone(context => context.Crop(new SixLabors.Primitives.Rectangle(region.X, region.Y, region.Width, region.Height))));
        }

        public static void CopyRegionIntoImage(Image<Rgba32> srcBitmap, System.Drawing.Rectangle srcRegion,
			ref Image<Rgba32> destBitmap,
			System.Drawing.Rectangle destRegion)
		{
			try
			{
				using (var newImage = srcBitmap.Clone(x =>
				{
					x.Crop(new SixLabors.Primitives.Rectangle(srcRegion.X, srcRegion.Y, srcRegion.Width,
						srcRegion.Height));
				}))
				{
					var nwImage = newImage;
					destBitmap.Mutate(context =>
					{
						context.DrawImage(nwImage, new Point(destRegion.Location.X, destRegion.Location.Y),
							PixelColorBlendingMode.Normal, 1f);
					});
				}
			}
			catch (Exception ex)
			{
				
			}
		}
	}
}
