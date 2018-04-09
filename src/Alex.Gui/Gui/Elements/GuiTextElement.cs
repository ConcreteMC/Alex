using Alex.Graphics.Gui.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Gui.Elements
{
    public class GuiTextElement : GuiElement
    {

        public string Text { get; set; }

        public SpriteFont Font { get; set; }

        protected override void OnInit(IGuiRenderer renderer)
        {
            Font = renderer.DefaultFont;
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);
        }

        protected override void OnDraw(GuiRenderArgs renderArgs)
        {
            
        }
    }
}
