using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Graphics.Typography;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Gui.Rendering
{
    public class GuiSpriteBatch : IDisposable
    {
        public IFont Font { get; set; }
        //public GraphicsContext Graphics { get; }
        public SpriteBatch SpriteBatch { get; }
        public GuiScaledResolution ScaledResolution { get; }

        private readonly GraphicsDevice _graphicsDevice;
        private readonly IGuiRenderer _renderer;
        private Texture2D _colorTexture;

        private bool _beginSpriteBatchAfterContext;
        private bool _hasBegun;

        public GuiSpriteBatch(IGuiRenderer renderer, GraphicsDevice graphicsDevice)
        {
            _renderer = renderer;
            _graphicsDevice = graphicsDevice;
            SpriteBatch = new SpriteBatch(_graphicsDevice);

            Font = _renderer.Font;
            ScaledResolution = _renderer.ScaledResolution;
        }
        
        public Vector2 Project(Vector2 point)
        {
            return Vector2.Transform(point, ScaledResolution.TransformMatrix);
        }
        public Vector2 Unproject(Vector2 screen)
        {
            return Vector2.Transform(screen, ScaledResolution.InverseTransformMatrix);
        }

        public void Begin()
        {
            if (_hasBegun) return;
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, ScaledResolution.TransformMatrix);
        
            _hasBegun = true;

        }
        public void End()
        {
            if (!_hasBegun) return;
            SpriteBatch.End();
            _hasBegun = false;
        }

        
        #region Sub-Contexts

        public GraphicsContext ExitContext(BlendState blendState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, SamplerState samplerState = null)
        {
            _beginSpriteBatchAfterContext = _hasBegun;
            End();

            var context = GraphicsContext.CreateContext(_graphicsDevice, blendState, depthStencilState, rasterizerState, samplerState);
            context.Disposed += OnGraphicsContextDisposed;
            return context;
        }

        private void OnGraphicsContextDisposed(object sender, EventArgs e)
        {
            if (_beginSpriteBatchAfterContext)
            {
                Begin();
            }
        }

        #endregion

        #region Drawing

        public void DrawLine(Vector2 from, Vector2 to, Color color, int thickness = 1)
        {
            var length = Vector2.Distance(from, to);
            var angle = (float) Math.Atan2(to.Y - from.Y, to.X - from.X);

            DrawLine(from, length, angle, color, thickness);
        }
        public void DrawLine(Vector2 from, float length, float angle, Color color, int thickness = 1)
        {
            SpriteBatch.Draw(ColorTexture, from, null, color, angle, new Vector2(0f, 0.5f), new Vector2(length, thickness), SpriteEffects.None, 0f);
        }

        public void DrawRectangle(Rectangle rectangle, Color color, int thickness)
        {
            DrawRectangle(rectangle, color, new Thickness(thickness));
        }
        public void DrawRectangle(Rectangle rectangle, Color color, Thickness thickness)
        {
            if (thickness.Top > 0)
            {
                DrawLine(rectangle.TopLeft(), rectangle.TopRight(), color, thickness.Top);
            }
            
            if (thickness.Right > 0)
            {
                DrawLine(rectangle.TopRight(), rectangle.BottomRight(), color, thickness.Right);
            }
            
            if (thickness.Bottom > 0)
            {
                DrawLine(rectangle.BottomLeft(), rectangle.BottomRight(), color, thickness.Bottom);
            }
            
            if (thickness.Left > 0)
            {
                DrawLine(rectangle.TopLeft(), rectangle.BottomLeft(), color, thickness.Left);
            }
        }

        public void FillRectangle(Rectangle rectangle, Color color)
        {
            SpriteBatch.Draw(ColorTexture, rectangle, color);
        }
        public void FillRectangle(Rectangle rectangle, ITexture2D texture)
        {
            SpriteBatch.Draw(texture, rectangle);
        }
        public void FillRectangle(Rectangle rectangle, ITexture2D texture, TextureRepeatMode repeatMode = TextureRepeatMode.Stretch)
        {
            if (repeatMode == TextureRepeatMode.NoScaleCenterSlice)
            {
                DrawTextureCenterSliced(rectangle, texture);
            }
            else if (repeatMode == TextureRepeatMode.Tile)
            {
                DrawTextureTiled(rectangle, texture);
            }
            else if (texture is NinePatchTexture2D ninePatchTexture)
            {
                DrawTextureNinePatch(rectangle, ninePatchTexture);
            }
            else
            {
                SpriteBatch.Draw(texture, rectangle);
            }
        } 
        public void FillRectangle(Rectangle rectangle, ITexture2D texture, TextureRepeatMode repeatMode, Vector2? scale)
        {
            if (repeatMode == TextureRepeatMode.NoScaleCenterSlice)
            {
                DrawTextureCenterSliced(rectangle, texture);
            }
            else if (repeatMode == TextureRepeatMode.Tile)
            {
                DrawTextureTiled(rectangle, texture);
            }
            else if (texture is NinePatchTexture2D ninePatchTexture)
            {
                DrawTextureNinePatch(rectangle, ninePatchTexture);
            }
            else if(scale.HasValue)
            {
                SpriteBatch.Draw(texture.Texture, rectangle.Location.ToVector2(), texture.ClipBounds, Color.White, 0f, Vector2.Zero, scale.Value, SpriteEffects.None, 0f);
            }
            else
            {
                SpriteBatch.Draw(texture, rectangle);
            }
        } 

        #endregion

        #region Drawing - Text
                
        public void DrawString(Vector2 position,      string   text,                  TextColor color, FontStyle style, float scale = 1f,
                               float   rotation = 0f, Vector2? rotationOrigin = null, float     opacity = 1f)
        {
            Font?.DrawString(SpriteBatch, text, position, color, style, new Vector2(scale), opacity, rotation, rotationOrigin);
        }
        public void DrawString(Vector2 position,      string   text,                  TextColor color, FontStyle style, Vector2? scale = null,
                               float   rotation = 0f, Vector2? rotationOrigin = null, float     opacity = 1f)
        {
            Font?.DrawString(SpriteBatch, text, position, color, style, scale, opacity, rotation, rotationOrigin);
        }

        public void DrawString(Vector2 position,      string   text, IFont font, TextColor color, FontStyle style, float scale = 1f,
                               float   rotation = 0f, Vector2? rotationOrigin = null, float     opacity = 1f)
        {
            font?.DrawString(SpriteBatch, text, position, color, style, new Vector2(scale), opacity, rotation, rotationOrigin);
        }
        public void DrawString(Vector2 position,      string   text, IFont font, TextColor color, FontStyle style, Vector2? scale = null,
                               float   rotation = 0f, Vector2? rotationOrigin = null, float     opacity = 1f)
        {
            font?.DrawString(SpriteBatch, text, position, color, style, scale, opacity, rotation, rotationOrigin);
        }
        

        #endregion

        #region TextureRepeatMode Helpers

        private void DrawTextureNinePatch(Rectangle rectangle, NinePatchTexture2D ninePatchTexture)
        {
            if (ninePatchTexture.Padding == Thickness.Zero)
            {
                SpriteBatch.Draw(ninePatchTexture, rectangle);
                return;
            }

            var sourceRegions = ninePatchTexture.SourceRegions;
            var destRegions   = ninePatchTexture.ProjectRegions(rectangle);

            for (var i = 0; i < sourceRegions.Length; i++)
            {
                var srcPatch = sourceRegions[i];
                var dstPatch = destRegions[i];

                if(dstPatch.Width > 0 && dstPatch.Height > 0)
                    SpriteBatch.Draw(ninePatchTexture, sourceRectangle: srcPatch, destinationRectangle: dstPatch);
            }
        }
        private void DrawTextureTiled(Rectangle rectangle, ITexture2D texture)
        {
            var repeatX = Math.Ceiling((float) rectangle.Width / texture.Width);
            var repeatY = Math.Ceiling((float) rectangle.Height / texture.Height);

            for (int i = 0; i < repeatX; i++)
            for (int j = 0; j < repeatY; j++)
            {
                SpriteBatch.Draw(texture, new Vector2(i * texture.Width, j * texture.Height));
            }
        }
        private void DrawTextureCenterSliced(Rectangle rectangle, ITexture2D texture)
        {
            var halfWidth  = rectangle.Width / 2f;
            var halfHeight = rectangle.Height / 2f;
            int xOffset    = rectangle.X + (int)Math.Max(0, (rectangle.Width - texture.Width) / 2f);
            int yOffset    = rectangle.Y + (int)Math.Max(0, (rectangle.Height - texture.Height) / 2f);

            int dstLeftWidth   = (int) Math.Floor(halfWidth);
            int dstRightWidth  = (int) Math.Ceiling(halfWidth);
            int dstLeftHeight  = (int) Math.Floor(halfHeight);
            int dstRightHeight = (int) Math.Ceiling(halfHeight);

            var srcHalfWidth  = Math.Min(texture.Width / 2f, halfWidth);
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

        #endregion

        private Texture2D ColorTexture
        {
            get
            {
                if (_colorTexture == null)
                {
                    _colorTexture = new Texture2D(_graphicsDevice, 1, 1, false, SurfaceFormat.Color);
                    _colorTexture.SetData(new [] { Color.White });
                }

                return _colorTexture;
            }
        }

        #region Disposing

        public void Dispose()
        {
            _colorTexture?.Dispose();
            SpriteBatch?.Dispose();
        }

        #endregion
    }
}
