using Alex.API.Gui;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Input;
using Microsoft.Xna.Framework;

namespace Alex.Gui.Forms
{
    public class FormBase : GuiDialogBase
    {
        public    uint               FormId       { get; set; }
        protected BedrockFormManager Parent       { get; }
        protected InputManager       InputManager { get; }
        protected GuiContainer       Container    => ContentContainer;
        public FormBase(uint formId, BedrockFormManager parent, InputManager inputManager)
        {
            FormId = formId;
            Parent = parent;
            InputManager = inputManager;
            
            Background = new Color(Color.Black, 0.5f);
            Container.Anchor = Alignment.FillCenter;
            Container.MinWidth = 356;
            Container.Width = 356;
            //   Container = new GuiContainer();
            //   Container.Anchor = Alignment.FillCenter;

            //    AddChild(Container);
        }    
        
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            if (InputManager.Any(x => x.IsPressed(InputCommand.Exit)))
            {
                Parent.Hide(FormId);
            }
            else
            {
                if (!Alex.Instance.IsMouseVisible)
                    Alex.Instance.IsMouseVisible = true;
            }
        }
    }
}