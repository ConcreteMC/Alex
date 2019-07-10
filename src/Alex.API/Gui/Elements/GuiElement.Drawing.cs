using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.API.Gui.Elements
{
    public partial class GuiElement
    {
        private float _rotation;
        [DebuggerVisible] public float Rotation
        {
            get => _rotation;
            set => _rotation = MathHelper.ToRadians(value);
        }

        [DebuggerVisible] public virtual Vector2 RotationOrigin { get; set; } = Vector2.Zero;

        [DebuggerVisible] public bool ClipToBounds { get; set; } = false;

        public GuiTexture2D Background;
        public GuiTexture2D BackgroundOverlay;


        protected virtual void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            graphics.FillRectangle(RenderBounds, Background);
            
            graphics.FillRectangle(RenderBounds, BackgroundOverlay);
        }
    }
}
