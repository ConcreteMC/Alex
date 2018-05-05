using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI.Elements.Layout;
using RocketUI.Graphics;
using RocketUI.Graphics.Textures;

namespace RocketUI.Elements.Controls
{
    public class Control : VisualElement, IGuiControl
    {
        public event EventHandler HighlightActivate;
        public event EventHandler HighlightDeactivate;
        public event EventHandler FocusActivate;
        public event EventHandler FocusDeactivate;
        public event EventHandler CursorDown;
        public event EventHandler CursorUp;
        public event EventHandler CursorMove;
        public event EventHandler CursorEnter;
        public event EventHandler CursorLeave;
        public event EventHandler KeyInput;

        public GuiTexture2D DisabledBackground;
        public GuiTexture2D HighlightedBackground;
        public GuiTexture2D FocusedBackground;
        
        public virtual Color     HighlightOutlineColor { get; set; } = new Color(Color.Gray, 0.75f);
        public virtual Thickness HighlightOutlineThickness { get; set; } = Thickness.Zero;

        public virtual Color     FocusOutlineColor     { get; set; } = new Color(Color.White, 0.75f);
        public virtual Thickness FocusOutlineThickness { get; set; } = Thickness.Zero;

        protected override void OnInit()
        {
            base.OnInit();

            //Resources.ResolveSoundEffects(this);

            DisabledBackground.TryResolveTexture(Resources);
            HighlightedBackground.TryResolveTexture(Resources);
            FocusedBackground.TryResolveTexture(Resources);
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
            HighlightActivate?.Invoke(this, null);
        }
        protected virtual void OnHighlightActivate() { }

        public void InvokeHighlightDeactivate()
        {
            Highlighted = false;
            OnHighlightDeactivate();
            HighlightDeactivate?.Invoke(this, null);
        }
        protected virtual void OnHighlightDeactivate() { }


        public void InvokeFocusActivate()
        {
            if (!Enabled) return;

            Focused = true;
            OnFocusActivate();
            FocusActivate?.Invoke(this, null);
        }
        protected virtual void OnFocusActivate() { }

        public void InvokeFocusDeactivate()
        {
            Focused = false;
            OnFocusDeactivate();
            FocusDeactivate?.Invoke(this, null);
        }
        protected virtual void OnFocusDeactivate() { }


        public void InvokeKeyInput(char character, Keys key)
        {
            OnKeyInput(character, key);
            KeyInput?.Invoke(this, null);
        }
        protected virtual void OnKeyInput(char character, Keys key) {}


        public void InvokeCursorDown(Vector2 cursorPosition)
        {
            OnCursorDown((cursorPosition - RenderPosition).ToPoint());
            CursorDown?.Invoke(this, null);
        }
        protected virtual void OnCursorDown(Point cursorPosition) { }

        public void InvokeCursorPressed(Vector2 cursorPosition)
        {
            OnCursorPressed((cursorPosition - RenderPosition).ToPoint());
            CursorUp?.Invoke(this, null);
        }
        protected virtual void OnCursorPressed(Point cursorPosition) { }

        public void InvokeCursorMove(Vector2 cursorPosition, Vector2 previousCursorPosition, bool isCursorDown)
        {
            var relativeNew = (cursorPosition - RenderPosition).ToPoint();
            var relativeOld = (previousCursorPosition - RenderPosition).ToPoint();

            OnCursorMove(relativeNew, relativeOld, isCursorDown);
            CursorMove?.Invoke(this, null);
        }
        protected virtual void OnCursorMove(Point cursorPosition, Point previousCursorPosition, bool isCursorDown) { }


        public void InvokeCursorEnter(Vector2 cursorPosition)
        {
            OnCursorEnter((cursorPosition - RenderPosition).ToPoint());
            CursorEnter?.Invoke(this, null);
        }
        protected virtual void OnCursorEnter(Point cursorPosition) { }

        public void InvokeCursorLeave(Vector2 cursorPosition)
        {
            OnCursorLeave((cursorPosition - RenderPosition).ToPoint());
            CursorLeave?.Invoke(this, null);
        }
        protected virtual void OnCursorLeave(Point cursorPosition) { }

    }
}
