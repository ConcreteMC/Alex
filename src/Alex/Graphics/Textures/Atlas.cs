using System.Collections.Generic;
using Alex.Common.Utils;
using FmodAudio;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Alex.Graphics.Textures
{
	/// <summary>
	/// The texture atlas
	/// </summary>
	public class Atlas
	{
		/// <summary>
		/// Width in pixels
		/// </summary>
		public int Width;
        
		/// <summary>
		/// Height in Pixel
		/// </summary>
		public int Height;

		/// <summary>
		/// List of the nodes in the Atlas. This will represent all the textures that are packed into it and all the remaining free space
		/// </summary>
		public List<Node> Nodes;

		public Image<Rgba32> GenerateTexture(bool debugMode)
		{
			  Image<Rgba32> img = new Image<Rgba32>(Width, Height);

            if (debugMode)
            {
                img.Mutate(x =>
                {
                    x.Fill(Color.Green,
                        new Rectangle(0, 0, Width, Height));
                });
            }

            foreach (Node n in Nodes)
            {
                if (n.Texture != null)
                {
                    var sourceImg = n.Texture.Source;

                    TextureUtils.CopyRegionIntoImage(
                        sourceImg, new System.Drawing.Rectangle(0, 0, sourceImg.Width, sourceImg.Height), ref img,
                        new System.Drawing.Rectangle(n.Bounds.Location.X, n.Bounds.Location.Y, n.Bounds.Width, n.Bounds.Height));

                   /* if (debugMode)
                    {
                        string label = Path.GetFileNameWithoutExtension(n.Texture.ResourceLocation.ToString());
                        
                        img.Mutate(x =>
                        {
                            var labelBox = TextMeasurer.Measure(label, new RendererOptions(font));
                            
                            RectangleF rectBounds = new Rectangle(n.Bounds.Location,
                                new Size((int)labelBox.Width, (int)labelBox.Height));
                            
                            x.Fill(Color.Black, rectBounds);

                            x.DrawText(label, font, Color.White, rectBounds.Location);
                        });
                    }*/
                }
                else
                {
                    img.Mutate(x =>
                    {
                        x.Fill(Color.DarkMagenta,
                                    new Rectangle(
                                        n.Bounds.Location.X, n.Bounds.Location.Y, n.Bounds.Width, n.Bounds.Height));
                    });
                    //g.FillRectangle(Brushes.DarkMagenta, n.Bounds);

                    /*if (debugMode)
                    {
                        string label = n.Bounds.Width.ToString() + "x" + n.Bounds.Height.ToString();
                        
                        img.Mutate(x =>
                        {
                            var labelBox = TextMeasurer.Measure(label, new RendererOptions(font));
                            
                            RectangleF rectBounds = new Rectangle(n.Bounds.Location,
                                new Size((int)labelBox.Width, (int)labelBox.Height));
                            
                            x.Fill(Color.Black, rectBounds);

                            x.DrawText(label, font, Color.White, rectBounds.Location);
                        });
                    }*/
                }
            }

            return img;
		}
	}
}