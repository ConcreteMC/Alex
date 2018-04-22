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
        public GuiTextures?      DisabledBackgroundTexture { get; set; }
        public GuiTextures?      HighlightedBackgroundTexture { get; set; }
        public GuiTextures?      FocusedBackgroundTexture { get; set; }
        
        public ITexture2D DisabledBackground { get; set; }
        public ITexture2D HighlightedBackground { get; set; }
        public ITexture2D FocusedBackground { get; set; }

        public virtual Color     HighlightOutlineColor { get; set; } = new Color(TextColor.Gray.ForegroundColor, 0.75f);
        public virtual Thickness HighlightOutlineThickness { get; set; } = Thickness.Zero;

        public virtual Color     FocusOutlineColor     { get; set; } = new Color(Color.White, 0.75f);
        public virtual Thickness FocusOutlineThickness { get; set; } = Thickness.Zero;

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);

            if (DisabledBackgroundTexture.HasValue)
            {
                DisabledBackground = renderer.GetTexture(DisabledBackgroundTexture.Value);
            }

            if (HighlightedBackgroundTexture.HasValue)
            {
                HighlightedBackground = renderer.GetTexture(HighlightedBackgroundTexture.Value);
            }

            if (FocusedBackgroundTexture.HasValue)
            {
                FocusedBackground = renderer.GetTexture(FocusedBackgroundTexture.Value);
            }
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            if (!Enabled && DisabledBackground != null)
            {
                Background = DisabledBackground;
            }
            else if (Focused)
            {
                Background = FocusedBackground ?? (Highlighted ? HighlightedBackground ?? DefaultBackground : DefaultBackground);
            }
            else {
                if (Highlighted)
                {
                    Background = HighlightedBackground ?? DefaultBackground;
                }
                else
                {
                    Background = DefaultBackground;
                }

            }
        }

        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            base.OnDraw(graphics, gameTime);

            if (Focused && FocusOutlineThickness != Thickness.Zero)
            {
                var outlineBounds = RenderBounds;
                outlineBounds.Inflate(1f,1f);
                graphics.DrawRectangle(outlineBounds, FocusOutlineColor, FocusOutlineThickness);
            }
            else if (Highlighted && HighlightOutlineThickness != Thickness.Zero)
            {
                var outlineBounds = RenderBounds;
                outlineBounds.Inflate(1f,1f);
                graphics.DrawRectangle(outlineBounds, HighlightOutlineColor, HighlightOutlineThickness);
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
