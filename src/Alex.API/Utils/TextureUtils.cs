using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Alex.API.Graphics;
using Microsoft.Xna.Framework.Graphics;

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

        public static Texture2D BitmapToTexture2D(GraphicsDevice device, Bitmap bmp)
        {
            return BitmapToTexture2D(device, bmp, out _);
        }

        public static Texture2D BitmapToTexture2D(GraphicsDevice device, Bitmap bmp, out long byteSize)
		{
			if (bmp == null)
            {
                byteSize = 16 * 16 * 32;
				return GpuResourceManager.GetTexture2D("Alex.Api.Utils.TextureUtils", device, 16, 16);
			}

            byteSize = 0;
            
			var depth = System.Drawing.Bitmap.GetPixelFormatSize(bmp.PixelFormat);
			
			uint[] imgData = new uint[bmp.Width * bmp.Height];
			Texture2D texture = GpuResourceManager.GetTexture2D("Alex.Api.Utils.TextureUtils", device, bmp.Width, bmp.Height);

			byte[] data = new byte[bmp.Width * bmp.Height * (depth / 8)];
			LockBitmap locked = new LockBitmap(bmp);
			
			locked.LockBits();
			data = new byte[locked.Pixels.Length];
				Buffer.BlockCopy(locked.Pixels, 0, data, 0, locked.Pixels.Length);
			locked.UnlockBits();

			/*for (int i = 0; i < imgData.Length; i++)
			{
				var val = imgData[i];
				imgData[i] = (val & 0x000000FF) << 16 | (val & 0x0000FF00) | (val & 0x00FF0000) >> 16 | (val & 0xFF000000);
			}*/

			int idx = 0;
			if(depth == 32)
			{
				//var o = (uint*) origdata.Scan0.ToPointer();
				for (int i = 0; i < data.Length; i += 4)
				{
					//var val = o[i];
					var r = data[i + 0];
					var g = data[i + 1];
					var b = data[i + 2];
					var a = data[i + 3];
					imgData[idx++] = (uint) ((a << 24) | (r << 16) | (g << 8) | b);
				}
			}
			else if (depth == 24)
			{
				//var o = (byte*) origdata.Scan0.ToPointer();
				for (int i = 0; i < data.Length; i+= 3)
				{
					var r = data[i];
					var g = data[i + 1];
					var b = data[i + 2];
					var a = (byte)255;
					imgData[idx++] = (uint) ((a << 24) | (r << 16) | (g << 8) | b);
				}
			}
			
            	/*unsafe
                {
                    BitmapData origdata =
                        bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

                    
                    //fixed (uint* ptrDest = imgData)
                    if(depth == 32)
                    {
                        var o = (uint*) origdata.Scan0.ToPointer();
                        for (int i = 0; i < imgData.Length; i ++)
                        {
                            var val = o[i];
                            imgData[i] = (val & 0x000000FF) << 16 | (val & 0x0000FF00) | (val & 0x00FF0000) >> 16 | (val & 0xFF000000);
                        }
                    }
                    else if (depth == 24)
                    {
	                    var o = (byte*) origdata.Scan0.ToPointer();
	                    for (int i = 0; i < imgData.Length; i++)
	                    {
		                   // var val = o[i];
		                   // imgData[i] = (uint) ((128) << 16 | (o[i + 2]) | (o[i + 1]) >> 16 | (o[i]));
		                   imgData[i] = (uint) ((o[i + 2] << 16) | (o[i + 1] << 8) | (o[i] << 0));
	                    }
                    }

                    bmp.UnlockBits(origdata);
                }*/

                byteSize = imgData.Length * 4;

            texture.SetData(imgData);

			return texture;
		}

		public static TextureCube TexturesToCube(GraphicsDevice device, int size, Texture2D front, Texture2D right, Texture2D back,
		                                         Texture2D      left,   Texture2D up,    Texture2D down)
		{
			var cube = new TextureCube(device, size, false, SurfaceFormat.Color);
			
			cube.SetCubeFaceTexture(CubeMapFace.PositiveX, front);
			cube.SetCubeFaceTexture(CubeMapFace.PositiveZ, right);
			cube.SetCubeFaceTexture(CubeMapFace.NegativeX, back);
			cube.SetCubeFaceTexture(CubeMapFace.NegativeZ, left);
			cube.SetCubeFaceTexture(CubeMapFace.PositiveY, up);
			cube.SetCubeFaceTexture(CubeMapFace.NegativeY, down);
			return cube;
		}

		public static void SetCubeFaceTexture(this TextureCube cube, CubeMapFace face, Texture2D texture)
		{
			//if(texture.Width || cube.RenderSize != texture.Height) throw new ArgumentOutOfRangeException(nameof(texture.RenderBounds));

			uint[] imgData = new uint[texture.Width * texture.Height];

			texture.GetData(imgData);
			cube.SetData(face, imgData);
		}

		public static Texture2D ImageToTexture2D(GraphicsDevice device, Image bmp)
		{
			var image = new Bitmap(new Bitmap(bmp));
			return BitmapToTexture2D(device, image);
		}

		public static Texture2D ImageToTexture2D(GraphicsDevice device, byte[] bmp)
		{
			using (MemoryStream s = new MemoryStream(bmp))
			{
				var image = new Bitmap(new Bitmap(s));
				return BitmapToTexture2D(device, image);
			}
		}

		public static Texture2D Slice(this Bitmap bmp, GraphicsDevice graphics, Rectangle region)
		{
			return ImageToTexture2D(graphics, bmp.Clone(region, PixelFormat.Format32bppPArgb));
		}

		public static void CopyRegionIntoImage(Bitmap srcBitmap, System.Drawing.Rectangle srcRegion, ref Bitmap destBitmap,
			System.Drawing.Rectangle destRegion)
		{
			using (System.Drawing.Graphics grD = System.Drawing.Graphics.FromImage(destBitmap))
			{
				grD.DrawImage(srcBitmap, destRegion, srcRegion, GraphicsUnit.Pixel);
			}
		}
	}
}
