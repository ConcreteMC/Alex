using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Alex.API.Resources;
using Alex.API.Utils;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Gamestates.InGame;
using Alex.Graphics.Models.Blocks;
using Alex.ResourcePackLib;
using Alex.Utils;
using Alex.Worlds.Singleplayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Alex.Graphics
{
    public class AtlasGenerator
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));

	    private Dictionary<ResourceLocation, TextureInfo> _atlasLocations = new Dictionary<ResourceLocation, TextureInfo>();
	    private Dictionary<ResourceLocation, TextureInfo> _animatedAtlasLocations = new Dictionary<ResourceLocation, TextureInfo>();
	    
	    private Texture2D[] _frames;
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

	    private Vector2 GetTextureDimensions(McResourcePack resourcePack, ResourceLocation location)
	    {
		    Vector2 size = new Vector2(TextureWidth, TextureHeight);

		    return size;
		    foreach (var state in resourcePack.BlockModels)
		    {
			    foreach (var element in state.Value.Elements)
			    foreach (var face in element.Faces)
			    {
				    var val = face.Value;
				    if (ResourcePackBlockModel.ResolveTexture(state.Value, val.Texture) == location)
				    {
					    var width = val.UV.X2 - val.UV.X1;
					    var height = val.UV.Y2 - val.UV.Y1;

					    if (width > size.X)
					    {
						    size.X = width;
					    }

					    if (height > size.Y)
					    {
						    size.Y = height;
					    }
				    }
			    }
		    }

		    return size;
	    }
	    
	    private void GetTextures(McResourcePack resourcePack,
		    Dictionary<ResourceLocation, Image<Rgba32>[]> seperateFrames,
		    Dictionary<ResourceLocation, Image<Rgba32>> regularTextures, IProgressReceiver progressReceiver)
	    {
		    progressReceiver.UpdateProgress(0, "Processing blockstate textures...");
			int blockstatesProcessed = 0;
			int totalStates = resourcePack.BlockStates.Count;
			foreach (var state in resourcePack.BlockStates)
		    {
			    progressReceiver.UpdateProgress((int) (100D * ((double)blockstatesProcessed / (double)totalStates)), null, state.Key);
			    
			    var blockState = BlockFactory.GetBlockState(state.Key);

			    if (blockState.Name == "Unknown")
			    {
				    Log.Warn($"Missing blockstate: {state.Key}");
				    blockstatesProcessed++;
				    continue;
			    }

			    foreach (var variant in state.Value.Variants)
			    {
				    foreach (var entry in BlockState.ParseData(variant.Key))
				    {
					    blockState = blockState.WithProperty(entry.Key, entry.Value);
				    }
				    
				    foreach (var va in variant.Value)
				    {
					    var model = va.Model;
					    
					    foreach (var texture in model.Textures)
					    //foreach(var element in model.Elements)
					    //foreach(var face in element.Faces)
					    {
						  //  var text = face.Value.Texture; //ResourcePackBlockModel.ResolveTexture(model, texture.Value);
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
									    if (parent == null) break;

									    if (parent.Textures.TryGetValue(substr, out string parentName))
									    {
										    text = parentName;

										    break;
									    }

									    parent = parent.Parent;
								    }
							    }
						    }
						    
						    if (resourcePack.TryGetBitmap(text, out var bmp))
						    {
							    if (blockState.Block.Animated && !seperateFrames.ContainsKey(text))
							    {
								    var frameSize = GetTextureDimensions(resourcePack, text);
								    seperateFrames.Add(text, GetFrames(bmp, (int) frameSize.X, (int) frameSize.Y));
							    }
							    else if (!blockState.Block.Animated
							             && regularTextures.All(x => x.Key != text))
							    {
								    regularTextures.Add(text, bmp);
							    }
						    }
						    else
						    {
							   /// if (!texture.Value)
								//	Log.Warn($"Could not get bitmap {texture.Value} or {text} (Key: {texture.Key} Model: {model.Name})");
						    }
					    }
				    }
			    }

			    blockstatesProcessed++;
			    // state.
		    }

			if (resourcePack.TryGetBitmap("block/water_flow", out var flow))
			{
				var frameSize = GetTextureDimensions(resourcePack, "block/water_flow");
				seperateFrames.TryAdd("block/water_flow", GetFrames(flow, (int) frameSize.X, (int) frameSize.Y));
			}
			
			if (resourcePack.TryGetBitmap("block/water_still", out var still))
			{
				var frameSize = GetTextureDimensions(resourcePack, "block/water_still");
				seperateFrames.TryAdd("block/water_still", GetFrames(still, (int) frameSize.X, (int) frameSize.Y));
			}
			
			if (resourcePack.TryGetBitmap("block/lava_flow", out var lavaFlow))
			{
				var frameSize = GetTextureDimensions(resourcePack, "block/lava_flow");
				seperateFrames.TryAdd("block/lava_flow", GetFrames(lavaFlow, (int) frameSize.X, (int) frameSize.Y));
			}
			
			if (resourcePack.TryGetBitmap("block/lava_still", out var lavastill))
			{
				var frameSize = GetTextureDimensions(resourcePack, "block/lava_still");
				seperateFrames.TryAdd("block/lava_still", GetFrames(lavastill, (int) frameSize.X, (int) frameSize.Y));
			}
	    }

	    public void LoadResourcePack(GraphicsDevice graphicsDevice,
		    McResourcePack resourcePack,
		    IProgressReceiver progressReceiver)
	    {
		    Dictionary<ResourceLocation, Image<Rgba32>[]> frames  = new Dictionary<ResourceLocation, Image<Rgba32>[]>();
		    Dictionary<ResourceLocation, Image<Rgba32>>   textures = new Dictionary<ResourceLocation, Image<Rgba32>>();

		    GetTextures(resourcePack, frames, textures, progressReceiver);

		    GenerateAtlas(graphicsDevice, frames, textures, progressReceiver);

		    string a = "b";
	    }
	    
	    private void GenerateAtlas(GraphicsDevice device, IDictionary<ResourceLocation, Image<Rgba32>[]> blockFrames, IDictionary<ResourceLocation, Image<Rgba32>> blockTextures, IProgressReceiver progressReceiver)
	    {
		    Stopwatch sw = Stopwatch.StartNew();

		    Log.Info($"Generating texture atlas out of {(blockFrames.Count + blockTextures.Count)} bitmaps...");
		    
		    long totalSize = 0;
		    
		    Image<Rgba32> no;
		    using (MemoryStream ms = new MemoryStream(ResourceManager.ReadResource("Alex.Resources.no.png")))
		    {
			    no = Image.Load<Rgba32>(ms);
		    }
		    
		    var regularTextures = new[]
		    {
			    new KeyValuePair<ResourceLocation, Image<Rgba32>>("no_texture", no),
		    }.Concat(blockTextures.Where(x => x.Value.Height == TextureHeight && x.Value.Width == TextureWidth)).ToArray();
		       
		    var oddSizedTextures = blockTextures.Where(x => x.Value.Height != TextureHeight || x.Value.Width != TextureWidth).ToArray();

		    for (var index = 0; index < oddSizedTextures.Length; index++)
		    {
			    var other = oddSizedTextures[index];

			    if (blockFrames.TryGetValue(other.Key, out var values))
			    {
				    oddSizedTextures[index] = new KeyValuePair<ResourceLocation, Image<Rgba32>>(other.Key, values[0]);
			    }
		    }

		    Dictionary<ResourceLocation, TextureInfo> stillFrameInfo = new Dictionary<ResourceLocation, TextureInfo>();
		    GenerateAtlasInternal(regularTextures, oddSizedTextures, progressReceiver, stillFrameInfo, false, out var stillAtlas);
		    _stillFrame = TextureUtils.BitmapToTexture2D(device, stillAtlas, out var size);
		    AtlasSize = new Vector2(stillAtlas.Width, stillAtlas.Height);
		    
		    totalSize += size;
		    
		    _atlasLocations = stillFrameInfo;

		    var animatedFrameInfo = new Dictionary<ResourceLocation, TextureInfo>();
	        GenerateAtlasInternal(blockFrames.Select(x => new KeyValuePair<ResourceLocation,Image<Rgba32>>(x.Key, x.Value[0])).ToArray(), new KeyValuePair<ResourceLocation, Image<Rgba32>>[0], progressReceiver,
		        animatedFrameInfo, true, out Image<Rgba32> animatedFrame);

	        AnimatedAtlasSize = new Vector2(animatedFrame.Width, animatedFrame.Height);
	        _animatedAtlasLocations = animatedFrameInfo;
	        
	        
	        var frameCount = blockFrames.Max(x => x.Value.Length);
		   while (frameCount % 2 != 0)
		   {
			   frameCount++;
		   }
		   
		   var frames = new Texture2D[frameCount];
		   
		    for (int i = 0; i < frames.Length; i++)
		    {
			    double percentage = 100D * ((double)i / (double)frames.Length);
			    progressReceiver.UpdateProgress((int)percentage, $"Animating frame {i}...");
			    
			    var target = animatedFrame.CloneAs<Rgba32>(); //new Bitmap(animatedFrame);

			    // System.Drawing.Rectangle destination;

			    foreach (var animated in blockFrames)
			    {
				    progressReceiver.UpdateProgress((int)percentage, null, animated.Key.ToString());

				    if (animatedFrameInfo.TryGetValue(animated.Key, out var textureInfo))
				    {
					    //((i % 3 == 0 ? i - 1 : i) / 6)
					    
					    var destination = new System.Drawing.Rectangle(
						    (int) textureInfo.Position.X, (int) textureInfo.Position.Y, textureInfo.Width, textureInfo.Height);

					    TextureUtils.CopyRegionIntoImage(
						    animated.Value[i % animated.Value.Length], new System.Drawing.Rectangle(0, 0, textureInfo.Width, textureInfo.Height), ref target,
						    destination);
				    }
			    }
			   
			    frames[i] = TextureUtils.BitmapToTexture2D(device, target, out var s);
			    totalSize += s;
		    }
		    
		    _frames = frames;
		    
		    sw.Stop();
		    
		    Log.Info($"TextureAtlas generated in {sw.ElapsedMilliseconds}ms! ({PlayingState.GetBytesReadable(totalSize, 2)})");
	    }
	    
	    private void GenerateAtlasInternal(KeyValuePair<ResourceLocation, Image<Rgba32>>[] regular, KeyValuePair<ResourceLocation, Image<Rgba32>>[] others, IProgressReceiver progressReceiver, Dictionary<ResourceLocation, TextureInfo> atlasLocations, bool animated, out Image<Rgba32> result)
        {
	        int total = regular.Length + others.Length;
	        var a = (int)Math.Ceiling(regular.Length / 32D);

	        int height = a * TextureHeight;
	        if (others.Length != 0)
	        {
		        height += others.Max(x => x.Value.Height);
	        }
	        
	        var bitmap = new Image<Rgba32>(Math.Min(32, total) * TextureWidth, height);

	        int xi = 0, yi = 0, offsetX = 0, yRemaining = 0;
	        int processedFiles = Process(ref bitmap, regular, ref xi, ref yi, ref offsetX, ref yRemaining, total, 0, atlasLocations, progressReceiver, animated);
	        yi += TextureHeight;
	        xi = 0;
			Process(ref bitmap, others.ToArray(), ref xi, ref yi, ref offsetX, ref yRemaining, total, processedFiles, atlasLocations, progressReceiver, animated);

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
				    locations.Add(key, new TextureInfo(new Vector2(xi, yi), bm.Value.Width, bm.Value.Height, animated));
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

		    //List<Image<Rgba32>> result = new List<Image<Rgba32>>();
		    Image<Rgba32>[] result = new Image<Rgba32>[framesInHeight * framesInWidth];
		    int counter = 0;
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

			Dictionary<ResourceLocation, Image<Rgba32>[]> vanillaFrames  = new Dictionary<ResourceLocation, Image<Rgba32>[]>();
			Dictionary<ResourceLocation, Image<Rgba32>>   vanillaTextures = new Dictionary<ResourceLocation, Image<Rgba32>>();

			GetTextures(vanilla, vanillaFrames, vanillaTextures, progressReceiver);
			
			Dictionary<ResourceLocation, Image<Rgba32>[]> frames   = new Dictionary<ResourceLocation, Image<Rgba32>[]>();
			Dictionary<ResourceLocation, Image<Rgba32>>   textures = new Dictionary<ResourceLocation, Image<Rgba32>>();

			GetTextures(resourcePack, frames, textures, progressReceiver);

			foreach (var image in vanillaFrames.ToArray())
			{
				if (!frames.ContainsKey(image.Key))
				{
					frames.Add(image.Key, image.Value);
				}
			}

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

            GenerateAtlas(device, frames, textures, progressReceiver);
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

		public TextureInfo GetAtlasLocation(Block block,
			ResourceLocation file,
			out Vector2 atlasSize,
			IDictionary<ResourceLocation, TextureInfo> dictionary = null)
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

			atlasSize = AtlasSize;
			return new TextureInfo(Vector2.Zero, TextureWidth, TextureHeight, false);
		}
    }
}
