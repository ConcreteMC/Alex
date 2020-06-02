using System;
using Alex.API.Graphics.Textures;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI;

namespace Alex.API.Gui.Elements.Icons
{
    public class GuiConnectionPingIcon : GuiElement
    {
        private GuiTextures _offlineState = GuiTextures.ServerPing0;

        private long[] _qualityThresholds = new long[]
        {
            50,
            150,
            250,
            500,
            1000
        };

        private GuiTextures[] _qualityStates = new[]
        {
            GuiTextures.ServerPing1,
            GuiTextures.ServerPing2,
            GuiTextures.ServerPing3,
            GuiTextures.ServerPing4,
            GuiTextures.ServerPing5,
        };

        private GuiTextures[] _connectingStates = new[]
        {
            GuiTextures.ServerPingPending1,
            GuiTextures.ServerPingPending2,
            GuiTextures.ServerPingPending3,
            GuiTextures.ServerPingPending4,
            GuiTextures.ServerPingPending5,
        };

        private TextureSlice2D _offlineTexture;
        private TextureSlice2D[] _qualityStateTextures = new TextureSlice2D[5];
        private TextureSlice2D[] _connectingStateTextures = new TextureSlice2D[5];
	    private GuiTextElement _playerCountElement;

        private bool _isPending;
        private int _animationFrame;
	    private bool _isPendingUpdate;
	    private bool _isOutdated = false;
	    private long _ping = 0;
	    private bool _renderLatency = false;
	    public GuiConnectionPingIcon() : base()
	    {
		    Background = GuiTextures.ServerPing0;
            SetFixedSize(10, 8);
	    }

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);

            _offlineTexture = renderer.GetTexture(_offlineState);

            for (int i = 0; i < _qualityStates.Length; i++)
            {
                _qualityStateTextures[i] = renderer.GetTexture(_qualityStates[i]);
            }
            for (int i = 0; i < _connectingStates.Length; i++)
            {
                _connectingStateTextures[i] = renderer.GetTexture(_connectingStates[i]);
            }

			AddChild(_playerCountElement = new GuiTextElement(false)
			{
				Font = renderer.Font,
				Text = string.Empty,
                Anchor = Alignment.TopRight,
				Margin = new Thickness(5, 0, Background.Width + 15, 0),
				Enabled = false
			});
        }

        public void SetPending()
        {
            _isPending = true;
            Background = _connectingStateTextures[0];
        }

        public void SetPing(long ms)
        {
	        _ping = ms;
            _isPending = false;

	        if (!_isOutdated)
	        {
		        int index = 0;
		        for (int i = _qualityStateTextures.Length - 1; i > 0; i--)
		        {
			        index = i;
			        if (ms > _qualityThresholds[i]) break;

		        }

		        Background = _qualityStateTextures[_qualityStateTextures.Length - index];
	        }
        }

	    public void SetPlayerCount(int players, int max)
	    {
		    _playerCountElement.Text = $"§7{players}/{max}";
	    }

	    public void SetOutdated(string message, bool isTranslation = false)
	    {
		    if (isTranslation)
		    {
			    _playerCountElement.TranslationKey = message;
            }
		    else
		    {
			    _playerCountElement.Text = $"§4{message}";
		    }

		    _isOutdated = true;
			SetOffline();
	    }

        public void SetOffline()
        {
            _isPending = false;
            Background = _offlineTexture;
        }

        private Point _cursorPosition = Point.Zero;
        protected override void OnUpdate(GameTime gameTime)
        {
	        base.OnUpdate(gameTime);

            if (_isPending)
            {
                var dt = gameTime.TotalGameTime.TotalSeconds;

                _animationFrame = (int)((dt * 5) % _connectingStates.Length);
            }
            else
            {
	            var mouseState = Mouse.GetState();

	            _cursorPosition = GuiRenderer.Unproject(new Vector2(mouseState.X, mouseState.Y)).ToPoint();
	            if (RenderBounds.Contains(_cursorPosition))
	            {
		            _renderLatency = true;
	            }
	            else
	            {
		            _renderLatency = false;
	            }
            }
        }

        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            if (_isPending)
            {
                graphics.FillRectangle(RenderBounds,_connectingStateTextures[_animationFrame],  TextureRepeatMode.NoScaleCenterSlice);
            }
            else
            {
	            base.OnDraw(graphics, gameTime);
	            
	            if (_renderLatency)
	            {
		            string text = $"{_ping}ms";

		            var size = graphics.Font.MeasureString(text);
		            var position = _cursorPosition + new Point(5, 5);
		            
		            graphics.SpriteBatch.FillRectangle(new Rectangle(position, size.ToPoint()), Color.Black * 0.5f);
		            graphics.DrawString(position.ToVector2(), text, TextColor.White, FontStyle.None, 1f);
	            }
            }
        }
    }
}
