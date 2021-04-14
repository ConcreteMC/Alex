using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Alex.API.Graphics;
using Alex.API.Resources;
using Alex.API.Utils;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Gamestates.InGame;
using Alex.Graphics.Models.Blocks;
using Alex.Graphics.Textures;
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
using TextureInfo = Alex.Graphics.Textures.TextureInfo;

namespace Alex.Graphics
{
    public class AtlasGenerator
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));

	    private Dictionary<ResourceLocation,  Utils.TextureInfo> _atlasLocations = new Dictionary<ResourceLocation,  Utils.TextureInfo>();
	    private PooledTexture2D _textureAtlas;
	    
        public Vector2 AtlasSize { get; private set; }
        public AtlasGenerator()
	    {

	    }

	    public void Reset()
	    {
		    _atlasLocations = new Dictionary<ResourceLocation,  Utils.TextureInfo>();

		    AtlasSize = default;
		    _textureAtlas = default;
	    }

	    public void GetTextures(McResourcePack resourcePack,
		    Dictionary<ResourceLocation, ImageEntry> textures,
		    IProgressReceiver progressReceiver)
	    {
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
		    
		    List<TextureInfo> textures = new List<TextureInfo>();
		    textures.Add(new TextureInfo()
		    {
			    Height = no.Height,
			    Source = no,
			    Width = no.Width,
			    ResourceLocation = new ResourceLocation("minecraft", "no_texture")
		    });
		    
		    foreach (var texture in blockTextures)
		    {
			    textures.Add(new TextureInfo()
			    {
				    ResourceLocation = texture.Key,
				    Height = texture.Value.Height,
				    Width = texture.Value.Width,
				    Source = texture.Value.Image,
				    Meta = texture.Value.Meta
			    });
		    }

		    var size = textures.Max(x => Math.Max(x.Width, x.Height));
		   
		    Packer p = new Packer();
		    p.FitHeuristic = BestFitHeuristic.MaxOneAxis;
		    
		    Dictionary<ResourceLocation, Utils.TextureInfo> textureInfos = new Dictionary<ResourceLocation, Utils.TextureInfo>();

		    foreach (var atlas in p.Process(textures, size, 4))
		    {
			    var img = atlas.GenerateTexture(false);// p.CreateAtlasImage(atlas);

			    foreach (var node in atlas.Nodes)
			    {
				    if (node.Texture == null)
					    continue;
				    
				    textureInfos.TryAdd(
					    node.Texture.ResourceLocation,
					    new Utils.TextureInfo(
						    new Vector2(img.Width, img.Height),
						    new Vector2(node.Bounds.Location.X, node.Bounds.Location.Y) ,
						    node.Bounds.Size.Width, 
						    node.Bounds.Size.Height, 
						    node.Bounds.Size.Height != node.Bounds.Size.Width,
						    node.Bounds.Width / TextureWidth,
						    node.Bounds.Height / TextureHeight));
			    }

			    _textureAtlas = GetMipMappedTexture2D(device, img);
			    _atlasLocations = textureInfos;
			    break;
		    }
		    
		    AtlasSize = new Vector2(_textureAtlas.Width, _textureAtlas.Height);
		    totalSize += _textureAtlas.MemoryUsage;
		    sw.Stop();
		    
		    Log.Info(
			    $"TextureAtlas's generated in {sw.ElapsedMilliseconds}ms! ({PlayingState.GetBytesReadable(totalSize, 2)})");
	    }

	    private PooledTexture2D GetMipMappedTexture2D(GraphicsDevice device, Image<Rgba32> image)
	    {
		    PooledTexture2D texture = GpuResourceManager.GetTexture2D(this, device, image.Width, image.Height, true, SurfaceFormat.Color);

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
					    colorData = new uint[pixelSpan.Length];

					    for (int i = 0; i < pixelSpan.Length; i++)
					    {
						    colorData[i] = pixelSpan[i].Rgba;
					    }
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

        public Texture2D GetAtlas()
		{
			return _textureAtlas;
		}

        public  Utils.TextureInfo GetAtlasLocation(
			ResourceLocation file)
		{
			if (_atlasLocations.TryGetValue(file, out var atlasInfo))
			{
				return atlasInfo;
			}

			return _atlasLocations
				["no_texture"]; // new TextureInfo(AtlasSize, Vector2.Zero, TextureWidth, TextureHeight, false, false);
		}
    }
}
