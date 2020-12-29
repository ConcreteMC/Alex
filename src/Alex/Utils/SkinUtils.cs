using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using Alex.API.Graphics;
using Alex.Blocks.Minecraft;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils.Skins;
using Newtonsoft.Json;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Point = System.Drawing.Point;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace Alex.Utils
{
	public enum SkinSize
	{
		W64H32,
		W64H64
	}
	
	public static class SkinUtils
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		public static Image<Rgba32> ConvertSkin(Image<Rgba32> input, int width, int height)
		{
			SkinSize size       = SkinSize.W64H32;
			SkinSize sourceSize = SkinSize.W64H32;
			
			if (width / height == 2)
				size = SkinSize.W64H64;
			else if (width / height == 1)
				size = SkinSize.W64H32;
			
			if (input.Width / input.Height == 2)
				sourceSize = SkinSize.W64H64;
			else if (input.Width / input.Height == 1)
				sourceSize = SkinSize.W64H32;

			if (size == sourceSize)
				return input;

			Image<Rgba32> output = new Image<Rgba32>(width, height, new Rgba32(255, 255, 255, 0f));
			output.Mutate(
				m =>
				{
					//Head
					var cloned = input.Clone();
					cloned.Mutate(x => x.Crop(new Rectangle(0, 0, 32, 16)));

					m.DrawImage(cloned, 1f);
					
					//Hat
					cloned = input.Clone();
					cloned.Mutate(x => x.Crop(new Rectangle(32, 0, 32, 16)));
					
					m.DrawImage(cloned, new SixLabors.ImageSharp.Point(32, 0), 1f);
					
					//Body
					var body = input.Clone();
					body.Mutate(x => x.Crop(new Rectangle(16, 16, 24, 16)));
					m.DrawImage(body, new SixLabors.ImageSharp.Point(16, 16), 1f);
					
					//Legs
					var leg = input.Clone();
					leg.Mutate(x => x.Crop(new Rectangle(0, 16, 16, 16)));
					
					m.DrawImage(leg, new SixLabors.ImageSharp.Point(0, 16), 1f); //Right Leg

					var clonedLeg = leg.Clone();
					leg.Mutate(
						x =>
						{
							var outer = clonedLeg.Clone();
							outer.Mutate(o =>
							{
								o.Crop(new Rectangle(0, 4, 4, 12));
								o.Flip(FlipMode.Horizontal);
							});
							
							x.DrawImage(outer, new SixLabors.ImageSharp.Point(8, 4), 1f);
							
							var inner = clonedLeg.Clone();
							inner.Mutate(o =>
							{
								o.Crop(new Rectangle(8, 4, 4, 12));
								o.Flip(FlipMode.Horizontal);
							});

							x.DrawImage(inner, new SixLabors.ImageSharp.Point(0, 4), 1f);
							
							var front = clonedLeg.Clone();
							front.Mutate(o =>
							{
								o.Crop(new Rectangle(4, 4, 4, 12));
								o.Flip(FlipMode.Horizontal);
							});

							x.DrawImage(front, new SixLabors.ImageSharp.Point(4, 4), 1f);
							
							var back = clonedLeg.Clone();
							back.Mutate(o =>
							{
								o.Crop(new Rectangle(12, 4, 4, 12));
								o.Flip(FlipMode.Horizontal);
							});

							x.DrawImage(back, new SixLabors.ImageSharp.Point(12, 4), 1f);
							
							var top = clonedLeg.Clone();
							top.Mutate(o =>
							{
								o.Crop(new Rectangle(4, 0, 4, 4));
								o.Flip(FlipMode.Horizontal);
							});

							x.DrawImage(top, new SixLabors.ImageSharp.Point(4, 0), 1f);
							
							var bottom = clonedLeg.Clone();
							bottom.Mutate(o =>
							{
								o.Crop(new Rectangle(8, 0, 4, 4));
								o.Flip(FlipMode.Horizontal);
							});

							x.DrawImage(bottom, new SixLabors.ImageSharp.Point(8, 0), 1f);
						});
					m.DrawImage(leg, new SixLabors.ImageSharp.Point(16, 48), 1f); //Left Leg
					
					//Arms
					var arm = input.Clone();
					arm.Mutate(x => x.Crop(new Rectangle(40, 16, 16, 16)));
					m.DrawImage(arm, new SixLabors.ImageSharp.Point(40, 16), 1f); //Right Arm
					
					var clonedArm = arm.Clone();
					arm.Mutate(
						x =>
						{
							var outer = clonedArm.Clone();
							outer.Mutate(o =>
							{
								o.Crop(new Rectangle(0, 4, 4, 12));
								o.Flip(FlipMode.Horizontal);
							});
							
							x.DrawImage(outer, new SixLabors.ImageSharp.Point(8, 4), 1f);
							
							var inner = clonedArm.Clone();
							inner.Mutate(o =>
							{
								o.Crop(new Rectangle(8, 4, 4, 12));
								o.Flip(FlipMode.Horizontal);
							});

							x.DrawImage(inner, new SixLabors.ImageSharp.Point(0, 4), 1f);
							
							var front = clonedArm.Clone();
							front.Mutate(o =>
							{
								o.Crop(new Rectangle(4, 4, 4, 12));
								o.Flip(FlipMode.Horizontal);
							});

							x.DrawImage(front, new SixLabors.ImageSharp.Point(4, 4), 1f);
							
							var back = clonedArm.Clone();
							back.Mutate(o =>
							{
								o.Crop(new Rectangle(12, 4, 4, 12));
								o.Flip(FlipMode.Horizontal);
							});

							x.DrawImage(back, new SixLabors.ImageSharp.Point(12, 4), 1f);
							
							var top = clonedArm.Clone();
							top.Mutate(o =>
							{
								o.Crop(new Rectangle(4, 0, 4, 4));
								o.Flip(FlipMode.Horizontal);
							});

							x.DrawImage(top, new SixLabors.ImageSharp.Point(4, 0), 1f);
							
							var bottom = clonedArm.Clone();
							bottom.Mutate(o =>
							{
								o.Crop(new Rectangle(8, 0, 4, 4));
								o.Flip(FlipMode.Horizontal);
							});

							x.DrawImage(bottom, new SixLabors.ImageSharp.Point(8, 0), 1f);
						});
				//	arm.Mutate(x => x.Flip(FlipMode.Horizontal));
					m.DrawImage(arm, new SixLabors.ImageSharp.Point(32, 48), 1f); //Left Arm
				});

			return output;
		}
		
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

						ManualResetEvent resetEvent = new ManualResetEvent(false);

						PooledTexture2D text = null;
						Alex.Instance.UIThreadQueue.Enqueue(
							() =>
							{
								using (MemoryStream ms = new MemoryStream(data))
								{
									text = GpuResourceManager.GetTexture2D(
										"SkinUtils", graphics, ms); // Texture2D.FromStream(graphics, ms);
								}

								resetEvent.Set();
							});
						
						resetEvent.WaitOne();

						texture = text;
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
				
				ManualResetEvent resetEvent = new ManualResetEvent(false);

				PooledTexture2D text = null;
				Alex.Instance.UIThreadQueue.Enqueue(
					() =>
					{
						using (MemoryStream ms = new MemoryStream(data))
						{
							text = GpuResourceManager.GetTexture2D(
								"SkinUtils", graphics, ms); // Texture2D.FromStream(graphics, ms);
						}

						resetEvent.Set();
					});
						
				resetEvent.WaitOne();

				texture = text;

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
