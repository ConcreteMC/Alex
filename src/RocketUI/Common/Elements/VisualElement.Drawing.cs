using Microsoft.Xna.Framework;
using RocketUI.Graphics;
using RocketUI.Graphics.Textures;
using RocketUI.Styling;

namespace RocketUI.Elements
{
    public partial class VisualElement
    {
        [StyledProperty]
        public float Rotation
        {
            get => (float) GetValue();
            set => SetValue(MathHelper.ToRadians(value));
        }
        
        [StyledProperty]
        public virtual Vector2 RotationOrigin
        {
            get => (Vector2) GetValue();
            set => SetValue(value, InvalidateLayout);
        }
        
        [StyledProperty]
        public GuiTexture2D Background
        {
            get => (GuiTexture2D) GetValue();
            set => SetValue(value, InvalidateLayout);
        }
        public GuiTexture2D BackgroundOverlay
        {
            get => (GuiTexture2D) GetValue();
            set => SetValue(value, InvalidateLayout);
        }


        protected virtual void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            graphics.FillRectangle(RenderBounds, Background);
            
            graphics.FillRectangle(RenderBounds, BackgroundOverlay);
        }
    }
}
