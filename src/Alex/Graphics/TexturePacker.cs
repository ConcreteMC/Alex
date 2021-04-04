using System;
using System.Collections.Generic;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = Microsoft.Xna.Framework.Color;
using Point = SixLabors.ImageSharp.Point;
using Rectangle = System.Drawing.Rectangle;

namespace Alex.Graphics
{
	public static class TexturePacker
    {
        /// <summary>
        /// Internal helper class keeps track of a sprite while it is being arranged.
        /// </summary>
        class ArrangedSprite
        {
            public int Index;
 
            public int X;
            public int Y;
 
            public int Width;
            public int Height;

            private Rectangle? _rectangle = null;
            public Rectangle Rectangle
            {
                get
                {
                    if (!_rectangle.HasValue)
                        _rectangle = new Rectangle(X, Y, Width, Height);

                    return _rectangle.Value;
                }
            }
        }
 
        public static Image<Rgba32> PackSprites(GraphicsDevice graphics, IList<Image<Rgba32>> sourceSprites,
                                                ICollection<Rectangle> outputSprites)
        {
            // Build up a list of all the sprites needing to be arranged.
            List<ArrangedSprite> sprites = new List<ArrangedSprite>();
 
            for (int i = 0; i < sourceSprites.Count; i++)
            {
                ArrangedSprite sprite = new ArrangedSprite();
 
                // Include a single pixel padding around each sprite, to avoid
                // filtering problems if the sprite is scaled or rotated.
                sprite.Width = sourceSprites[i].Width + 2;
                sprite.Height = sourceSprites[i].Height + 2;
 
                sprite.Index = i;
 
                sprites.Add(sprite);
            }
 
            // Sort so the largest sprites get arranged first.
            sprites.Sort(CompareSpriteSizes);
 
            // Work out how big the output bitmap should be.
            int outputWidth = GuessOutputWidth(sprites);
            int outputHeight = 0;
            int totalSpriteSize = 0;
 
            // Choose positions for each sprite, one at a time.
            for (int i = 0; i < sprites.Count; i++)
            {
                PositionSprite(sprites, i, outputWidth);
 
                outputHeight = Math.Max(outputHeight, sprites[i].Y + sprites[i].Height);
 
                totalSpriteSize += sprites[i].Width * sprites[i].Height;
            }
 
            // Sort the sprites back into index order.
            sprites.Sort(CompareSpriteIndices);
 
            return CopySpritesToOutput(graphics, sprites, sourceSprites, outputSprites,
                                       outputWidth, outputHeight);
        }
 
        /// <summary>
        /// Once the arranging is complete, copies the bitmap data for each
        /// sprite to its chosen position in the single larger output bitmap.
        /// </summary>
        static Image<Rgba32> CopySpritesToOutput(GraphicsDevice graphics, 
                                                List<ArrangedSprite> sprites,
                                                 IList<Image<Rgba32>> sourceSprites,
                                                 ICollection<Rectangle> outputSprites,
                                                 int width, int height)
        {
            Image<Rgba32> image = new Image<Rgba32>(width, height, new Rgba32(0, 0, 0, 0));
          /*  SpriteBatch spriteBatch = new SpriteBatch(graphics);
 
            RenderTarget2D renderTarget = new RenderTarget2D(graphics, width, height);
            graphics.SetRenderTarget(renderTarget);
            graphics.Clear(Color.Transparent);
 
            spriteBatch.Begin();*/
          foreach (ArrangedSprite sprite in sprites)
          {
              int x = sprite.X;
              int y = sprite.Y;

              int w = sourceSprites[sprite.Index].Width;
              int h = sourceSprites[sprite.Index].Height;

              var sourceSprite = sourceSprites[sprite.Index];

              TextureUtils.CopyRegionIntoImage(
                  sourceSprite, new System.Drawing.Rectangle(0, 0, w, h), ref image,
                  new System.Drawing.Rectangle(x + 1, y + 1, w, h));
              
              TextureUtils.CopyRegionIntoImage(
                  sourceSprite, new System.Drawing.Rectangle(0, 0, 1, h), ref image,
                  new System.Drawing.Rectangle(x, y + 1, 1, h));
              
              TextureUtils.CopyRegionIntoImage(
                  sourceSprite, new System.Drawing.Rectangle(w - 1, 0, 1, h), ref image,
                  new System.Drawing.Rectangle(x + w + 1, y + 1, 1, h));
              
              TextureUtils.CopyRegionIntoImage(
                  sourceSprite, new System.Drawing.Rectangle(0, 0, w, 1), ref image,
                  new System.Drawing.Rectangle(x + 1, y, w, 1));
              
              TextureUtils.CopyRegionIntoImage(
                  sourceSprite, new System.Drawing.Rectangle(0, h - 1, w, 1), ref image,
                  new System.Drawing.Rectangle(x + 1, y + h + 1, w, 1));
              
              TextureUtils.CopyRegionIntoImage(
                  sourceSprite, new System.Drawing.Rectangle(0, 0, 1, 1), ref image,
                  new System.Drawing.Rectangle(x, y, 1, 1));
              
              TextureUtils.CopyRegionIntoImage(
                  sourceSprite, new System.Drawing.Rectangle(w - 1, 0, 1, 1), ref image,
                  new System.Drawing.Rectangle(x + w + 1, y, 1, 1));
              
              TextureUtils.CopyRegionIntoImage(
                  sourceSprite, new System.Drawing.Rectangle(0, h - 1, 1, 1), ref image,
                  new System.Drawing.Rectangle(x, y + h + 1, 1, 1));
              
              TextureUtils.CopyRegionIntoImage(
                  sourceSprite, new System.Drawing.Rectangle(w - 1, h - 1, 1, 1), ref image,
                  new System.Drawing.Rectangle(x + w + 1, y + h + 1, 1, 1));
              // spriteBatch.Draw(sourceSprites[sprite.Index], new Vector2(x + 1, y + 1), Color.White);

              // Copy a border strip from each edge of the sprite, creating
              // a one pixel padding area to avoid filtering problems if the
              // sprite is scaled or rotated.

              // Copy a single pixel from each corner of the sprite,
              // filling in the corners of the one pixel padding area.
        
              outputSprites.Add(new Rectangle(x + 1, y + 1, w, h));
          }

          // spriteBatch.End();
 
          //  graphics.SetRenderTarget(null);
            return image;
        }
 
        /// <summary>
        /// Works out where to position a single sprite.
        /// </summary>
        static void PositionSprite(List<ArrangedSprite> sprites,
                                   int index, int outputWidth)
        {
            int x = 0;
            int y = 0;
 
            while (true)
            {
                // Is this position free for us to use?
                int intersects = FindIntersectingSprite(sprites, index, x, y);
 
                if (intersects < 0)
                {
                    sprites[index].X = x;
                    sprites[index].Y = y;
 
                    return;
                }
 
                // Skip past the existing sprite that we collided with.
                x = sprites[intersects].X + sprites[intersects].Width;
 
                // If we ran out of room to move to the right,
                // try the next line down instead.
                if (x + sprites[index].Width > outputWidth)
                {
                    x = 0;
                    y++;
                }
            }
        }
 
 
        /// <summary>
        /// Checks if a proposed sprite position collides with anything
        /// that we already arranged.
        /// </summary>
        static int FindIntersectingSprite(List<ArrangedSprite> sprites,
                                          int index, int x, int y)
        {
            int w = sprites[index].Width;
            int h = sprites[index].Height;

            for (int i = 0; i < index; i++)
            {
                if (sprites[i].X >= x + w)
                    continue;

                if (sprites[i].X + sprites[i].Width <= x)
                    continue;

                if (sprites[i].Y >= y + h)
                    continue;

                if (sprites[i].Y + sprites[i].Height <= y)
                    continue;

                return i;
            }

            return -1;
        }
 
        /// <summary>
        /// Comparison function for sorting sprites by size.
        /// </summary>
        static int CompareSpriteSizes(ArrangedSprite a, ArrangedSprite b)
        {
            int aSize = a.Height * 1024 + a.Width;
            int bSize = b.Height * 1024 + b.Width;
 
            return bSize.CompareTo(aSize);
        }
 
        /// <summary>
        /// Comparison function for sorting sprites by their original indices.
        /// </summary>
        static int CompareSpriteIndices(ArrangedSprite a, ArrangedSprite b)
        {
            return a.Index.CompareTo(b.Index);
        }
 
 
        /// <summary>
        /// Heuristic guesses what might be a good output width for a list of sprites.
        /// </summary>
        static int GuessOutputWidth(List<ArrangedSprite> sprites)
        {
            // Gather the widths of all our sprites into a temporary list.
            List<int> widths = new List<int>();
 
            foreach (ArrangedSprite sprite in sprites)
            {
                widths.Add(sprite.Width);
            }
 
            // Sort the widths into ascending order.
            widths.Sort();
 
            // Extract the maximum and median widths.
            int maxWidth = widths[widths.Count - 1];
            int medianWidth = widths[widths.Count / 2];
 
            // Heuristic assumes an NxN grid of median sized sprites.
            int width = medianWidth * (int)Math.Round(Math.Sqrt(sprites.Count));
 
            // Make sure we never choose anything smaller than our largest sprite.
            return Math.Max(width, maxWidth);
        }
    }
}