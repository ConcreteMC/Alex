using Alex.API.Gui;
using Alex.API.Input;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using RocketUI;
using RocketUI.Input;
using DialogBase = RocketUI.DialogBase;

namespace Alex.Gui.Forms
{
    public class FormBase : DialogBase
    {
        public    uint               FormId       { get; set; }
        protected BedrockFormManager Parent       { get; }
        protected InputManager       InputManager { get; }
        protected Container       Container    => ContentContainer;
        public FormBase(uint formId, BedrockFormManager parent, InputManager inputManager)
        {
            FormId = formId;
            Parent = parent;
            InputManager = inputManager;
            
            Background = new Color(Color.Black, 0.5f);
            Container.Anchor = Alignment.FillCenter;
            Container.MinWidth = 356;
            Container.Width = 356;
            //   Container = new Container();
            //   Container.Anchor = Alignment.FillCenter;

            //    AddChild(Container);
        }

        protected string FixContrast(string text)
        {
            return text
               .Replace(TextColor.Gray.ToString(), TextColor.White.ToString())
               .Replace(TextColor.DarkGray.ToString(), TextColor.White.ToString())
               .Replace(TextColor.Black.ToString(), TextColor.White.ToString());
        }
        
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            if (InputManager.Any(x => x.IsPressed(AlexInputCommand.Exit)))
            {
                Parent.Hide(FormId);
            }
            else
            {
                if (!Alex.Instance.IsMouseVisible)
                    Alex.Instance.IsMouseVisible = true;
            }
        }

        /// <inheritdoc />
        public override void OnClose()
        {
            
        }
    }
}