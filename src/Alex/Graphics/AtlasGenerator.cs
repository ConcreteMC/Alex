using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Resources;
using Alex.API.Utils;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Gamestates.InGame;
using Alex.Graphics.Models.Blocks;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Textures;
using Alex.Utils;
using Alex.Worlds.Singleplayer;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using Color = SixLabors.ImageSharp.Color;
using Point = SixLabors.ImageSharp.Point;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace Alex.Graphics
{
    public class AtlasGenerator
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));

	    private Dictionary<ResourceLocation, TextureInfo> _atlasLocations = new Dictionary<ResourceLocation, TextureInfo>();
	    private Dictionary<ResourceLocation, TextureInfo> _animatedAtlasLocations = new Dictionary<ResourceLocation, TextureInfo>();
	    
	    private PooledTexture2D[] _frames;
	    private Texture2D _stillFrame;
        public Vector2 AtlasSize { get; private set; }
        public Vector2 AnimatedAtlasSize { get; private set; }
        
        public AtlasGenerator()
	    {

	    }

	    public void Reset()
	    {
		    _atlasLocations = new Dictionary<ResourceLocation, TextureInfo>();
		    _animatedAtlasLocations = new Dictionary<ResourceLocation, TextureInfo>();

		    AtlasSize = default;
		    _frames = default;
		    _stillFrame = default;
	    }

	    public void GetTextures(McResourcePack resourcePack,
		    Dictionary<ResourceLocation, ImageEntry> textures,
		    IProgressReceiver progressReceiver)
	    {
		    /*List<ResourceLocation> texturePaths = new List<ResourceLocation>();
		    foreach (var model in resourcePack.BlockModels)
		    {
			    foreach (var texture in model.Value.Textures)
			    {
				    if (!texturePaths.Contains(texture.Value))
					    texturePaths.Add(texture.Value);
			    }
			   // model.Value.Textures
		    }*/

		    int done  = 0;
		//    var items = resourcePack.Textures.Where(x => texturePaths.Contains(x.Key)).ToArray();
		var texturePaths = resourcePack.Textures.Where(x => x.Key.Path.Contains("block/")).ToArray();
		    foreach (var path in texturePaths)
		    {
			    progressReceiver?.UpdateProgress(done++, texturePaths.Length, "Resolving textures...", path.ToString());

			    if (resourcePack.TryGetBitmap(path.Key, out var texture))
			    {
				    TextureMeta meta = null;
				    resourcePack.TryGetTextureMeta(path.Key, out meta);
				    //var entry = new ImageEntry(texture.Value.Value, meta);

				    if (textures.ContainsKey(path.Key))
				    {
					    if (meta != null)
					    {
						    textures[path.Key].Meta = meta;
					    }

					    if (texture != null)
						    textures[path.Key].Image = texture;

					    //textures[texture.Key] = entry;
				    }
				    else
				    {
					    textures.Add(path.Key, new ImageEntry(texture, meta));
				    }
			    }
		    }
	    }

	    public class ImageEntry
	    {
		    public Image<Rgba32> Image { get; set; }
		    public TextureMeta         Meta  { get; set; }

		    public int Width  => Image.Width;
		    public int Height => Image.Height;

		    public ImageEntry(Image<Rgba32> image, TextureMeta meta)
		    {
			    Image = image;
			    Meta = meta;
		    }
	    }

	    private void GenerateAtlas(GraphicsDevice device,
		    IDictionary<ResourceLocation, ImageEntry> blockTextures,
		    IProgressReceiver progressReceiver)
	    {
		    Stopwatch sw = Stopwatch.StartNew();

		    Log.Info($"Generating texture atlas out of {(blockTextures.Count)} bitmaps...");

		    long totalSize = 0;

		    Image<Rgba32> no = new Image<Rgba32>(TextureWidth, TextureHeight);

		    for (int x = 0; x < no.Width; x++)
		    {
			    var xCheck = x < no.Width / 2;
			    
			    for (int y = 0; y < no.Height; y++)
			    {
				    var yCheck = y < no.Height / 2;
				    
				    if ((xCheck && yCheck) || (!xCheck && !yCheck))
				    {
					    no[x, y] = Color.Purple;
				    }
				    else
				    {
					    no[x, y] = Color.Black;
				    }
			    }
		    }
			/*
		    using (MemoryStream ms = new MemoryStream(ResourceManager.ReadResource("Alex.Resources.no.png")))
		    {
			    no = Image.Load<Rgba32>(ms);
		    }*/

			var normal = blockTextures.Where(x => x.Value.Height == TextureHeight && x.Value.Width == TextureWidth)
			   .Select(x => new KeyValuePair<ResourceLocation, Image<Rgba32>>(x.Key, x.Value.Image)).ToArray();

			var nonNormal = blockTextures.Where(x => x.Value.Height != x.Value.Width)
			   .ToDictionary(x => x.Key, x => x.Value);
			 //  .Select(x => new KeyValuePair<ResourceLocation, Image<Rgba32>>(x.Key, x.Value.Image.Value)).ToArray();

			Dictionary<ResourceLocation, TextureInfo> stillFrameInfo = new Dictionary<ResourceLocation, TextureInfo>();

		    GenerateAtlasInternal(
			    new[] {new KeyValuePair<ResourceLocation, Image<Rgba32>>("no_texture", no),}.Concat(
				    normal
			    ).ToArray(),
			    progressReceiver, stillFrameInfo, false, out var stillAtlas);

		    foreach (var nor in normal)
		    {
			    nor.Value.Dispose();
		    }

		    AtlasSize = new Vector2(stillAtlas.Width, stillAtlas.Height);
		    //  totalSize += size;

		    _atlasLocations = stillFrameInfo;

		    var stillFrame = GetMipMappedTexture2D(device, stillAtlas, "static", true);
		    totalSize += stillFrame.MemoryUsage;
		    _stillFrame = stillFrame;
			    
		    _frames = ProcessFrames(device,
			    progressReceiver, nonNormal);

		    totalSize += _frames.Sum(x => x.MemoryUsage);
		    
		    sw.Stop();
		    
		    Log.Info(
			    $"TextureAtlas's generated in {sw.ElapsedMilliseconds}ms! ({PlayingState.GetBytesReadable(totalSize, 2)})");
	    }
	    
	    private PooledTexture2D[] ProcessFrames(GraphicsDevice device, IProgressReceiver progressReceiver,
		    Dictionary<ResourceLocation, ImageEntry> blockTextures)
	    {
		    Dictionary<ResourceLocation, Image<Rgba32>[]> blockFrames =
			    new Dictionary<ResourceLocation, Image<Rgba32>[]>();
		    
		    foreach (var other in blockTextures.Where(
			    x => x.Value.Height != x.Value.Width))
		    {
			    if (!blockFrames.TryGetValue(other.Key, out _))
			    {
				    var f = GetFrames(other.Value, other.Value.Width < TextureWidth ? 16 : TextureWidth, other.Value.Height < TextureHeight ? 16 : TextureHeight);

				    if (f.Length > 0)
				    {
					    blockFrames.Add(other.Key, f);
				    }
			    }
		    }

		    var animatedFrameInfo = new Dictionary<ResourceLocation, TextureInfo>();
		    
		    GenerateAtlasInternal(blockFrames.Select(x => new KeyValuePair<ResourceLocation, Image<Rgba32>>(x.Key, x.Value[0])).ToArray(),
			    progressReceiver, animatedFrameInfo, true, out var animatedFrame);

		    AnimatedAtlasSize = new Vector2(animatedFrame.Width, animatedFrame.Height);
		    _animatedAtlasLocations = animatedFrameInfo;


		    var frameCount = blockFrames.Max(x => x.Value.Length);

		    while (frameCount % 2 != 0)
		    {
			    frameCount++;
		    }

		    var frames = new Image<Rgba32>[frameCount];
		    frames[0] = animatedFrame;

		    for (int i = 1; i < frames.Length; i++)
		    {
			  //  double percentage = 100D * ((double) i / (double) frames.Length);
			    progressReceiver.UpdateProgress(i, frames.Length, $"Animating frame {i + 1} / {frames.Length}...");

			    var target = (i > 0 ? frames[i - 1] : animatedFrame).CloneAs<Rgba32>(); //new Bitmap(animatedFrame);

			    // System.Drawing.Rectangle destination;

			    foreach (var animated in blockFrames)
			    {
				    progressReceiver.UpdateProgress(i, frames.Length, null, animated.Key.ToString());

				    if (animatedFrameInfo.TryGetValue(animated.Key, out var textureInfo))
				    {
					    //((i % 3 == 0 ? i - 1 : i) / 6)

					    var destination = new System.Drawing.Rectangle(
						    (int) textureInfo.Position.X, (int) textureInfo.Position.Y, textureInfo.Width * 16,
						    textureInfo.Height * 16);

					    var sourceRegion = new System.Drawing.Rectangle(0, 0, textureInfo.Width * 16, textureInfo.Height * 16);

					    var index = i % animated.Value.Length;

					    var   indexOffset        = 0;
					    bool  shouldInterpolate  = false;
					    float interpolationValue = 0.5f;

					    if (blockTextures.TryGetValue(animated.Key, out var imageEntry) && imageEntry.Meta != null)
					    {
						    var meta = imageEntry.Meta;

						    if (meta.Animation != null)
						    {
							    if (meta.Animation.Interpolate)
							    {
								    int extraFrames = (frames.Length - animated.Value.Length);

								    var interpolationFrames = (int) Math.Floor(
									    ((double) extraFrames / (double) animated.Value.Length));

								    var remainder = i % interpolationFrames;

								    if (remainder != 0)
								    {
									    shouldInterpolate = true;
									    interpolationValue = (1f / interpolationFrames) * remainder;

									    indexOffset = -remainder;
									    //   index -= remainder;
								    }
							    }

							    if (meta.Animation.Frames != null)
							    {
								    var entry = meta.Animation.Frames[(i + indexOffset) % meta.Animation.Frames.Length];

								    if (entry.Integer.HasValue)
								    {
									    index = (int) entry.Integer.Value;
								    }
								    else if (entry.FrameClass != null)
								    {
									    index = (int) entry.FrameClass.Index;
								    }
							    }
							    else
							    {
								    index = (i + indexOffset);
							    }
						    }
					    }

					    //TextureUtils.ClearRegion(ref target, destination);

					    if (shouldInterpolate)
					    {
						    TextureUtils.CopyRegionIntoImage(
							    ((i + indexOffset >= 0) ? frames[(i + indexOffset) % frames.Length] : animatedFrame),
							    destination, ref target, destination, clear: true);
					    }

					    var texture = animated.Value[index % animated.Value.Length];

					    TextureUtils.CopyRegionIntoImage(
						    texture, sourceRegion, ref target, destination, shouldInterpolate, interpolationValue,
						    clear: !shouldInterpolate);
				    }
			    }

			    frames[i] = target;
		    }

		    foreach (var a in blockTextures)
		    {
			    a.Value.Image.Dispose();
		    }

		    foreach (var blockFrame in blockFrames)
		    {
			    foreach (var b in blockFrame.Value)
			    {
				    b.Dispose();
			    }
		    }
		    
		    
		    return frames.Select(
			    (x, index) =>
			    {
				    var a = GetMipMappedTexture2D(device, x, $"frame-{index}");
				    //totalSize += a.MemoryUsage;

				    return a;
			    }).ToArray();
	    }

	    private PooledTexture2D GetMipMappedTexture2D(GraphicsDevice device, Image<Rgba32> image, string name, bool save = false)
	    {
		    //device.VertexSamplerStates.
		    PooledTexture2D texture = GpuResourceManager.GetTexture2D(this, device, image.Width, image.Height, true, SurfaceFormat.Color);

		 //   if (!Directory.Exists("atlas"))
		//	    Directory.CreateDirectory("atlas");
		    // Directory.CreateDirectory(name);
		    // var resampler = new BicubicResampler();
		    // var encoder = new PngEncoder();
		    for (int level = 0; level < Alex.MipMapLevel; level++)
		    {
			    int mipWidth  = (int) System.Math.Max(1, image.Width >> level);
			    int mipHeight = (int) System.Math.Max(1, image.Height >> level);

			    if (mipWidth < TextureWidth || mipHeight < TextureHeight)
			    {
				    Alex.MipMapLevel = level - 1;
				    break;
			    }
			    
			    var bmp = image.CloneAs<Rgba32>(); //.CloneAs<Rgba32>();

			    try
			    {
				    bmp.Mutate(x => x.Resize(mipWidth, mipHeight, KnownResamplers.NearestNeighbor, true));

				    uint[] colorData;

				    if (bmp.TryGetSinglePixelSpan(out var pixelSpan))
				    {
					    colorData = pixelSpan.ToArray().Select(x => x.Rgba).ToArray();
				    }
				    else
				    {
					    throw new Exception("Could not get image data!");
				    }

				    //TODO: Resample per texture instead of whole texture map.

				    texture.SetData(level, null, colorData, 0, colorData.Length);
				  //  if (save)
					//	bmp.SaveAsPng(Path.Combine("atlas", $"{name}-{level}.png"));

			    }
			    finally
			    {
				    bmp.Dispose();
			    }
		    }

		    return texture;
	    }

	    private const int Spacing     = 0;
	    private       int _spacingHalf = Spacing > 0 ? Spacing / 2 : 0;
	    private void GenerateAtlasInternal(KeyValuePair<ResourceLocation, Image<Rgba32>>[] regular, IProgressReceiver progressReceiver, Dictionary<ResourceLocation, TextureInfo> atlasLocations, bool animated, out Image<Rgba32> result)
        {
	        int total = regular.Length;
	        var a = (int)Math.Ceiling(regular.Length / 32D);

	        int height = a * (TextureHeight + Spacing);
	        var width  = Math.Min(32, total) * (TextureWidth + Spacing);

	        if (height == 0 || width == 0)
	        {
		        result = null;
		        return;
	        }

	        var bitmap = new Image<Rgba32>(width, height);

	        int xi = 0, yi = 0, offsetX = 0, yRemaining = 0;
	        Process(ref bitmap, regular, ref xi, ref yi, ref offsetX, ref yRemaining, total, 0, atlasLocations, progressReceiver, animated);

	        result = bitmap;
        }

	    private int Process(ref Image<Rgba32> target, KeyValuePair<ResourceLocation, Image<Rgba32>>[] data, ref int xi, ref int yi, ref int xOffset, ref int yRemaining, int total, int processed, IDictionary<ResourceLocation, TextureInfo> locations, IProgressReceiver progressReceiver, bool animated)
	    {
		    xi += _spacingHalf;
		    yi += _spacingHalf;
		    
		    int done = processed;
			var count = 0;

		    foreach (var bm in data.OrderByDescending(x => x.Value.Height))
		    {
			    var key = bm.Key;
			    count++;

			   // double percentage = 100D * ((double)processed / (double)total);
			    progressReceiver.UpdateProgress(processed, total, null, key.ToString());

                var sourceRegion = new System.Drawing.Rectangle(0, 0, bm.Value.Width, bm.Value.Height);
			    var targetRegion = new System.Drawing.Rectangle(xi, yi, bm.Value.Width, bm.Value.Height);

			    TextureUtils.CopyRegionIntoImage(bm.Value, sourceRegion, ref target, targetRegion);

			    if (!locations.ContainsKey(key)) 
			    {
				    locations.Add(key, new TextureInfo(new Vector2(target.Width, target.Height),  new Vector2(xi, yi), bm.Value.Width / 16, bm.Value.Height / 16, animated, true));
			    }

			    if (bm.Value.Height > TextureHeight)
			    {
				    xOffset = xi;
				    yRemaining = bm.Value.Height;
			    }
			    xi += bm.Value.Width + Spacing;

			    if (count == 32)
			    {
				    yi += TextureHeight + Spacing;
				    xi = _spacingHalf;
				    yRemaining -= TextureHeight + Spacing;

				    count = 0;
			    }

			    done++;

			   
			}

		    return done;
	    }

	    private Image<Rgba32>[] GetFrames(ImageEntry entry, int frameWidth, int frameHeight)
	    {
		    var source         = entry.Image;
		    
		    int framesInWidth  = source.Width / frameWidth;
		    int framesInHeight = source.Height / frameHeight;

		    //List<Image<Rgba32>> result = new List<Image<Rgba32>>();
		    Image<Rgba32>[] result  = new Image<Rgba32>[framesInHeight * framesInWidth];
		    int             counter = 0;
		    for (int x = 0; x < framesInWidth; x++)
		    for (int y = 0; y < framesInHeight; y++)
		    {
			    var x1 = x;

			    var y1 = y;

			    var newBitmap = entry.Image.Clone(
				    img => img.Crop(
					    new Rectangle(
						    new Point(x1 * frameWidth, y1 * frameHeight), new Size(frameWidth, frameHeight))));

			    result[counter++] = newBitmap;
		    }
		    
		    return result;
	    }

	    public int TextureWidth { get; private set; } = 16;
	    public int TextureHeight { get; private set; }= 16;

        public void LoadResourcePackOnTop(GraphicsDevice device, Dictionary<ResourceLocation, ImageEntry> loadedTextures, McResourcePack resourcePack, IProgressReceiver progressReceiver, bool build)
		{
			int textureWidth = TextureWidth, textureHeight = TextureHeight;

			GetTextures(resourcePack, loadedTextures, progressReceiver);

			foreach (var image in loadedTextures.ToArray())
			{
				var texture = image.Value;

				if ((texture.Width > textureWidth && texture.Width % 16 == 0)
				    && (texture.Height > textureHeight && texture.Height % 16 == 0))
				{
					if (texture.Width == texture.Height)
					{
						textureWidth = texture.Width;
						textureHeight = texture.Height;
					}
				}
			}

            TextureHeight = textureHeight;
            TextureWidth = textureWidth;

            if (build) GenerateAtlas(device, loadedTextures, progressReceiver);
		}


		public int GetFrameCount()
		{
			return _frames.Length;
		}

		public Texture2D GetStillAtlas()
		{
			return _stillFrame;
		}
		
		public Texture2D GetAtlas(int frame)
		{
			return _frames[frame % _frames.Length];
        }

		public TextureInfo GetAtlasLocation(
			ResourceLocation file)
		{
			if (_animatedAtlasLocations.TryGetValue(file, out var textureInfo))
			{
				return textureInfo;
			}

			if (_atlasLocations.TryGetValue(file, out var atlasInfo))
			{
				return atlasInfo;
			}

			return _atlasLocations
				["no_texture"]; // new TextureInfo(AtlasSize, Vector2.Zero, TextureWidth, TextureHeight, false, false);
		}
    }
}
