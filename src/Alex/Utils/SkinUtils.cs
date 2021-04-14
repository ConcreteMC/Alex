using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Alex.API.Graphics;
using Alex.Blocks.Minecraft;
using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils.Skins;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;
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
			return input;
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
		
		public static void TryGetSkin(string json, GraphicsDevice graphics, Action<PooledTexture2D, bool> onComplete)
		{
			//isSlim = false;
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

						PooledTexture2D text = null;
						Alex.Instance.UIThreadQueue.Enqueue(
							() =>
							{
								using (MemoryStream ms = new MemoryStream(data))
								{
									text = GpuResourceManager.GetTexture2D(
										"SkinUtils", graphics, ms); // Texture2D.FromStream(graphics, ms);
								}

								onComplete?.Invoke(text, r.textures.SKIN.metadata?.model == "slim");
								//resetEvent.Set();
							});
						
						//resetEvent.WaitOne();

						//texture = text;
						//isSlim = (r.textures.SKIN.metadata?.model == "slim");

						//return true;
					}
				}
			}
			catch(Exception ex)
			{
				Log.Warn(ex, $"Could not retrieve skin: {ex.ToString()}");
			}

			//texture = null;
			//return false;
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

		public static bool TryGetBitmap(this Skin skin, EntityModel model, out Image<Rgba32> result)
		{
			try
			{
				var bytes = skin.Data;

				/*int width = 64;
				var height = bytes.Length == 64 * 32 * 4 ? 32 : 64;*/
				int width = skin.Width;
				int height = skin.Height;

				if (bytes.Length != width * height * 4)
				{
					if (model.Description.TextureWidth * model.Description.TextureHeight * 4 == bytes.Length)
					{
						width = (int) model.Description.TextureWidth;
						height = (int) model.Description.TextureHeight;
					}
				}
				
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

		public static MiNET.Utils.Skins.Skin ToSkin(this EntityModel model)
		{
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
			settings.MissingMemberHandling = MissingMemberHandling.Ignore;
			//settings.Formatting = Formatting.Indented;
			settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			settings.Converters.Add(new StringEnumConverter {NamingStrategy = new CamelCaseNamingStrategy()});

			MiNET.Utils.Skins.Skin skin     = null;
			Geometry               geometry = new Geometry();

			geometry.Bones = new List<Bone>();

			foreach (var bone in model.Bones)
			{
				var a = new Bone();

				if (Enum.TryParse<BoneName>(bone.Name, true, out var boneName))
				{
					a.Name = boneName;
				}
				else
				{
					a.Name = BoneName.Unknown;
				}

				if (!string.IsNullOrWhiteSpace(bone.Parent))
				{
					if (Enum.TryParse<BoneName>(bone.Parent, true, out var parentName))
					{
						if (parentName != a.Name)
						{
							a.Parent = parentName;
						}
					}
				}

				if (bone.NeverRender)
					a.NeverRender = true;

				if (bone.Pivot.HasValue)
				{
					a.Pivot = new float[] {bone.Pivot.Value.X, bone.Pivot.Value.Y, bone.Pivot.Value.Z};
				}

				if (bone.Rotation.HasValue)
				{
					a.Rotation = new float[] {bone.Rotation.Value.X, bone.Rotation.Value.Y, bone.Rotation.Value.Z};
				}

				/*if (bone.BindPoseRotation.HasValue)
				{
					if (a.Rotation == null || a.Rotation.Length == 0)
						a.Rotation = new float[3];

					a.Rotation[0] += bone.BindPoseRotation.Value.X;
					a.Rotation[1] += bone.BindPoseRotation.Value.Y;
					a.Rotation[2] += bone.BindPoseRotation.Value.Z;
				}*/
				
				if (bone.Cubes != null)
				{
					a.Cubes = new List<Cube>();

					foreach (var c in bone.Cubes)
					{
						var newCube = new ExtendedCube()
						{
							Inflate = (float) (c.Inflate.HasValue ? c.Inflate.Value : 0f),
							Origin = new float[] {c.Origin.X, c.Origin.Y, c.Origin.Z},
							Size = new float[] {c.Size.X, c.Size.Y, c.Size.Z},
							Uv = new float[] {c.Uv.South.Origin.X, c.Uv.South.Origin.Y}
						};

						if (c.Mirror.HasValue)
							newCube.Mirror = c.Mirror.Value;

						if (c.Rotation.HasValue)
							newCube.Rotation = new float[] {c.Rotation.Value.X, c.Rotation.Value.Y, c.Rotation.Value.Z};

						if (c.Pivot.HasValue)
							newCube.Pivot = new float[] {c.Pivot.Value.X, c.Pivot.Value.Y, c.Pivot.Value.Z};

						a.Cubes.Add(newCube);
					}
				}

				geometry.Bones.Add(a);
			}

			// geometry.Bones = model.Bones.Select(x => ).ToList();

			//   geometry.Name = model.Description.Identifier;
			//geometry.TextureHeight = (int) model.Description.TextureHeight;
			// geometry.TextureWidth = (int) model.Description.TextureWidth;

			geometry.Description = new Description()
			{
				Identifier = $"geometry.humanoid.customSlim",
				TextureHeight = (int) model.Description.TextureHeight,
				TextureWidth = (int) model.Description.TextureWidth,
				VisibleBoundsHeight = (int) model.Description.VisibleBoundsHeight,
				VisibleBoundsWidth = (int) model.Description.VisibleBoundsWidth,
				VisibleBoundsOffset = new int[]
				{
					(int) model.Description.VisibleBoundsOffset.X,
					(int) model.Description.VisibleBoundsOffset.Y, (int) model.Description.VisibleBoundsOffset.Z
				}
			};

			//geometry.Subdivide(true, true, true, false);

			skin = new MiNET.Utils.Skins.Skin()
			{
				SkinId = $"{Guid.NewGuid().ToString()}.steve",
				SkinResourcePatch =
					new MiNET.Utils.Skins.SkinResourcePatch()
					{
						Geometry = new MiNET.Utils.Skins.GeometryIdentifier()
						{
							Default = geometry.Description.Identifier
						}
					},
				GeometryData = JsonConvert.SerializeObject(
					new Dictionary<string, object>()
					{
						{"format_version", "1.12.0"}, {"minecraft:geometry", new[] {geometry}},
					}, Formatting.None, settings),
				SkinColor = "#0",
				ArmSize = "slim",
				Data = null
			};

			return skin;
		}

		public static MiNET.Utils.Skins.Skin UpdateTexture(this MiNET.Utils.Skins.Skin skin, PooledTexture2D texture)
		{
			Image<Rgba32> skinTexture;

			using (MemoryStream ms = new MemoryStream())
			{
				texture.SaveAsPng(ms, texture.Width, texture.Height);
				ms.Position = 0;

				skinTexture = Image.Load(ms, new PngDecoder()).CloneAs<Rgba32>();
			}
			    
			byte[] skinData;
			using (MemoryStream ms = new MemoryStream())
			{
				if (skinTexture.TryGetSinglePixelSpan(out var span))
				{
					foreach (var value in span)
					{
						ms.WriteByte(value.R);
						ms.WriteByte(value.G);
						ms.WriteByte(value.B);
						ms.WriteByte(value.A);
					}
				}

				skinData = ms.ToArray();
			}

			skin.Width = skinTexture.Width;
			skin.Height = skinTexture.Height;
			skin.Data = skinData;

			return skin;
		}

		public class ExtendedCube : MiNET.Utils.Skins.Cube
		{
			/// <summary>
			/// If this field is specified, rotation of this cube occurs around this point, otherwise its rotation is around the center of the box.
			/// Note that in 1.12 this is flipped upside-down, but is fixed in 1.14.
			/// </summary>
			[JsonProperty("pivot", NullValueHandling = NullValueHandling.Ignore)]
			public float[] Pivot { get; set; } = null;
			
			/// <summary>
			/// The cube is rotated by this amount (in degrees, x-then-y-then-z order) around the pivot.
			/// </summary>
			[JsonProperty("rotation", NullValueHandling = NullValueHandling.Ignore)]
			public float[] Rotation { get; set; } = null;
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
