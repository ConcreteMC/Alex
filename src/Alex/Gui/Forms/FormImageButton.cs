using System;
using RocketUI;

namespace Alex.Gui.Forms
{
    public class FormImageButton : Container
    {
        private string ImageUrl { get; }
        private FormImage Image { get; set; }
        private StackMenuItem Button { get; set; }
        public FormImageButton(string image, string text, Action action, bool isTranslationKey = false)
        {
            ImageUrl = image;
            
            StackContainer stackContainer = new StackContainer();
            stackContainer.Orientation = Orientation.Horizontal;
            stackContainer.ChildAnchor = Alignment.MiddleCenter;
            stackContainer.Anchor = Alignment.MiddleFill;
            
            Image = new FormImage(ImageUrl);
            Image.Anchor = Alignment.MiddleLeft;
            Image.Width = 16;
            Image.Height = 16;
           // Image.Margin = new Thickness(0, 0, 5, 0);

            Button = new StackMenuItem(text, action, isTranslationKey).ApplyModernStyle(true);
            Button.Anchor = Alignment.MiddleFill;
            
         //   RocketControl contr = new RocketControl();
         //   contr.AddChild(Button);
            
            RocketControl imgControl = new RocketControl();
            imgControl.AddChild(Image);
            
            RocketControl buttonCtr = new RocketControl();
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