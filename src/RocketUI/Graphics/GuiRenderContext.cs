﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI.Graphics.Textures;
using RocketUI.Utilities;

namespace RocketUI.Graphics
{
    public class GuiSpriteBatch : IDisposable
    {
        //public GraphicsContext Graphics { get; }
        public SpriteBatch SpriteBatch { get; }
        public GuiScaledResolution ScaledResolution { get; }

        public GraphicsContext Context { get; }

        private readonly GraphicsDevice _graphicsDevice;
        private readonly IGuiResourceProvider _resourceProvider;
        private Texture2D _colorTexture;

        private bool _beginSpriteBatchAfterContext;
        private bool _hasBegun;

        public GuiSpriteBatch(IGuiResourceProvider resourceProvider, GuiScaledResolution scaledResolution, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _resourceProvider = resourceProvider;
            _graphicsDevice = graphicsDevice;
            SpriteBatch = spriteBatch;
            Context = GraphicsContext.CreateContext(_graphicsDevice, BlendState.AlphaBlend, DepthStencilState.None, RasterizerState.CullNone, SamplerState.PointClamp);

            ScaledResolution = scaledResolution;
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

        public GraphicsContext BranchContext(BlendState blendState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, SamplerState samplerState = null)
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
        public void DrawRectangle(Rectangle rectangle, Color color, Thickness thickness, bool borderOutside = false)
        {
            var borderRectangle = rectangle;

            if (borderOutside)
            {
                borderRectangle += thickness;
            }

            if (thickness.Top > 0)
            {
                DrawLine(borderRectangle.TopLeft()   , borderRectangle.TopRight()   , color, thickness.Top);
            }
            
            if (thickness.Right > 0)
            {
                DrawLine(borderRectangle.TopRight()  , borderRectangle.BottomRight(), color, thickness.Right);
            }
            
            if (thickness.Bottom > 0)
            {
                DrawLine(borderRectangle.BottomLeft(), borderRectangle.BottomRight(), color, thickness.Bottom);
            }
            
            if (thickness.Left > 0)
            {
                DrawLine(borderRectangle.TopLeft()   , borderRectangle.BottomLeft() , color, thickness.Left);
            }
        }

        public void FillRectangle(Rectangle rectangle, Color color)
        {
            SpriteBatch.Draw(ColorTexture, rectangle, color);
        }

        public void FillRectangle(Rectangle rectangle, GuiTexture2D texture)
        {
            var bounds = rectangle;
            if (texture.Offset.HasValue)
            {
                bounds.Offset(texture.Offset.Value);
            }

            if (texture.Color.HasValue)
            {
                FillRectangle(bounds, texture.Color.Value);
            }

            if (texture.Texture == null && !string.IsNullOrEmpty(texture.TextureResource))
            {
                texture.TryResolveTexture(_resourceProvider);
            }

            if (texture.Texture != null)
            {
                FillRectangle(bounds, texture.Texture, texture.RepeatMode, texture.Scale, texture.Mask);
            }
        }

        public void FillRectangle(Rectangle rectangle, ITexture2D texture, TextureRepeatMode repeatMode = TextureRepeatMode.Stretch)
        {
            FillRectangle(rectangle, texture, repeatMode, null, Color.White);
        } 
        public void FillRectangle(Rectangle rectangle, ITexture2D texture, TextureRepeatMode repeatMode, Vector2? scale, Color? colorMask)
        {
            if(texture?.Texture == null) return;

            var mask = colorMask.HasValue ? colorMask.Value : Color.White;
            
            if (repeatMode == TextureRepeatMode.NoScaleCenterSlice)
            {
                DrawTextureCenterSliced(rectangle, texture, mask);
            }
            else if (repeatMode == TextureRepeatMode.Tile)
            {
                DrawTextureTiled(rectangle, texture, mask);
            }
            else if (repeatMode == TextureRepeatMode.ScaleToFit)
            {
                DrawTextureScaledToFit(rectangle, texture, mask);
            }
            else if (texture is NinePatchTexture2D ninePatchTexture)
            {
                DrawTextureNinePatch(rectangle, ninePatchTexture, mask);
            }
            else if(scale.HasValue)
            {
                SpriteBatch.Draw(texture.Texture, rectangle.Location.ToVector2(), texture.ClipBounds, mask, 0f, Vector2.Zero, scale.Value, SpriteEffects.None, 0f);
            }
            else
            {
                SpriteBatch.Draw(texture, rectangle, mask);
            }
        }
        #endregion

        #region TextureRepeatMode Helpers

        private void DrawTextureNinePatch(Rectangle rectangle, NinePatchTexture2D ninePatchTexture, Color mask)
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
                    SpriteBatch.Draw(ninePatchTexture, dstPatch, srcPatch, mask);
            }
        }
        private void DrawTextureTiled(Rectangle rectangle, ITexture2D texture, Color mask)
        {
            var repeatX = Math.Ceiling((float) rectangle.Width / texture.Width);
            var repeatY = Math.Ceiling((float) rectangle.Height / texture.Height);

            for (int i = 0; i < repeatX; i++)
            for (int j = 0; j < repeatY; j++)
            {
                SpriteBatch.Draw(texture, new Vector2(i * texture.Width, j * texture.Height), mask);
            }
        }

        private void DrawTextureScaledToFit(Rectangle rectangle, ITexture2D texture, Color mask)
        {
            var widthRatio = rectangle.Width / (float)texture.Width;
            var heightRatio = rectangle.Height / (float)texture.Height;

            var resultRatio = Math.Min(heightRatio, widthRatio);
            var scaledSize = new Vector2(texture.Width, texture.Height) * resultRatio;

            var xOffset = (rectangle.Width - scaledSize.X) / 2f;
            var yOffset = (rectangle.Height - scaledSize.Y) / 2f;

            var dstBounds = new Rectangle((int)xOffset, (int)yOffset, (int)scaledSize.X, (int)scaledSize.Y);
            SpriteBatch.Draw(texture, dstBounds, mask);
        }

        private void DrawTextureCenterSliced(Rectangle rectangle, ITexture2D texture, Color mask)
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
            SpriteBatch.Draw(texture.Texture, new Rectangle(xOffset               , yOffset, dstLeftWidth, dstLeftHeight), new Rectangle(srcX, srcY, srcLeftWidth, srcLeftHeight), mask);
                
            // MinY MaxX
            SpriteBatch.Draw(texture.Texture, new Rectangle(xOffset + dstLeftWidth, yOffset, dstRightWidth, dstRightHeight), new Rectangle(srcX + texture.Width - srcRightWidth, srcY, srcRightWidth, srcRightHeight), mask);


            // MaxY MinX
            SpriteBatch.Draw(texture.Texture, new Rectangle(xOffset               , yOffset + dstLeftHeight , dstLeftWidth, dstLeftHeight), new Rectangle(srcX, srcY + texture.Height - srcRightHeight, srcLeftWidth, srcLeftHeight), mask);
                
            // MaxY MaxX
            SpriteBatch.Draw(texture.Texture, new Rectangle(xOffset + dstLeftWidth, yOffset + dstRightHeight, dstRightWidth, dstRightHeight), new Rectangle(srcX + texture.Width - srcRightWidth, srcY + texture.Height - srcRightHeight, srcRightWidth, srcRightHeight), mask);

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

        public void DrawString(Vector2 renderPosition, string text, IFont font, Color textColor, Color? textShadowColor = null, FontStyle fontStyle = FontStyle.None, float scale = 1f, float rotation = 0f, Vector2? rotationOrigin = null, float textOpacity = 1f)
        {
            font.DrawString(SpriteBatch, text, renderPosition, textColor * textOpacity, textShadowColor ?? Color.TransparentBlack, fontStyle, new Vector2(scale), 1f, rotation, rotationOrigin ?? Vector2.Zero, SpriteEffects.None, 0f);
        }
    }
}
