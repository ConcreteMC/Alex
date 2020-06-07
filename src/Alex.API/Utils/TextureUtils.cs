using System;
using System.IO;
using System.Linq;
using Alex.API.Graphics;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;
using Rectangle = System.Drawing.Rectangle;

namespace Alex.API.Utils
{
	public static class TextureUtils
	{
		public static PooledTexture2D BitmapToTexture2D(GraphicsDevice device, Image<Rgba32> bmp)
        {
	        return BitmapToTexture2D(device, bmp, out _);
        }
        
        public static PooledTexture2D BitmapToTexture2D(GraphicsDevice device, Image<Rgba32> image, out long byteSize)
        {
	        var bmp = image;//.CloneAs<Rgba32>();
	        uint[] colorData;
	        if (bmp.TryGetSinglePixelSpan(out var pixelSpan))
	        {
		        colorData = pixelSpan.ToArray().Select(x => x.Rgba).ToArray();
	        }
	        else
	        {
		        throw new Exception("Could not get image data!");
	        }
	       // var colorData = pixels.ToArray().Select(x => x.Rgba).ToArray();

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
				var image = Image.Load(bmp);
				return BitmapToTexture2D(device, image);
			}
		}

        public static Texture2D Slice(this Image<Rgba32> bmp, GraphicsDevice graphics, Rectangle region)
        {
	        return BitmapToTexture2D(graphics, bmp.Clone(context => context.Crop(new SixLabors.ImageSharp.Rectangle(region.X, region.Y, region.Width, region.Height))));
        }

        public static void CopyRegionIntoImage(Image<Rgba32> srcBitmap, System.Drawing.Rectangle srcRegion,
			ref Image<Rgba32> destBitmap,
			System.Drawing.Rectangle destRegion)
		{
			try
			{
				using (var newImage = srcBitmap.Clone(x =>
				{
					x.Crop(new SixLabors.ImageSharp.Rectangle(srcRegion.X, srcRegion.Y, srcRegion.Width,
						srcRegion.Height));
				}))
				{
					var nwImage = newImage;
					destBitmap.Mutate(context =>
					{
						context.DrawImage(nwImage, new SixLabors.ImageSharp.Point(destRegion.Location.X, destRegion.Location.Y),
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
