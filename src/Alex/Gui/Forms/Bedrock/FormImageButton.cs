using System;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using RocketUI;

namespace Alex.Gui.Forms.Bedrock
{
    public class FormImageButton : GuiContainer
    {
        private string ImageUrl { get; }
        private FormImage Image { get; set; }
        private GuiStackMenuItem Button { get; set; }
        public FormImageButton(string image, string text, Action action, bool isTranslationKey = false)
        {
            ImageUrl = image;
            
            GuiStackContainer stackContainer = new GuiStackContainer();
            stackContainer.Orientation = Orientation.Horizontal;
            stackContainer.ChildAnchor = Alignment.MiddleFill;
            stackContainer.Anchor = Alignment.MiddleFill;
            
            Image = new FormImage(ImageUrl);
            Image.Anchor = Alignment.MiddleLeft;
            Image.Width = 16;
            Image.Height = 16;
           // Image.Margin = new Thickness(0, 0, 5, 0);

            Button = new GuiStackMenuItem(text, action, isTranslationKey);
            Button.Anchor = Alignment.MiddleFill;
            
         //   GuiControl contr = new GuiControl();
         //   contr.AddChild(Button);
            
            GuiControl imgControl = new GuiControl();
            imgControl.AddChild(Image);
            
            GuiControl buttonCtr = new GuiControl();
            buttonCtr.AddChild(Button);
            
            stackContainer.AddChild(imgControl);
            stackContainer.AddChild(buttonCtr);
            
            AddChild(stackContainer);
           // Button.AddChild(Image);
           // base.TextElement.Anchor = Alignment.MiddleRight;
           //AddChild(Button);
          // AddChild(Image);
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
        }
    }
}