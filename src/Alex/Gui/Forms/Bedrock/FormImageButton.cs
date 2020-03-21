using System;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using RocketUI;

namespace Alex.Gui.Forms.Bedrock
{
    public class FormImageButton : GuiStackMenuItem
    {
        private string ImageUrl { get; }
        private FormImage Image { get; set; }
        public FormImageButton(string image, string text, Action action, bool isTranslationKey = false) : base(text, action, isTranslationKey)
        {
            ImageUrl = image;
            
            Image = new FormImage(ImageUrl);
            Image.Anchor = Alignment.TopLeft;

            base.TextElement.Anchor = Alignment.MiddleRight;
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
            
            AddChild(Image);
        }
    }
}