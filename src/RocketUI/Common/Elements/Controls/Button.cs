using System;
using Microsoft.Xna.Framework;

namespace RocketUI.Elements.Controls
{
    public class Button : Control
    {

        public string Text
        {
            get => TextBlock.Text;
	        set => TextBlock.Text = value;
        }
	    public string TranslationKey
	    {
		    get => TextBlock.TranslationKey;
		    set => TextBlock.TranslationKey = value;
	    }

        protected TextBlock TextBlock { get; }
        protected Action Action { get; }
		
	    public Button(Action action = null) : this(string.Empty, action)
	    {

	    }
		
        public Button(string text, Action action = null)
        {
            Action = action;
            MinHeight = 20;
	        MinWidth = 20;

	        Padding = new Thickness(15, 15);
			Margin = new Thickness(5);

	        AddChild(TextBlock = new TextBlock()
            {
				Margin =  Thickness.Zero,
                Anchor = Anchor.MiddleCenter,
                Text = text,
                Foreground = Color.White,
				TextOpacity = 0.875f
            });
        }

	    protected override void OnHighlightActivate()
	    {
		    base.OnHighlightActivate();

			TextBlock.Foreground = Color.Yellow;
	    }

	    protected override void OnHighlightDeactivate()
	    {
		    base.OnHighlightDeactivate();

			TextBlock.Foreground = Color.White;
	    }

	    protected override void OnCursorPressed(Point cursorPosition)
        {
            Action?.Invoke();
        }
    }
}
