using System;
using System.Threading;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui.Elements
{
    public class TitleComponent : GuiStackContainer, ITitleComponent
    {
	    private float _fadeValue = 1.0f;
	    private GuiTextElement _title;
	    private GuiTextElement _subTitle;

	    public TitleComponent()
	    {
		    Anchor = Alignment.MiddleFill;
		    Orientation = Orientation.Vertical;

		    _title = new GuiTextElement()
		    {
			    //Anchor = Alignment.TopCenter,
			    TextColor = TextColor.White,
			    FontStyle = FontStyle.DropShadow,
			    Scale = 2f,
			    Text = ""
		    };

			_subTitle = new GuiTextElement()
			{
				//Anchor = Alignment.MiddleCenter,
				TextColor = TextColor.White,
				FontStyle = FontStyle.None,
				Scale = 1f,
				Text =	""
            };
	    }

	    #region Titles

	    private ManualResetEventSlim TitleResetEvent = new ManualResetEventSlim(false);
	    
	    public void Show()
	    {
		    _hidden = false;
		    _hideTime = DateTime.UtcNow + TimeSpan.FromMilliseconds((_fadeIn + _fadeOut + _stay) * 50);
	    }
	    
	    public void SetTitle(ChatObject value)
	    {
		    _title.Text = value.RawMessage;

		    AddChild(_title);
		    AddChild(_subTitle);
        }

	    public void SetSubtitle(ChatObject value)
	    {
		    _subTitle.Text = value.RawMessage;
        }

	    private bool _hidden = true;
		private DateTime _hideTime = DateTime.MinValue;

	    private int _fadeIn = 0, _fadeOut = 0, _stay = 20;
	    public void SetTimes(int fadeIn, int stay, int fadeOut)
	    {
		    _fadeIn = fadeIn;
		    _fadeOut = fadeOut;
		    _stay = stay;
			
		    TitleResetEvent.Reset();
	    }

	    public void Hide()
	    {
		    _hidden = true;
			RemoveChild(_title);
			RemoveChild(_subTitle);
	    }

	    public void Reset()
	    {
		    _title.Text = string.Empty;
		    _subTitle.Text = string.Empty;
		    TitleResetEvent.Set();
	    }

	    #endregion

	    protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
	    {
            if (_hidden) return;
		    base.OnDraw(graphics, gameTime);
	    }

	    protected override void OnUpdate(GameTime gameTime)
	    {
			base.OnUpdate(gameTime);

		    if (!_hidden)
		    {
			    if (DateTime.UtcNow >= _hideTime)
			    {
				    Hide();
			    }
		    }
	    }
    }
}
