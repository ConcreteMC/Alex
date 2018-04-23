using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiControl : GuiContainer, IGuiControl
    {
        public GuiTexture2D DisabledBackground;
        public GuiTexture2D HighlightedBackground;
        public GuiTexture2D FocusedBackground;
        
        public virtual Color     HighlightOutlineColor { get; set; } = new Color(TextColor.Gray.ForegroundColor, 0.75f);
        public virtual Thickness HighlightOutlineThickness { get; set; } = Thickness.Zero;

        public virtual Color     FocusOutlineColor     { get; set; } = new Color(Color.White, 0.75f);
        public virtual Thickness FocusOutlineThickness { get; set; } = Thickness.Zero;

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
            DisabledBackground.TryResolveTexture(renderer);
            HighlightedBackground.TryResolveTexture(renderer);
            FocusedBackground.TryResolveTexture(renderer);
        }
        
        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            base.OnDraw(graphics, gameTime);

            if (!Enabled)
            {
                graphics.FillRectangle(RenderBounds, DisabledBackground);
            }
            else 
            {
                if(Focused)
                {
                    graphics.FillRectangle(RenderBounds, FocusedBackground);
                    
                    if (FocusOutlineThickness != Thickness.Zero)
                    {
                        graphics.DrawRectangle(RenderBounds, FocusOutlineColor, FocusOutlineThickness, true);
                    }
                }

                if (Highlighted)
                {
                    graphics.FillRectangle(RenderBounds, HighlightedBackground);
                    
                    if (HighlightOutlineThickness != Thickness.Zero)
                    {
                        graphics.DrawRectangle(RenderBounds, HighlightOutlineColor, HighlightOutlineThickness, true);
                    }
                }
            }
        }


        public bool Enabled { get; set; } = true;
        public bool Focused { get; private set; }
        public bool Highlighted { get; private set; }


        public void InvokeHighlightActivate()
        {
            if (!Enabled) return;

            Highlighted = true;
            OnHighlightActivate();
        }
        protected virtual void OnHighlightActivate() { }

        public void InvokeHighlightDeactivate()
        {
            Highlighted = false;
            OnHighlightDeactivate();
        }
        protected virtual void OnHighlightDeactivate() { }


        public void InvokeFocusActivate()
        {
            if (!Enabled) return;

            Focused = true;
            OnFocusActivate();
        }
        protected virtual void OnFocusActivate() { }

        public void InvokeFocusDeactivate()
        {
            Focused = false;
            OnFocusDeactivate();
        }
        protected virtual void OnFocusDeactivate() { }


        public void InvokeKeyInput(char character, Keys key)
        {
            OnKeyInput(character, key);
        }
        protected virtual void OnKeyInput(char character, Keys key) {}


        public void InvokeCursorDown(Vector2 cursorPosition)
        {
            OnCursorDown((cursorPosition - RenderPosition).ToPoint());
        }
        protected virtual void OnCursorDown(Point cursorPosition) { }

        public void InvokeCursorPressed(Vector2 cursorPosition)
        {
            OnCursorPressed((cursorPosition - RenderPosition).ToPoint());
        }
        protected virtual void OnCursorPressed(Point cursorPosition) { }

        public void InvokeCursorMove(Vector2 cursorPosition, Vector2 previousCursorPosition, bool isCursorDown)
        {
            var relativeNew = (cursorPosition - RenderPosition).ToPoint();
            var relativeOld = (previousCursorPosition - RenderPosition).ToPoint();

            OnCursorMove(relativeNew, relativeOld, isCursorDown);
        }
        protected virtual void OnCursorMove(Point cursorPosition, Point previousCursorPosition, bool isCursorDown) { }


        public void InvokeCursorEnter(Vector2 cursorPosition)
        {
            OnCursorEnter((cursorPosition - RenderPosition).ToPoint());
        }
        protected virtual void OnCursorEnter(Point cursorPosition) { }

        public void InvokeCursorLeave(Vector2 cursorPosition)
        {
            OnCursorLeave((cursorPosition - RenderPosition).ToPoint());
        }
        protected virtual void OnCursorLeave(Point cursorPosition) { }

    }
}
