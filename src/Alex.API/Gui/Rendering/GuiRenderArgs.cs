using System;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Gui.Rendering
{
    public class GuiRenderArgs
    {
        public IGuiRenderer Renderer { get; }

        public GraphicsDevice Graphics { get; }
        public SpriteBatch SpriteBatch { get; }

        public GameTime GameTime { get; }

        public GuiScaledResolution ScaledResolution { get; }

        //public GuiElementRenderContext ActiveContext { get; private set; }

        public GuiRenderArgs(IGuiRenderer renderer, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, GameTime gameTime, GuiScaledResolution scaledResolution)
        {
            Renderer = renderer;
            Graphics = graphicsDevice;
            SpriteBatch = spriteBatch;
            GameTime = gameTime;
            ScaledResolution = scaledResolution;
        }

        public void BeginSpriteBatch()
        {
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, ScaledResolution.TransformMatrix);
        }

        public void EndSpriteBatch()
        {
            SpriteBatch.End();
        }

        #region Debug Helpers

        private static readonly Vector2 DebugFontScale = new Vector2(0.25f);
        
        private static readonly Color DebugTextBackground = Color.WhiteSmoke * 0.5f;
        private static readonly Color DebugTextForeground = Color.Black * 0.85f;

        private static readonly Color OuterBoundsBackground = Color.LightGoldenrodYellow * 0.1f;
        private static readonly Color BoundsBackground = Color.LightSeaGreen * 0.2f;
        private static readonly Color InnerBoundsBackground = Color.CornflowerBlue * 0.1f;

        public void DrawDebug(GuiElement element)
        {
            if (element.OuterBounds != element.Bounds)
            {
                DrawDebugBounds(element.OuterBounds, OuterBoundsBackground, false, true, false, false);
            }

            DrawDebugBounds(element.Bounds, BoundsBackground, false, true, false, false);

            if (element.AutoSizeMode == AutoSizeMode.None)
            {
                DrawDebugBounds(element.RenderBounds, Color.Red, false, true, true, true);
            }

            if (element.AutoSizeMode == AutoSizeMode.GrowAndShrink)
            {
                DrawDebugBounds(element.RenderBounds, Color.YellowGreen, false, true, true, true);
            }

            if (element.AutoSizeMode == AutoSizeMode.GrowOnly)
            {
                DrawDebugBounds(element.RenderBounds, Color.LawnGreen, false, true, true, true);
            }
            
            if(element.InnerBounds != element.Bounds);
            {
                DrawDebugBounds(element.InnerBounds, InnerBoundsBackground, true, true, false, false);
            }

            DrawDebugString(element.Bounds.TopCenter(), element.GetType().Name);
        }

        public void DrawDebugBounds(Rectangle bounds, Color color, bool drawBackground = false, bool drawBorders = true, bool drawCoordinates = true, bool drawSize = true)
        {
            // Bounding Rectangle
            if (drawBackground)
            {
                FillRectangle(bounds, color);
            }

            if (drawBorders)
            {
                DrawRectangle(bounds, color, 1);
            }

            var pos = bounds.Location;
            if (drawCoordinates)
            {
                DrawDebugString(bounds.TopLeft(), $"({pos.X}, {pos.Y})", Alignment.BottomLeft);
            }

            if (drawSize)
            {
                DrawDebugString(bounds.TopRight(), $"[{bounds.Width} x {bounds.Height}]");
            }
        }

        public void DrawDebugString(Vector2 position,   object obj, Alignment align = Alignment.TopLeft)
        {
            var x = (align & (Alignment.CenterX | Alignment.FillX)) != 0 ? 0 : ((align & Alignment.MinX) != 0 ? -1 : 1);
            var y = (align & (Alignment.CenterY | Alignment.FillY)) != 0 ? 0 : ((align & Alignment.MinY) != 0 ? -1 : 1);

            DrawDebugString(position, obj.ToString(), Color.WhiteSmoke * 0.5f, Color.Black, 2, x, y);
        }

        public void DrawDebugString(Vector2 position, object obj, Color color, int padding = 2, int xAlign = 0, int yAlign = 0)
        {
            DrawDebugString(position, obj.ToString(), color, padding, xAlign, yAlign);
        }

        private void DrawDebugString(Vector2 position, string text, Color? background, Color color, int padding = 2, int xAlign = 0, int yAlign = 0)
        {
            if (Renderer.DebugFont == null) return;

            var p = position;
            var s = Renderer.DebugFont.MeasureString(text) * DebugFontScale;

            if (xAlign == 1)
            {
                p.X = p.X - (s.X + padding);
            }
            else if(xAlign == 0)
            {
                p.X = p.X - (s.X / 2f);
            }
            else if (xAlign == -1)
            {
                p.X = p.X + padding;
            }

            if (yAlign == 1)
            {
                p.Y = p.Y - (s.Y + padding);
            }
            else if(yAlign == 0)
            {
                p.Y = p.Y - (s.Y / 2f);
            }
            else if (yAlign == -1)
            {
                p.Y = p.Y + padding;
            }

            if (background.HasValue)
            {
                FillRectangle(new Rectangle((int)(p.X - padding), (int)(p.Y - padding), (int)(s.X + 2*padding), (int)(s.Y + 2*padding)), background.Value);
            }

            SpriteBatch.DrawString(Renderer.DebugFont, text, p, color, 0f, Vector2.Zero, DebugFontScale, SpriteEffects.None, 0);
        }

        public Vector2 Project(Vector2 vector)
        {
            return Renderer.Project(vector);
        }
        public Point Project(Point point)
        {
            return Renderer.Project(point.ToVector2()).ToPoint();
        }
        public Rectangle Project(Rectangle rectangle)
        {
            var location = Project(rectangle.Location);
            var size = Project(rectangle.Location + rectangle.Size);
            return new Rectangle(location.X, location.Y, size.X-location.X, size.Y-location.Y);
        }

        #endregion
        
        public void DrawRectangle(Rectangle bounds, Color color, int thickness = 1)
        {
            DrawRectangle(bounds, color, thickness, thickness, thickness, thickness);
        }

        public void DrawRectangle(Rectangle bounds, Color color, Thickness thickness)
        {
            DrawRectangle(bounds, color, thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
        }

        public void DrawRectangle(Rectangle bounds,         Color color, int thicknessVertical, int thicknessHorizontal)
        {
            DrawRectangle(bounds, color, thicknessHorizontal, thicknessVertical, thicknessHorizontal, thicknessVertical);
        }

        public void DrawRectangle(Rectangle bounds, Color color, int thicknessLeft, int thicknessTop, int thicknessRight, int thicknessBottom)
        {
            var texture = new Texture2D(Graphics, 1, 1, false, SurfaceFormat.Color);
            texture.SetData(new Color[] {color});

            // MinY
            if (thicknessTop > 0)
            {
                SpriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y, bounds.Width, thicknessTop), color);   
            }

            // MaxX
            if (thicknessRight > 0)
            {
                SpriteBatch.Draw(texture,
                                 new Rectangle(bounds.X + bounds.Width - thicknessRight, bounds.Y, thicknessRight, bounds.Height),
                                 color);
            }

            // MaxY
            if (thicknessBottom > 0)
            {
                SpriteBatch.Draw(texture,
                                 new Rectangle(bounds.X, bounds.Y + bounds.Height - thicknessBottom, bounds.Width, thicknessBottom),
                                 color);
            }

            // MinX
            if (thicknessLeft > 0)
            {
                SpriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y, thicknessLeft, bounds.Height), color);
            }
        }

        public void FillRectangle(Rectangle bounds, Color color)
        {
            var texture = new Texture2D(Graphics, 1, 1, false, SurfaceFormat.Color);
            texture.SetData(new Color[] {color});

            SpriteBatch.Draw(texture, bounds, Color.White);
        }
        
        public void DrawLine(int startX, int startY, int endX, int endY, Color color, int thickness = 1)
        {
            // TODO: support angles
            DrawRectangle(new Rectangle(startX, startY, endX-startX, endY-startY), color, thickness);
        }

        public void DrawLine(Vector2 startPosition, Vector2 endPosition, Color color)
        {
            //var diff = (endPosition - startPosition);
            //var len = Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);

            //diff.Normalize();
            
            //SpriteBatch.
        }

        public void Draw(TextureSlice2D    texture, Rectangle bounds,
                         TextureRepeatMode repeatMode = TextureRepeatMode.Stretch, Vector2? scale = null)
        {
            if (texture is NinePatchTexture2D ninePatch)
            {
                DrawNinePatch(bounds, ninePatch, repeatMode, scale);
            }
            else
            {
                FillRectangle(bounds, texture, repeatMode, scale);
            }
        }
        
        public void DrawNinePatch(Rectangle bounds, NinePatchTexture2D ninePatchTexture, TextureRepeatMode repeatMode, Vector2? scale = null)
        {
            if(scale == null) scale = Vector2.One;
            
            if (ninePatchTexture.Padding == Thickness.Zero)
            {
                FillRectangle(bounds, ninePatchTexture.Texture, repeatMode);
                return;
            }
			
            var sourceRegions = ninePatchTexture.SourceRegions;
            var destRegions   = ninePatchTexture.ProjectRegions(bounds);

            for (var i = 0; i < sourceRegions.Length; i++)
            {
                var srcPatch = sourceRegions[i];
                var dstPatch = destRegions[i];

                if(dstPatch.Width > 0 && dstPatch.Height > 0)
                    SpriteBatch.Draw(ninePatchTexture, sourceRectangle: srcPatch, destinationRectangle: dstPatch);
            }
        }

        public void FillRectangle(Rectangle bounds, TextureSlice2D texture, TextureRepeatMode repeatMode, Vector2? scale = null)
        {
            if(scale == null) scale = Vector2.One;

            if (repeatMode == TextureRepeatMode.NoRepeat)
            {
                SpriteBatch.Draw(texture, bounds);
            }
            else if (repeatMode == TextureRepeatMode.Stretch)
            {
                SpriteBatch.Draw(texture, bounds);
            }
            else if (repeatMode == TextureRepeatMode.ScaleToFit)
            {
            }
            else if (repeatMode == TextureRepeatMode.Tile)
            {
                Vector2 size = texture.ClipBounds.Size.ToVector2() * scale.Value;

                var repeatX = Math.Ceiling((float) bounds.Width  / size.X);
                var repeatY = Math.Ceiling((float) bounds.Height / size.Y);

                for (int i = 0; i < repeatX; i++)
                {
                    for (int j = 0; j < repeatY; j++)
                    {
                        var p = bounds.Location.ToVector2() + new Vector2(i * size.X, j * size.Y);
                        SpriteBatch.Draw(texture, p, scale);
                    }
                }
            }
            else if (repeatMode == TextureRepeatMode.NoScaleCenterSlice)
            {

                var halfWidth = bounds.Width / 2f;
                var halfHeight = bounds.Height / 2f;
                int xOffset = bounds.X + (int)Math.Max(0, (bounds.Width - texture.Width) / 2f);
                int yOffset = bounds.Y + (int)Math.Max(0, (bounds.Height - texture.Height) / 2f);

                int dstLeftWidth = (int) Math.Floor(halfWidth);
                int dstRightWidth = (int) Math.Ceiling(halfWidth);
                int dstLeftHeight = (int) Math.Floor(halfHeight);
                int dstRightHeight = (int) Math.Ceiling(halfHeight);

                var srcHalfWidth = Math.Min(texture.Width / 2f, halfWidth);
                var srcHalfHeight = Math.Min(texture.Height / 2f, halfHeight);

                var srcX = texture.ClipBounds.X;
                var srcY = texture.ClipBounds.Y;

                int srcLeftWidth   = (int) Math.Floor(srcHalfWidth);
                int srcRightWidth  = (int) Math.Ceiling(srcHalfWidth);
                int srcLeftHeight  = (int) Math.Floor(srcHalfHeight);
                int srcRightHeight = (int) Math.Ceiling(srcHalfHeight);

                // MinY MinX
                SpriteBatch.Draw(texture, new Rectangle(xOffset               , yOffset, dstLeftWidth, dstLeftHeight), new Rectangle(srcX, srcY, srcLeftWidth, srcLeftHeight), Color.White);
                
                // MinY MaxX
                SpriteBatch.Draw(texture, new Rectangle(xOffset + dstLeftWidth, yOffset, dstRightWidth, dstRightHeight), new Rectangle(srcX + texture.Width - srcRightWidth, srcY, srcRightWidth, srcRightHeight), Color.White);


                // MaxY MinX
                SpriteBatch.Draw(texture, new Rectangle(xOffset               , yOffset + dstLeftHeight , dstLeftWidth, dstLeftHeight), new Rectangle(srcX, srcY + texture.Height - srcRightHeight, srcLeftWidth, srcLeftHeight), Color.White);
                
                // MaxY MaxX
                SpriteBatch.Draw(texture, new Rectangle(xOffset + dstLeftWidth, yOffset + dstRightHeight, dstRightWidth, dstRightHeight), new Rectangle(srcX + texture.Width - srcRightWidth, srcY + texture.Height - srcRightHeight, srcRightWidth, srcRightHeight), Color.White);
            }
        }


        #region SpriteFont Proxy

        public void DrawString(Vector2 position, SpriteFont font, string text, Color color, float scale = 1f)
        {
            SpriteBatch.DrawString(font, text, position, color, 0f, Vector2.Zero, new Vector2(scale), SpriteEffects.None, 0);
        }
        
        public void DrawString(Vector2 position, SpriteFont font, string text, Color color, float scale, float rotation, Vector2 origin, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            SpriteBatch.DrawString(font, text, position, color, rotation, origin, new Vector2(scale), effects, layerDepth);
        }

        public void DrawString(Vector2 position, SpriteFont font, string text, Color color, Vector2 scale, float rotation, Vector2 origin, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            SpriteBatch.DrawString(font, text, position, color, rotation, origin, scale, effects, layerDepth);
        }

        #endregion

        #region FontRenderer Proxy
        
		public void DrawString(IFontRenderer spriteFont, string text, Vector2 position, Color color, float scale = 1f)
		{
			//spriteFont.DrawString(SpriteBatch, text, position.X, position.Y, (int) color.PackedValue, false, new Vector2(scale));
            DrawString(spriteFont, text, position, color, new Vector2(scale), 0f, Vector2.Zero);
		}
        
        public void DrawString(IFontRenderer spriteFont, string text, Vector2 position, Color color, float scale, float rotation, Vector2 origin, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            spriteFont.DrawString(SpriteBatch, text, position, color, false, new Vector2(scale), rotation, origin, effects, layerDepth);
        }

        public void DrawString(IFontRenderer spriteFont, string text, Vector2 position, Color color, Vector2 scale, float rotation, Vector2 origin, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            spriteFont.DrawString(SpriteBatch, text, position, color, false, scale, rotation, origin, effects, layerDepth);
        }       

        #endregion

        #region BitmapFont Proxy
        
		public void DrawString(BitmapFont bitmapFont, string text, Vector2 position, TextColor color, bool dropShadow = true)
		{
		    SpriteBatch.DrawString(bitmapFont, text, position, color, dropShadow, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}

		public void DrawString(BitmapFont bitmapFont, string text, Vector2 position, float scale, TextColor color, bool dropShadow = true)
		{
		    SpriteBatch.DrawString(bitmapFont, text, position, color, dropShadow, 0f, Vector2.Zero, new Vector2(scale), SpriteEffects.None, 0f);
		}

		public void DrawString(BitmapFont bitmapFont, string text, Vector2 position, Vector2 scale, TextColor color, bool dropShadow = true)
		{
		    SpriteBatch.DrawString(bitmapFont, text, position, color, dropShadow, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		}

		public void DrawString(BitmapFont bitmapFont, string text, Vector2 position, float scale, TextColor color, bool dropShadow = true, float rotation = 0f, Vector2? origin = null)
		{
		    SpriteBatch.DrawString(bitmapFont, text, position, color, dropShadow, rotation, origin.HasValue ? origin.Value : Vector2.Zero, new Vector2(scale), SpriteEffects.None, 0f);
		}

		public void DrawString(BitmapFont bitmapFont, string text, Vector2 position, Vector2 scale, TextColor color, bool dropShadow = true, float rotation = 0f, Vector2? origin = null)
		{
		    SpriteBatch.DrawString(bitmapFont, text, position, color, dropShadow, rotation, origin.HasValue ? origin.Value : Vector2.Zero, scale, SpriteEffects.None, 0f);
		}

		public void DrawString(BitmapFont bitmapFont, string text, Vector2 position, TextColor color, bool dropShadow = true, float rotation = 0f, Vector2? origin = null)
		{
		    SpriteBatch.DrawString(bitmapFont, text, position, color, dropShadow, rotation, origin.HasValue ? origin.Value : Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}

        
        public void DrawString(BitmapFont bitmapFont, string text, Vector2 position, TextColor color, bool dropShadow, float rotation, Vector2 origin, float scale = 1f, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f, float opacity = 1f)
        {
            SpriteBatch.DrawString(bitmapFont, text, position, color, dropShadow, rotation, origin, new Vector2(scale), effects, layerDepth, opacity);
        }

        public void DrawString(BitmapFont bitmapFont, string text, Vector2 position, TextColor color, bool dropShadow, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f, float opacity = 1f)
        {
            SpriteBatch.DrawString(bitmapFont, text, position, color, dropShadow, rotation, origin, scale, effects, layerDepth, opacity);
        }

        #endregion
	}
    
}
