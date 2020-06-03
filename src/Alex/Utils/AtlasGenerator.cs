using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Alex.API.Utils;
using Alex.Gamestates.InGame;
using Alex.ResourcePackLib.Json.Textures;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Alex.Utils
{
    public class AtlasGenerator
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));

	    private Dictionary<string, TextureInfo> _atlasLocations = new Dictionary<string, TextureInfo>();
	    private Dictionary<string, TextureInfo> _animatedAtlasLocations = new Dictionary<string, TextureInfo>();
	    
	    private Texture2D[] _frames;
	    private Texture2D _stillFrame;
        public Vector2 AtlasSize { get; private set; }
        public Vector2 AnimatedAtlasSize { get; private set; }
        
        public AtlasGenerator()
	    {

	    }

	    public void Reset()
	    {
		    _atlasLocations = new Dictionary<string, TextureInfo>();
		    _animatedAtlasLocations = new Dictionary<string, TextureInfo>();

		    AtlasSize = default;
		    _frames = default;
		    _stillFrame = default;
	    }

	    public void GenerateAtlas(GraphicsDevice device, KeyValuePair<string, Image<Rgba32>>[] bitmaps, IReadOnlyDictionary<string, TextureMeta> meta, IProgressReceiver progressReceiver)
	    {
		    Stopwatch sw = Stopwatch.StartNew();
		    
		    Log.Info($"Generating texture atlas out of {bitmaps.Length} bitmaps...");
		    
		    long totalSize = 0;
		    
		    Image<Rgba32> no;
		    using (MemoryStream ms = new MemoryStream(ResourceManager.ReadResource("Alex.Resources.no.png")))
		    {
			    no = Image.Load<Rgba32>(ms);
		    }

		  //  Dictionary<string, Bitmap[]> animatedFrames = new Dictionary<string, Bitmap[]>();
		    foreach (var bmp in bitmaps)
		    {
			    if (meta.TryGetValue(bmp.Key, out var textureMeta))
			    {
				    if (textureMeta.Animation == null || textureMeta.Animation == default)
					    continue;

				   // Bitmap[] bmpFrames = GetFrames(bmp.Value);
				 //   animatedFrames.Add(bmp.Key, bmpFrames);
			    }
		    }
		    
		    var regular = new[]
		    {
			    new KeyValuePair<string, Image<Rgba32>>("no_texture", no),
		    }.Concat(bitmaps.Where(x => x.Value.Height == TextureHeight && x.Value.Width == TextureWidth)).ToArray();
		       
		    var others = bitmaps.Where(x => x.Value.Height != TextureHeight || x.Value.Width != TextureWidth).ToList();
		    
		    Image<Rgba32>[] waterFrames = new Image<Rgba32>[0];
		    Image<Rgba32>[] lavaFrames = new Image<Rgba32>[0];
		    Image<Rgba32>[] waterFlowFrames = new Image<Rgba32>[0];
		    Image<Rgba32>[] lavaFlowFrames = new Image<Rgba32>[0];
		    Image<Rgba32>[] fireFrames = new Image<Rgba32>[0];
		    Image<Rgba32>[] fireFrames2 = new Image<Rgba32>[0];
		    Image<Rgba32>[] portalFrames = new Image<Rgba32>[0];
	        Image<Rgba32>[] seagrassFrames = new Image<Rgba32>[0];
	        
		    foreach (var other in others.ToArray())
		    {
			    if (other.Key.Contains("water") && other.Key.Contains("still"))
			    {
				    waterFrames = GetFrames(other.Value);
				    others.Remove(other);
			    }
			    else if (other.Key.Contains("water") && other.Key.Contains("flow"))
			    {
				    waterFlowFrames = GetFrames(other.Value);
				    others.Remove(other);
			    }
			    else if (other.Key.Contains("lava") && other.Key.Contains("still"))
			    {
				    lavaFrames = GetFrames(other.Value);
				    others.Remove(other);
			    }
			    else if (other.Key.Contains("lava") && other.Key.Contains("flow"))
			    {
				    lavaFlowFrames = GetFrames(other.Value);
				    others.Remove(other);
			    }
			    else if (other.Key.Contains("fire_0"))
			    {
				    fireFrames = GetFrames(other.Value);
				    others.Remove(other);
			    }
			    else if (other.Key.Contains("fire_1"))
			    {
				    fireFrames2 = GetFrames(other.Value);
				    others.Remove(other);
			    }
			    else if (other.Key.Contains("nether_portal"))
			    {
				    portalFrames = GetFrames(other.Value);
				    others.Remove(other);
			    }
			    else if (other.Key.Contains("seagrass"))
			    {
				    seagrassFrames = GetFrames(other.Value);
				    others.Remove(other);
			    }
			    //seagrass
		    }
		    
		    Dictionary<string, TextureInfo> stillFrameInfo = new Dictionary<string, TextureInfo>();
		    GenerateAtlasInternal(regular, others.ToArray(), progressReceiver, stillFrameInfo, out var stillAtlas);
		    _stillFrame = TextureUtils.BitmapToTexture2D(device, stillAtlas, out var size);
		    
		    //stillAtlas.Save(Path.Combine(DebugPath, "atlas.png"));
		    
		    totalSize += size;
		    
		    _atlasLocations = stillFrameInfo;
		    
		    Dictionary<string, Image<Rgba32>> animated = new Dictionary<string, Image<Rgba32>>();
		    
		    if (waterFrames.Length > 0)
			    animated.Add("block/water_still", waterFrames[0]);
	        

	        if (waterFlowFrames.Length > 0)
		        animated.Add("block/water_flow", waterFlowFrames[0]);
	        
	        
	        if (lavaFrames.Length > 0)
		        animated.Add("block/lava_still", lavaFrames[0]);
	        
	        
	        if (lavaFlowFrames.Length > 0)
		        animated.Add("block/lava_flow", lavaFlowFrames[0]);

	        if (fireFrames.Length > 0)
		        animated.Add("block/fire_0", fireFrames[0]);

	        if (fireFrames2.Length > 0)
		        animated.Add("block/fire_1", fireFrames2[0]);

	        if (portalFrames.Length > 0)
		        animated.Add("block/nether_portal", portalFrames[0]);
	        
	        if (seagrassFrames.Length > 0)
		        animated.Add("block/seagrass", seagrassFrames[0]);
	        
	        var animatedFrameInfo = new Dictionary<string, TextureInfo>();
	        GenerateAtlasInternal(animated.ToArray(), new KeyValuePair<string, Image<Rgba32>>[0], progressReceiver,
		        animatedFrameInfo, out Image<Rgba32> animatedFrame);

	        AnimatedAtlasSize = new Vector2(animatedFrame.Width, animatedFrame.Height);
	        
	        TextureInfo waterLocation, waterFlowLocation, lavaLocation, lavaFlowLocation, fireLocation, fireLocation2, portalLocation, seagrassLocation;

	        animatedFrameInfo.TryGetValue("block/water_still", out waterLocation);
	        animatedFrameInfo.TryGetValue("block/water_flow", out waterFlowLocation);
	        animatedFrameInfo.TryGetValue("block/lava_still", out lavaLocation);
	        animatedFrameInfo.TryGetValue("block/lava_flow", out lavaFlowLocation);
	        animatedFrameInfo.TryGetValue("block/fire_0", out fireLocation);
	        animatedFrameInfo.TryGetValue("block/fire_1", out fireLocation2);
	        animatedFrameInfo.TryGetValue("block/nether_portal", out portalLocation);
	        animatedFrameInfo.TryGetValue("block/seagrass", out seagrassLocation);
	        
	        //var waterLocation = new Vector3();
		    
		   // var baseBitmap = new Bitmap(stillAtlas.Width, stillAtlas.Height);
		   var frameCount = Math.Max(Math.Max(Math.Max(waterFrames.Length,
			   Math.Max(waterFlowFrames.Length,
				   Math.Max(lavaFrames.Length, Math.Max(lavaFlowFrames.Length, fireFrames.Length)))), portalFrames.Length), seagrassFrames.Length);

		   while (frameCount % 2 != 0)
		   {
			   frameCount++;
		   }
		   
		   var frames = new Texture2D[frameCount];
		   
		    for (int i = 0; i < frames.Length; i++)
		    {
			    var target = animatedFrame.CloneAs<Rgba32>(); //new Bitmap(animatedFrame);
			    var r = new System.Drawing.Rectangle(0, 0, TextureWidth, TextureHeight);
			    var destination = new System.Drawing.Rectangle((int) waterLocation.Position.X, (int) waterLocation.Position.Y, TextureWidth, TextureHeight);

			    if (waterFrames.Length > 0)
				    TextureUtils.CopyRegionIntoImage(waterFrames[((i % 3 == 0 ? i -1 : i) / 6) % waterFrames.Length], r, ref target, destination);
				
			    destination = new System.Drawing.Rectangle((int) waterFlowLocation.Position.X, (int) waterFlowLocation.Position.Y, TextureWidth, TextureHeight);
			    if (waterFlowFrames.Length > 0)
				    TextureUtils.CopyRegionIntoImage(waterFlowFrames[i % waterFlowFrames.Length], r, ref target, destination);
				
			    destination = new System.Drawing.Rectangle((int) lavaLocation.Position.X, (int) lavaLocation.Position.Y, TextureWidth, TextureHeight);
			    if (lavaFrames.Length > 0)
				    TextureUtils.CopyRegionIntoImage(lavaFrames[i % lavaFrames.Length], r, ref target, destination);
				
			    destination = new System.Drawing.Rectangle((int) lavaFlowLocation.Position.X, (int) lavaFlowLocation.Position.Y, TextureWidth, TextureHeight);
			    if (lavaFlowFrames.Length > 0)
				    TextureUtils.CopyRegionIntoImage(lavaFlowFrames[i % lavaFlowFrames.Length], r, ref target, destination);
			    
			    destination = new System.Drawing.Rectangle((int) fireLocation.Position.X, (int) fireLocation.Position.Y, TextureWidth, TextureHeight);
			    if (fireFrames.Length > 0)
				    TextureUtils.CopyRegionIntoImage(fireFrames[i % fireFrames.Length], r, ref target, destination);
			    
			    destination = new System.Drawing.Rectangle((int) fireLocation2.Position.X, (int) fireLocation2.Position.Y, TextureWidth, TextureHeight);
			    if (fireFrames2.Length > 0)
				    TextureUtils.CopyRegionIntoImage(fireFrames2[i % fireFrames2.Length], r, ref target, destination);
			    
			    destination = new System.Drawing.Rectangle((int) portalLocation.Position.X, (int) portalLocation.Position.Y, TextureWidth, TextureHeight);
			    if (portalFrames.Length > 0)
				    TextureUtils.CopyRegionIntoImage(portalFrames[i % portalFrames.Length], r, ref target, destination);
			    
			    destination = new System.Drawing.Rectangle((int) seagrassLocation.Position.X, (int) seagrassLocation.Position.Y, TextureWidth, TextureHeight);
			    if (seagrassFrames.Length > 0)
				    TextureUtils.CopyRegionIntoImage(seagrassFrames[i % seagrassFrames.Length], r, ref target, destination);
				
			    frames[i] = TextureUtils.BitmapToTexture2D(device, target, out var s);
			    totalSize += s;
			    
			 //   target.Save(Path.Combine(DebugFramePath, $"frame{i}.png"));
		    }

		    _animatedAtlasLocations = animatedFrameInfo;
		    
		    AtlasSize = new Vector2(stillAtlas.Width, stillAtlas.Height);
		    _frames = frames;
		    
		    sw.Stop();
		    
		    Log.Info($"TextureAtlas generated in {sw.ElapsedMilliseconds}ms! ({PlayingState.GetBytesReadable(totalSize, 2)})");
	    }
	    
	    private void GenerateAtlasInternal(KeyValuePair<string, Image<Rgba32>>[] regular, KeyValuePair<string, Image<Rgba32>>[] others, IProgressReceiver progressReceiver, Dictionary<string, TextureInfo> atlasLocations, out Image<Rgba32> result)
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
	        int processedFiles = Process(ref bitmap, regular, ref xi, ref yi, ref offsetX, ref yRemaining, total, 0, atlasLocations, progressReceiver);
	        yi += TextureHeight;
	        xi = 0;
			Process(ref bitmap, others.ToArray(), ref xi, ref yi, ref offsetX, ref yRemaining, total, processedFiles, atlasLocations, progressReceiver);

			result = bitmap;
        }

	    private int Process(ref Image<Rgba32> bmp, KeyValuePair<string, Image<Rgba32>>[] data, ref int xi, ref int yi, ref int xOffset, ref int yRemaining, int total, int processed, IDictionary<string, TextureInfo> locations, IProgressReceiver progressReceiver)
	    {
		    int done = processed;
			var count = 0;

		    foreach (var bm in data.OrderByDescending(x => x.Value.Height))
		    {
			    string key = bm.Key;
			    count++;

			    double percentage = 100D * ((double)processed / (double)total);
			    progressReceiver.UpdateProgress((int)percentage, null, key);

                var sourceRegion = new System.Drawing.Rectangle(0, 0, bm.Value.Width, bm.Value.Height);
			    var targetRegion = new System.Drawing.Rectangle(xi, yi, bm.Value.Width, bm.Value.Height);

			    TextureUtils.CopyRegionIntoImage(bm.Value, sourceRegion, ref bmp, targetRegion);

			    if (!locations.ContainsKey(key))
			    {
				    locations.Add(key, new TextureInfo(new Vector2(xi, yi), bm.Value.Width, bm.Value.Height));
			    }

			    if (bm.Value.Height > TextureHeight)
			    {
				    xOffset = xi;
				    yRemaining = bm.Value.Height;
			    }
			    xi += bm.Value.Width;

			    if (count == bmp.Width / TextureWidth)
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

	    private Image<Rgba32>[] GetFrames(Image<Rgba32> source)
	    {
		    int ix = source.Width / TextureWidth;
		    int iy = source.Height / TextureHeight;

		    List<Image<Rgba32>> result = new List<Image<Rgba32>>();
		    
		    for (int x = 0; x < ix; x++)
		    for(int y = 0; y < iy; y++)
		    {
			    var newBitmap = new Image<Rgba32>(TextureWidth, TextureHeight);
			    TextureUtils.CopyRegionIntoImage(source, new System.Drawing.Rectangle(x * TextureWidth,y * TextureHeight, TextureWidth, TextureHeight), ref newBitmap, new System.Drawing.Rectangle(0, 0, TextureWidth, TextureHeight));
			    
			    result.Add(newBitmap);
		    }

		    return result.ToArray();
	    }

	    private int AtlasWidth = 512;
	    private int AtlasHeight = 512;

	    public int TextureWidth { get; private set; } = 16;
	    public int TextureHeight { get; private set; }= 16;

        public void LoadResourcePackOnTop(GraphicsDevice device, KeyValuePair<string, Image<Rgba32>>[] vanilla, KeyValuePair<string, Image<Rgba32>>[] bitmapsRaw, IReadOnlyDictionary<string, TextureMeta> meta, IProgressReceiver progressReceiver)
		{

            int textureWidth = 16, textureHeight = 16;
			Dictionary<string, Image<Rgba32>> bitmaps = new Dictionary<string, Image<Rgba32>>();
            foreach (var bmp in vanilla)
            {
                string name = bmp.Key;
                if (!bitmaps.ContainsKey(name))
                {
                    var bitmap = bitmapsRaw.FirstOrDefault(x =>
                        x.Key.Equals(bmp.Key, StringComparison.InvariantCultureIgnoreCase));
                    if (bitmap.Value == null) bitmap = bmp;

                    var texture = bitmap.Value;
                    if (texture.Width > textureWidth && texture.Width % 16 == 0 && texture.Height > textureHeight &&
                        texture.Height % 16 == 0)
                    {
                        if (texture.Width == texture.Height)
                        {
                            textureWidth = texture.Width;
                            textureHeight = texture.Height;
                        }
                    }

                    bitmaps.Add(name, texture);
                }
            }

            AtlasWidth = 32 * textureWidth;
            AtlasHeight = 32 * textureHeight;

            AtlasSize = new Vector2(AtlasWidth, AtlasHeight);
            TextureHeight = textureHeight;
            TextureWidth = textureWidth;

            GenerateAtlas(device, bitmaps.ToArray(), meta, progressReceiver);
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

		public TextureInfo GetAtlasLocation(string file, out Vector2 atlasSize, IDictionary<string, TextureInfo> dictionary = null)
        {
            if (dictionary == null)
                dictionary = _atlasLocations;

		    if (dictionary.Count == 0) throw new Exception();

		    if (file == "water_still" && !dictionary.ContainsKey(file))
			    file = "water_flow";

		    atlasSize = AtlasSize;
		    
		    if (dictionary.ContainsKey(file))
		    {
			    return dictionary[file];
		    }
		    else
		    {
			    if (_animatedAtlasLocations.TryGetValue(file, out var textureInfo))
			    {
				    atlasSize = AnimatedAtlasSize;
				    return textureInfo;
			    }
			    
			    return new TextureInfo(Vector2.Zero, TextureWidth, TextureHeight);
		    }
	    }
	}
}
