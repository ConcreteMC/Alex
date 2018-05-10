using System;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using RocketUI;
using RocketUI.Elements.Controls;
using RocketUI.Graphics;
using RocketUI.Graphics.Textures;

namespace Alex.API.Gui.Elements.Controls
{
    public class MCButton : Control
    {

        public string Text
        {
            get => TextElement.Text;
	        set => TextElement.Text = value;
        }
	    public string TranslationKey
	    {
		    get => TextElement.TranslationKey;
		    set => TextElement.TranslationKey = value;
	    }

	    private bool _disabled = false;
	    public bool Disabled
	    {
		    get => _disabled;
		    set
		    {
			    //var oldValue = _disabled;
			    Enabled = !value;
			    _disabled = value;

			    if (_isModern)
			    {
				    if (value)
				    {
					    TextElement.TextColor = TextColor.DarkGray;
					    // TextElement.TextOpacity = 0.3f;
				    }
				    else
				    {
					    TextElement.TextColor = TextColor.White;
					    TextElement.TextOpacity = 1f;
				    }
			    }
		    }
	    }

        protected GuiMCTextElement TextElement { get; }
        protected Action Action { get; }
		
	    public MCButton(Action action = null) : this(string.Empty, action)
	    {
			
	    }
		
        public MCButton(string text, Action action = null)
        {
            Background			  = new GuiTexture2D(GuiTextures.ButtonDefault, TextureRepeatMode.NoScaleCenterSlice);
	        DisabledBackground	  = new GuiTexture2D(GuiTextures.ButtonDisabled, TextureRepeatMode.NoScaleCenterSlice);
            HighlightedBackground = new GuiTexture2D(GuiTextures.ButtonHover, TextureRepeatMode.NoScaleCenterSlice);
            FocusedBackground	  = new GuiTexture2D(GuiTextures.ButtonFocused, TextureRepeatMode.NoScaleCenterSlice);
			
            Action = action;
            MinHeight = 20;
	        MinWidth = 20;

	        MaxHeight = 22;
	        MaxWidth = 200;
			Padding = new Thickness(5, 5);
			Margin = new Thickness(2);

	        AddChild(TextElement = new GuiMCTextElement()
            {
				Margin =  Thickness.Zero,
                Anchor = Anchor.MiddleCenter,
                Text = text,
                TextColor = TextColor.White,
				TextOpacity = 0.875f,
				FontStyle = FontStyle.DropShadow
            });
        }

	    private bool _isModern = false;
	    public bool Modern
	    {
		    get { return _isModern; }
		    set
		    {
			    if (value)
			    {
				    _isModern = true;

				    Background = DisabledBackground = FocusedBackground = Color.Transparent;

				    HighlightedBackground = new Color(Color.Black * 0.8f, 0.5f);

				}
			    else
			    {
				    _isModern = false;
				    Background = GuiTextures.ButtonDefault;
				    DisabledBackground = GuiTextures.ButtonDisabled;
				    HighlightedBackground = GuiTextures.ButtonHover;
				    FocusedBackground = GuiTextures.ButtonFocused;
				}
			}
	    }

	    protected override void OnHighlightActivate()
	    {
		    base.OnHighlightActivate();
		    if (_isModern)
		    {
			    TextElement.TextColor = TextColor.Cyan;
			}
		    else
		    {
			    TextElement.TextColor = TextColor.Yellow;
			}
		}

	    protected override void OnHighlightDeactivate()
	    {
		    base.OnHighlightDeactivate();

			TextElement.TextColor = TextColor.White;
	    }

	    protected override void OnCursorPressed(Point cursorPosition)
        {
            Action?.Invoke();
        }

	    protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
	    {
		    if (!Enabled && !_disabled)
		    {
			    Disabled = true;
		    }

		    base.OnDraw(graphics, gameTime);
	    }
    }
}
