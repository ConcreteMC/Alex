using Alex.Common.Input;
using Alex.Common.Utils;
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
        protected InputManager       ReliableInputManager { get; }
        protected Container       Container    => ContentContainer;
        public FormBase(uint formId, BedrockFormManager parent, InputManager inputManager)
        {
            FormId = formId;
            Parent = parent;
            ReliableInputManager = inputManager;
            
            Background = new Color(Color.Black, 0.5f);
            Container.Anchor = Alignment.FillCenter;
            Container.MinWidth = 356;
            Container.Width = 356;
            //   Container = new Container();
            //   Container.Anchor = Alignment.FillCenter;

            //    AddChild(Container);
        }

        /// <inheritdoc />
        public override void OnShow()
        {
            base.OnShow();
            Alex.Instance.IsMouseVisible = true;
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

            if (ReliableInputManager.Any(x => x.IsPressed(AlexInputCommand.Exit)))
            {
                Parent.Hide(FormId);
            }
        }

        /// <inheritdoc />
        public override void OnClose()
        {
            Parent.Hide(FormId);
        }
    }
}