using Alex.API.Graphics;
using Alex.API.Gui.Rendering;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiControl : GuiContainer
    {

        public bool Enabled { get; set; } = true;

        private bool _isHighlighted = false;
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                if (_isHighlighted == value) return;
                _isHighlighted = value;

                if(_isHighlighted)
                    InvokeHighlighted();
            }
        }

        private bool _isFocused = false;
        public bool IsFocused
        {
            get => _isFocused;
            set
            {
                if (_isFocused == value) return;
                _isFocused = value;

                if(_isFocused)
                    InvokeFocused();
            }
        }

        public GuiTextures? HighlightedBackgroundTexture { get; set; }
        public GuiTextures? FocusedBackgroundTexture { get; set; }

        public NinePatchTexture2D HighlightedBackground { get; set; }
        public NinePatchTexture2D FocusedBackground { get; set; }

        public void InvokeHighlighted()
        {
            OnHighlighted();
        }

        protected virtual void OnHighlighted() {}


        public void InvokeFocused()
        {
            OnFocused();
        }

        protected virtual void OnFocused() {}


        public void InvokeClick(Vector2 cursorPosition)
        {
            OnClick(cursorPosition);
        }

        protected virtual void OnClick(Vector2 cursorPosition) {}

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
            
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


            if (IsFocused)
            {
                Background = FocusedBackground;
            }
            else
            {
                if (IsHighlighted)
                {
                    Background = HighlightedBackground;
                }
                else
                {
                    Background = DefaultBackground;
                }
            }
        }

        protected override void OnDraw(GuiRenderArgs args)
        {
            base.OnDraw(args);

            var outlineBounds = Bounds;
                outlineBounds.Inflate(1f,1f);

            if (IsHighlighted)
            {
                args.DrawRectangle(outlineBounds, Color.White, 1);
            }
        }
    }
}
