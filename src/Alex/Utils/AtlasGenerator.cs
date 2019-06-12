using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Alex.API.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using Color = System.Drawing.Color;

namespace Alex.Utils
{
    public class AtlasGenerator
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));

	    private Dictionary<string, TextureInfo> _atlasLocations = new Dictionary<string, TextureInfo>();

	    private Texture2D[] _frames;
	    private Texture2D _atlas;
	    public Vector2 AtlasSize { get; private set; }
		public Bitmap Atlas { get; set; } = null;
	    private GraphicsDevice Graphics { get; }
	    public AtlasGenerator(GraphicsDevice graphics)
	    {
		    Graphics = graphics;
	    }

	    public void GenerateAtlas(KeyValuePair<string, Bitmap>[] bitmaps, IProgressReceiver progressReceiver)
        {
	        Log.Info($"Generating texture atlas out of {bitmaps.Length} bitmaps...");

	        Bitmap no;
	        using (MemoryStream ms = new MemoryStream(ResourceManager.ReadResource("Alex.Resources.no.png")))
	        {
				no = new Bitmap(ms);
	        }

	        var regular = new[]
	        {
		        new KeyValuePair<string, Bitmap>("no_texture", no),
	        }.Concat(bitmaps.Where(x => x.Value.Height == 16 && x.Value.Width == 16)).ToArray();
		       
	        var others = bitmaps.Where(x => x.Value.Height != 16 || x.Value.Width != 16).ToList();

	        Bitmap[] waterFrames = new Bitmap[0];
	        Bitmap[] lavaFrames = new Bitmap[0];
	        Bitmap[] waterFlowFrames = new Bitmap[0];
	        Bitmap[] lavaFlowFrames = new Bitmap[0];
	        
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
	        }

	        var bitmap = new Bitmap(AtlasWidth, AtlasHeight);

	        int total = regular.Length + others.Count;

	        if (waterFrames.Length > 0)
	        {
		        total++;

		        regular = regular.Append(new KeyValuePair<string, Bitmap>("block/water_still", waterFrames[0])).ToArray();
	        }

	        if (waterFlowFrames.Length > 0)
	        {
		        total++;

		        regular = regular.Append(new KeyValuePair<string, Bitmap>("block/water_flow", waterFlowFrames[0])).ToArray();
	        }
	        
	        if (lavaFrames.Length > 0)
	        {
		        total++;
		        regular = regular.Append(new KeyValuePair<string, Bitmap>("block/lava_still", lavaFrames[0])).ToArray();
	        }
	        
	        if (lavaFlowFrames.Length > 0)
	        {
		        total++;
		        regular = regular.Append(new KeyValuePair<string, Bitmap>("block/lava_flow", lavaFlowFrames[0])).ToArray();
	        }
	        
			int xi = 0, yi = 0, offsetX = 0, yRemaining = 0;
	        int processedFiles = Process(ref bitmap, regular, ref xi, ref yi, ref offsetX, ref yRemaining, total, 0, progressReceiver);
	        yi += 16;
	        xi = 0;
			Process(ref bitmap, others.ToArray(), ref xi, ref yi, ref offsetX, ref yRemaining, total, processedFiles, progressReceiver);
		
			Atlas = bitmap;

			_atlas = TextureUtils.BitmapToTexture2D(Graphics, bitmap);
			AtlasSize = new Vector2(_atlas.Width, _atlas.Height);

			var waterLocation = GetAtlasLocation("block/water_still");
			var waterFlowLocation = GetAtlasLocation("block/water_flow");
			
			var lavaLocation = GetAtlasLocation("block/lava_still");
			var lavaFlowLocation = GetAtlasLocation("block/lava_flow");
			_frames = new Texture2D[Math.Max(waterFrames.Length, waterFlowFrames.Length)];
			for (int i = 0; i < _frames.Length; i++)
			{
				var target = new Bitmap(bitmap);
				var r = new System.Drawing.Rectangle(0, 0, 16, 16);
				var destination = new System.Drawing.Rectangle((int) waterLocation.Position.X, (int) waterLocation.Position.Y, 16, 16);

				if (waterFrames.Length > 0)
					TextureUtils.CopyRegionIntoImage(waterFrames[((i % 3 == 0 ? i -1 : i) / 6) % waterFrames.Length], r, ref target, destination);
				
				destination = new System.Drawing.Rectangle((int) waterFlowLocation.Position.X, (int) waterFlowLocation.Position.Y, 16, 16);
				if (waterFlowFrames.Length > 0)
					TextureUtils.CopyRegionIntoImage(waterFlowFrames[i % waterFlowFrames.Length], r, ref target, destination);
				
				destination = new System.Drawing.Rectangle((int) lavaLocation.Position.X, (int) lavaLocation.Position.Y, 16, 16);
				if (lavaFrames.Length > 0)
					TextureUtils.CopyRegionIntoImage(lavaFrames[i % lavaFrames.Length], r, ref target, destination);
				
				destination = new System.Drawing.Rectangle((int) lavaFlowLocation.Position.X, (int) lavaFlowLocation.Position.Y, 16, 16);
				if (lavaFlowFrames.Length > 0)
					TextureUtils.CopyRegionIntoImage(lavaFlowFrames[i % lavaFlowFrames.Length], r, ref target, destination);
				
				_frames[i] = TextureUtils.BitmapToTexture2D(Graphics, target);
			}
			
           // bitmap.Save("assets\\terrain.png", ImageFormat.Png);
			Log.Info($"TextureAtlas generated! (Width:{_atlas.Width}px Height:{_atlas.Height}px)");
        }

	    private int Process(ref Bitmap bmp, KeyValuePair<string, Bitmap>[] data, ref int xi, ref int yi, ref int xOffset, ref int yRemaining, int total, int processed, IProgressReceiver progressReceiver)
	    {
		    int done = processed;
			var count = 0;

		    foreach (var bm in data.OrderByDescending(x => x.Value.Height))
		    {
			    string key = bm.Key;
			    count++;

			    double percentage = 100D * ((double)processed / (double)total);
			    progressReceiver.UpdateProgress((int)percentage, "Stitching textures...", key);

                var r = new System.Drawing.Rectangle(0, 0, bm.Value.Width, bm.Value.Height);
			    var destination = new System.Drawing.Rectangle(xi, yi, bm.Value.Width, bm.Value.Height);

			    TextureUtils.CopyRegionIntoImage(bm.Value, r, ref bmp, destination);

			    if (!_atlasLocations.ContainsKey(key))
			    {
				    _atlasLocations.Add(key, new TextureInfo(new Vector2(xi, yi), bm.Value.Width, bm.Value.Height));
			    }

			    if (bm.Value.Height > 16)
			    {
				    xOffset = xi;
				    yRemaining = bm.Value.Height;
			    }
			    xi += bm.Value.Width;

			    if (count == AtlasWidth / 16)
			    {
				    yi += 16;
				    xi = 0;
				    yRemaining -= 16;

				    count = 0;
			    }

			    done++;

			   
			}

		    return done;
	    }

	    private Bitmap[] GetFrames(Bitmap source)
	    {
		    int ix = source.Width / 16;
		    int iy = source.Height / 16;

		    List<Bitmap> result = new List<Bitmap>();
		    
		    for (int x = 0; x < ix; x++)
		    for(int y = 0; y < iy; y++)
		    {
			    Bitmap newBitmap = new Bitmap(16, 16);
			    TextureUtils.CopyRegionIntoImage(source, new System.Drawing.Rectangle(x,y, 16, 16), ref newBitmap, new System.Drawing.Rectangle(0, 0, 16, 16));
			    
			    result.Add(newBitmap);
		    }

		    return result.ToArray();
	    }

	    private const int AtlasWidth = 512;
	    private const int AtlasHeight = 512;

	    public int TextureWidth { get; private set; } = 16;
	    public int TextureHeight { get; private set; }= 16;

		public void LoadResourcePackOnTop(KeyValuePair<string, Bitmap>[] bitmapsRaw, IProgressReceiver progressReceiver)
		{
			/*  string path = Path.Combine("assets", "minecraft", "textures", "blocks");

			  int textureWidth = 16, textureHeight = 16;
			  var bitmapsRaw = archive.Entries.Where(x =>
				  x.FullName.Replace('/', '\\').StartsWith(path, StringComparison.InvariantCultureIgnoreCase) &&
				  x.Name.EndsWith(".png")).ToArray();
			  Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();
			  foreach (var entry in bitmapsRaw)
			  {
				  string name = entry.Name.Replace(".png", "");
				  if (!bitmaps.ContainsKey(name))
				  {
					  using (Stream stream = entry.Open())
					  {
						  Bitmap texture = new Bitmap(stream);
						  if (texture.Width > textureWidth && texture.Width % 16 == 0 && texture.Height > textureHeight && texture.Height % 16 == 0)
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
			  }*/

			int textureWidth = 16, textureHeight = 16;
			Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();
			foreach (var bitmap in bitmapsRaw)
			{
				string name = Path.GetFullPath(bitmap.Key);

				if (!bitmaps.ContainsKey(name))
				{
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


			Dictionary<string, TextureInfo> newLocations = new Dictionary<string, TextureInfo>();
			int cx = 0;
			int cy = 0;
			//Log.Debug($"Texture Height: {textureHeight} Width: {textureWidth}");
			Bitmap modifiedBitmap = new Bitmap(32 * textureWidth, 32 * textureHeight);

			foreach (var loc in _atlasLocations)
			{
				System.Drawing.Rectangle target = new System.Drawing.Rectangle(cx, cy, textureWidth, textureHeight);

				int w = textureWidth;
				int h = textureHeight;

				var atlasLocation = loc.Value;
				if (bitmaps.TryGetValue(loc.Key, out Bitmap texture))
				{
					TextureUtils.CopyRegionIntoImage(texture, new System.Drawing.Rectangle(0, 0, textureWidth, textureHeight), ref modifiedBitmap, target);
				}
				else
				{
					w = 16;
					h = 16;
					TextureUtils.CopyRegionIntoImage(Atlas, new System.Drawing.Rectangle((int)atlasLocation.Position.X, (int)atlasLocation.Position.Y, 16, 16), ref modifiedBitmap, target);
				}

				newLocations.Add(loc.Key, new TextureInfo(new Vector2(cx, cy), w, h));

				cx += textureWidth;
				if (cx == modifiedBitmap.Width)
				{
					cx = 0;
					cy += textureHeight;
				}
			}

			int noDataSection = textureWidth / 2;
			for (int xi = 0; xi < 2; xi++)
			{
				for (int yi = 0; yi < 2; yi++)
				{
					for (int x = xi * noDataSection; x < (xi * noDataSection) + noDataSection; x++)
					{
						for (int y = yi * noDataSection; y < (yi * noDataSection) + noDataSection; y++)
						{
							Color color;
							if ((xi == 0 && yi == 0) || (xi == 1 && yi == 1))
							{
								color = Color.Magenta;
							}
							else
							{
								color = Color.Black;
							}
							modifiedBitmap.SetPixel(x, y, color);
						}
					}
				}
			}

			Atlas = modifiedBitmap;
			_atlasLocations = newLocations;
			TextureWidth = textureWidth;
			TextureHeight = textureHeight;

			_atlas = TextureUtils.BitmapToTexture2D(Graphics, modifiedBitmap);
		    AtlasSize = new Vector2(_atlas.Width, _atlas.Height);

			//modifiedBitmap.Save("debug.png", ImageFormat.Png);
		}


		public int GetFrameCount()
		{
			return _frames.Length;
		}
		
		public Texture2D GetAtlas(int frame)
		{
			return _frames[frame % _frames.Length];
		    return _atlas;
	    }

	    public TextureInfo GetAtlasLocation(string file)
	    {
		    if (_atlasLocations.Count == 0) throw new Exception();

		    if (file == "water_still" && !_atlasLocations.ContainsKey(file))
			    file = "water_flow";
		    
		    return _atlasLocations.ContainsKey(file) ? _atlasLocations[file] : new TextureInfo(Vector2.Zero, TextureWidth, TextureHeight);
	    }
	}
}
