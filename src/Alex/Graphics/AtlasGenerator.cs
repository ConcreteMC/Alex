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

	    private void GetTextures(McResourcePack resourcePack,
		    Dictionary<ResourceLocation, ImageEntry> textures,
		    IProgressReceiver progressReceiver)
	    {
		    foreach (var texture in resourcePack.Textures.Where(x => x.Key.Path.StartsWith("block/", StringComparison.InvariantCultureIgnoreCase)))
		    {
			    if (textures.ContainsKey(texture.Key))
			    {
				    continue;
			    }
			    
			    TextureMeta meta = null;
			    resourcePack.TryGetTextureMeta(texture.Key, out meta);
			    textures.Add(texture.Key, new ImageEntry(texture.Value, meta));
		    }
		    /*
		    progressReceiver.UpdateProgress(0, "Processing blockstate textures...");
		    int blockstatesProcessed = 0;
		    int totalStates          = resourcePack.BlockStates.Count;

		    foreach (var kv in resourcePack.BlockStates)
		    {
			    progressReceiver.UpdateProgress(
				    (int) (100D * ((double) blockstatesProcessed / (double) totalStates)), null, kv.Key);

			    var state = kv.Value;
			    
			    foreach (var variant in state.Variants)
			    {
				    foreach (var va in variant.Value)
				    {
					    ProcessBlockStateModel(resourcePack, textures, va);
				    }
			    }


			    foreach (var part in state.Parts)
			    {
				    if (part.Apply == null)
					    continue;

				    foreach (var applied in part.Apply)
				    {
					    ProcessBlockStateModel(resourcePack, textures, applied);
				    }
			    }

			    blockstatesProcessed++;
			    // state.
		    }*/
	    }

	    private void ProcessBlockStateModel(McResourcePack resourcePack, Dictionary<ResourceLocation, ImageEntry> textures, BlockStateModel bsModel)
	    {
		    var model = bsModel.Model;

		    foreach (var texture in model.Textures)
		    {
			    var text = texture.Value;

			    if (text[0] == '#')
			    {
				    var substr = text.Substring(1);

				    if (model.Textures.TryGetValue(substr, out var p))
				    {
					    text = p;
				    }
				    else
				    {
					    var parent = model.Parent;

					    while (parent != null)
					    {
						    if (parent.Textures.TryGetValue(substr, out string parentName))
						    {
							    text = parentName;

							    break;
						    }

						    parent = parent.Parent;
					    }
				    }
			    }

			    var key = new ResourceLocation(text);

			    var alreadyLoaded = textures.ContainsKey(key);
			    if (!alreadyLoaded && resourcePack.TryGetBitmap(key, out var bmp))
			    {
				    TextureMeta meta = null;
				    resourcePack.TryGetTextureMeta(key, out meta);
				    
				    textures.TryAdd(key, new ImageEntry(bmp, meta));
			    }
			    else if (!alreadyLoaded)
			    {
				    if (texture.Value[0] != '#' || text[0] != '#')
					    Log.Warn(
						    $"Could not get bitmap {texture.Value} or {text} (Key: {texture.Key} Model: {bsModel.ModelName})");
			    }
		    }
	    }

	    public void LoadResourcePack(GraphicsDevice graphicsDevice,
		    McResourcePack resourcePack,
		    IProgressReceiver progressReceiver)
	    {
		    Dictionary<ResourceLocation, ImageEntry>   textures = new Dictionary<ResourceLocation, ImageEntry>();

		    GetTextures(resourcePack, textures, progressReceiver);

		    GenerateAtlas(graphicsDevice, textures, progressReceiver);
	    }

	    private class ImageEntry
	    {
		    public Image<Rgba32> Image { get; }
		    public TextureMeta Meta { get; }

		    public ImageEntry(Image<Rgba32> image, TextureMeta meta)
		    {
			    Image = image;
			    Meta = meta;
		    }
	    }

	    private class MipMappedImage
	    {
		    private Image<Rgba32>[] _levels;
		    public  int             Levels => _levels.Length;
		    public MipMappedImage(Image<Rgba32>[] levels)
		    {
			    _levels = levels;
		    }

		    public Image<Rgba32> GetLevel(int level)
		    {
			    return _levels[level];
		    }
	    }
	    
	    private void GenerateAtlas(GraphicsDevice device,
		    IDictionary<ResourceLocation, ImageEntry> blockTextures,
		    IProgressReceiver progressReceiver)
	    {
		    Stopwatch sw = Stopwatch.StartNew();

		    Log.Info($"Generating texture atlas out of {(blockTextures.Count)} bitmaps...");

		    long totalSize = 0;

		    Image<Rgba32> no;

		    using (MemoryStream ms = new MemoryStream(ResourceManager.ReadResource("Alex.Resources.no.png")))
		    {
			    no = Image.Load<Rgba32>(ms);
		    }

		    Dictionary<ResourceLocation, TextureInfo> stillFrameInfo = new Dictionary<ResourceLocation, TextureInfo>();

		    GenerateAtlasInternal(
			    new[] {new KeyValuePair<ResourceLocation, Image<Rgba32>>("no_texture", no),}.Concat(
				    blockTextures
					   .Where(x => x.Value.Image.Height == TextureHeight && x.Value.Image.Width == TextureWidth).Select(
						    x => new KeyValuePair<ResourceLocation, Image<Rgba32>>(x.Key, x.Value.Image))).ToArray(),
			    progressReceiver, stillFrameInfo, false, out var stillAtlas);

		    AtlasSize = new Vector2(stillAtlas.Width, stillAtlas.Height);

		    //  totalSize += size;

		    _atlasLocations = stillFrameInfo;

		    var stillFrame = GetMipMappedTexture2D(device, stillAtlas);
		    totalSize += stillFrame.MemoryUsage;
		    _stillFrame = stillFrame;
			    
		    _frames = ProcessFrames(device,
			    progressReceiver, blockTextures);

		    totalSize += _frames.Sum(x => x.MemoryUsage);
		    
		    sw.Stop();
		    
		    Log.Info(
			    $"TextureAtlas's generated in {sw.ElapsedMilliseconds}ms! ({PlayingState.GetBytesReadable(totalSize, 2)})");
	    }
	    
	    private PooledTexture2D[] ProcessFrames(GraphicsDevice device, IProgressReceiver progressReceiver,
		    IDictionary<ResourceLocation, ImageEntry> blockTextures)
	    {
		    Dictionary<ResourceLocation, Image<Rgba32>[]> blockFrames =
			    new Dictionary<ResourceLocation, Image<Rgba32>[]>();
		    
		    foreach (var other in blockTextures.Where(
			    x => x.Value.Image.Height != x.Value.Image.Width))
		    {
			    if (!blockFrames.TryGetValue(other.Key, out _))
			    {
				    blockFrames.Add(other.Key, GetFrames(other.Value.Image, TextureWidth, TextureHeight));
			    }
		    }

		    var animatedFrameInfo = new Dictionary<ResourceLocation, TextureInfo>();
		    
		    GenerateAtlasInternal(
			    blockFrames.Select(x => new KeyValuePair<ResourceLocation, Image<Rgba32>>(x.Key, x.Value[0])).ToArray(),
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
			    double percentage = 100D * ((double) i / (double) frames.Length);
			    progressReceiver.UpdateProgress((int) percentage, $"Animating frame {i + 1} / {frames.Length}...");

			    var target = (i > 0 ? frames[i - 1] : animatedFrame).CloneAs<Rgba32>(); //new Bitmap(animatedFrame);

			    // System.Drawing.Rectangle destination;

			    foreach (var animated in blockFrames)
			    {
				    progressReceiver.UpdateProgress((int) percentage, null, animated.Key.ToString());

				    if (animatedFrameInfo.TryGetValue(animated.Key, out var textureInfo))
				    {
					    //((i % 3 == 0 ? i - 1 : i) / 6)

					    var destination = new System.Drawing.Rectangle(
						    (int) textureInfo.Position.X, (int) textureInfo.Position.Y, textureInfo.Width,
						    textureInfo.Height);

					    var sourceRegion = new System.Drawing.Rectangle(0, 0, textureInfo.Width, textureInfo.Height);

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
								    index = (i + indexOffset) % animated.Value.Length;
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

					    var texture = animated.Value[index];

					    TextureUtils.CopyRegionIntoImage(
						    texture, sourceRegion, ref target, destination, shouldInterpolate, interpolationValue,
						    clear: !shouldInterpolate);
				    }
			    }

			    frames[i] = target;
		    }
		    
		    return frames.Select(
			    (x, index) =>
			    {
				    var a = GetMipMappedTexture2D(device, x);
				    //totalSize += a.MemoryUsage;

				    return a;
			    }).ToArray();
	    }

	    private PooledTexture2D GetMipMappedTexture2D(GraphicsDevice device, Image<Rgba32> image)
	    {
		    //device.VertexSamplerStates.
		    PooledTexture2D texture = GpuResourceManager.GetTexture2D(this, device, image.Width, image.Height, true, SurfaceFormat.Color);

		    // Directory.CreateDirectory(name);
		    // var resampler = new BicubicResampler();
		    // var encoder = new PngEncoder();
		    for (int level = 0; level < Alex.MipMapLevel; level++)
		    {
			    int mipWidth  = (int) System.Math.Max(1, image.Width >> level);
			    int mipHeight = (int) System.Math.Max(1, image.Height >> level);
			    
			    var bmp = image.CloneAs<Rgba32>(); //.CloneAs<Rgba32>();
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
		    }

		    return texture;
	    }
	    
	    private void GenerateAtlasInternal(KeyValuePair<ResourceLocation, Image<Rgba32>>[] regular, IProgressReceiver progressReceiver, Dictionary<ResourceLocation, TextureInfo> atlasLocations, bool animated, out Image<Rgba32> result)
        {
	        int total = regular.Length;
	        var a = (int)Math.Ceiling(regular.Length / 32D);

	        int height = a * TextureHeight;

	        var bitmap = new Image<Rgba32>(Math.Min(32, total) * TextureWidth, height);

	        int xi = 0, yi = 0, offsetX = 0, yRemaining = 0;
	        Process(ref bitmap, regular, ref xi, ref yi, ref offsetX, ref yRemaining, total, 0, atlasLocations, progressReceiver, animated);

	        result = bitmap;
        }

	    private int Process(ref Image<Rgba32> target, KeyValuePair<ResourceLocation, Image<Rgba32>>[] data, ref int xi, ref int yi, ref int xOffset, ref int yRemaining, int total, int processed, IDictionary<ResourceLocation, TextureInfo> locations, IProgressReceiver progressReceiver, bool animated)
	    {
		    int done = processed;
			var count = 0;

		    foreach (var bm in data.OrderByDescending(x => x.Value.Height))
		    {
			    var key = bm.Key;
			    count++;

			    double percentage = 100D * ((double)processed / (double)total);
			    progressReceiver.UpdateProgress((int)percentage, null, key.ToString());

                var sourceRegion = new System.Drawing.Rectangle(0, 0, bm.Value.Width, bm.Value.Height);
			    var targetRegion = new System.Drawing.Rectangle(xi, yi, bm.Value.Width, bm.Value.Height);

			    TextureUtils.CopyRegionIntoImage(bm.Value, sourceRegion, ref target, targetRegion);

			    if (!locations.ContainsKey(key)) 
			    {
				    locations.Add(key, new TextureInfo(new Vector2(xi, yi), bm.Value.Width, bm.Value.Height, animated, true));
			    }

			    if (bm.Value.Height > TextureHeight)
			    {
				    xOffset = xi;
				    yRemaining = bm.Value.Height;
			    }
			    xi += bm.Value.Width;

			    if (count == target.Width / TextureWidth)
			    {
				    yi += TextureHeight;
				    xi = 0;
				    yRemaining -= TextureHeight;

				    count = 0;
			    }

			    done++;

			   
			}

		    return done;
	    }

	    private Image<Rgba32>[] GetFrames(Image<Rgba32> source, int frameWidth, int frameHeight)
	    {
		    int framesInWidth = source.Width / frameWidth;
		    int framesInHeight = source.Height / frameHeight;

		    if (framesInWidth * framesInHeight == 0)
		    {
			//    string a = "b";
		    }
		    
		    //List<Image<Rgba32>> result = new List<Image<Rgba32>>();
		    Image<Rgba32>[] result  = new Image<Rgba32>[framesInHeight * framesInWidth];
		    int             counter = 0;
		    for (int x = 0; x < framesInWidth; x++)
		    for (int y = 0; y < framesInHeight; y++)
		    {
			    var newBitmap = new Image<Rgba32>(frameWidth, frameHeight);

			    TextureUtils.CopyRegionIntoImage(
				    source,
				    new System.Drawing.Rectangle(x * frameWidth, y * frameHeight, frameWidth, frameHeight),
				    ref newBitmap, new System.Drawing.Rectangle(0, 0, frameWidth, frameHeight));

			    result[counter++] = newBitmap;
			    //result.Add(newBitmap);
		    }

		    return result.ToArray();
	    }

	    public int TextureWidth { get; private set; } = 16;
	    public int TextureHeight { get; private set; }= 16;

        public void LoadResourcePackOnTop(GraphicsDevice device, McResourcePack vanilla, McResourcePack resourcePack, IProgressReceiver progressReceiver)
		{

            int textureWidth = 16, textureHeight = 16;
            
			Dictionary<ResourceLocation, Image<Rgba32>>   vanillaTextures = new Dictionary<ResourceLocation, Image<Rgba32>>();

			//GetTextures(vanilla, vanillaTextures, progressReceiver);
			
			Dictionary<ResourceLocation, Image<Rgba32>>   textures = new Dictionary<ResourceLocation, Image<Rgba32>>();

			//GetTextures(resourcePack, textures, progressReceiver);

			foreach (var image in vanillaTextures.ToArray())
			{
				if (!textures.ContainsKey(image.Key))
				{
					textures.Add(image.Key, image.Value);
				}

				var texture = image.Value;
				if (texture.Width > textureWidth && texture.Width % 16 == 0 && texture.Height > textureHeight &&
				    texture.Height % 16 == 0)
				{
					if (texture.Width == texture.Height)
					{
						textureWidth = texture.Width;
						textureHeight = texture.Height;
					}
				}
			}
			
			var a     = (int)Math.Ceiling(textures.Count / 32D);
			int height = a * TextureHeight;
			
			AtlasSize = new Vector2(Math.Min(32, textures.Count), height);
            TextureHeight = textureHeight;
            TextureWidth = textureWidth;

          //  GenerateAtlas(device, textures, progressReceiver);
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
			ResourceLocation file,
			out Vector2 atlasSize)
		{
		/*	if (dictionary == null)
			{
				atlasSize = block.Animated ? AnimatedAtlasSize : AtlasSize;
				dictionary = block.Animated ? _animatedAtlasLocations : _atlasLocations;
			}
			else
			{
				atlasSize = AtlasSize;
			}

			if (dictionary.Count == 0) throw new Exception();*/

			if (_animatedAtlasLocations.TryGetValue(file, out var textureInfo))
			{
				atlasSize = AnimatedAtlasSize;

				return textureInfo;
			}

			if (_atlasLocations.TryGetValue(file, out var atlasInfo))
			{
				atlasSize = AtlasSize;

				return atlasInfo;
			}

			if (file.Path.Contains("fire_"))
			{
				string a = "b";
			}
			
			atlasSize = AtlasSize;
			return new TextureInfo(Vector2.Zero, TextureWidth, TextureHeight, false, false);
		}
    }
}
