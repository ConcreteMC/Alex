using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Rendering;
using Alex.API.Input;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Alex.API.Gui
{
    public class GuiDebugHelper
    {
        private static readonly Vector2 DebugFontScale = new Vector2(0.25f);
        
        private static readonly Color DebugTextBackground = Color.WhiteSmoke * 0.6f;
        private static readonly Color DebugTextForeground = Color.Black * 0.95f;

        private static readonly Color OuterBoundsBackground = Color.LightGoldenrodYellow * 0.1f;
        private static readonly Color BoundsBackground = Color.LightSeaGreen * 0.2f;
        private static readonly Color InnerBoundsBackground = Color.CornflowerBlue * 0.1f;
        
        public bool Enabled { get; set; }

        public Keys ToggleDebugHotKey { get; set; } = Keys.Pause;

        protected GuiManager GuiManager { get; }

        protected IGuiRenderer Renderer => GuiManager.GuiRenderer;
        protected InputManager Input => GuiManager.InputManager;
        protected GuiRenderArgs RenderArgs => GuiManager.GuiRenderArgs;
        protected SpriteBatch SpriteBatch => GuiManager.SpriteBatch;
        
        private KeyboardState _previousKeyboard, _currentKeyboard;

        internal GuiDebugHelper(GuiManager manager)
        {
            GuiManager = manager;
        }

        public void Update(GameTime gameTime)
        {
            _previousKeyboard = _currentKeyboard;
            _currentKeyboard = Keyboard.GetState();

            if (_previousKeyboard.IsKeyDown(ToggleDebugHotKey) && _currentKeyboard.IsKeyUp(ToggleDebugHotKey))
            {
                Enabled = !Enabled;
            }

            if (!Enabled) return;

            // add extra updates below here
        }

        public void DrawScreen(IGuiScreen screen)
        {
            if (!Enabled) return;

            screen.ForEachChild(c => DrawElementRecursive(c));
        }

        private void DrawElementRecursive(IGuiElement element)
        {
            DrawDebug(element);

            element.ForEachChild(c => DrawElementRecursive(c));
        }
        private void DrawDebug(IGuiElement element)
        {
            if (!Enabled) return;


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

        #region Draw Helpers

        private void DrawDebugBounds(Rectangle bounds, Color color, bool drawBackground = false, bool drawBorders = true, bool drawCoordinates = true, bool drawSize = true)
        {
            // Bounding Rectangle
            if (drawBackground)
            {
                RenderArgs.FillRectangle(bounds, color);
            }

            if (drawBorders)
            {
                RenderArgs.DrawRectangle(bounds, color, 1);
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

        private void DrawDebugString(Vector2 position,   object obj, Alignment align = Alignment.TopLeft)
        {
            var x = (align & (Alignment.CenterX | Alignment.FillX)) != 0 ? 0 : ((align & Alignment.MinX) != 0 ? -1 : 1);
            var y = (align & (Alignment.CenterY | Alignment.FillY)) != 0 ? 0 : ((align & Alignment.MinY) != 0 ? -1 : 1);

            DrawDebugString(position, obj.ToString(), DebugTextBackground, DebugTextForeground, 2, x, y);
        }
        private void DrawDebugString(Vector2 position, object obj, Color color, int padding = 2, int xAlign = 0, int yAlign = 0)
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
                RenderArgs.FillRectangle(new Rectangle((int)(p.X - padding), (int)(p.Y - padding), (int)(s.X + 2*padding), (int)(s.Y + 2*padding)), background.Value);
            }

            Renderer.DebugFont.DrawString(SpriteBatch, text, p, (TextColor) color, scale: DebugFontScale);
        }

        #endregion
    }
}
