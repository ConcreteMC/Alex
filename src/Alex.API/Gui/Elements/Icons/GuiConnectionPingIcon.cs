using System;
using Alex.API.Gui.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI;

namespace Alex.API.Gui.Elements.Icons
{
	public class GuiConnectionPingIcon : RocketElement
	{
		private static readonly GuiTextures OfflineState = AlexGuiTextures.ServerPing0;

		private static readonly long[] QualityThresholds = new long[] {50, 150, 250, 500, 1000};

		private static readonly GuiTextures[] QualityStates = new[]
		{
			AlexGuiTextures.ServerPing1, AlexGuiTextures.ServerPing2, AlexGuiTextures.ServerPing3, AlexGuiTextures.ServerPing4,
			AlexGuiTextures.ServerPing5,
		};

		private static readonly GuiTextures[] ConnectingStates = new[]
		{
			AlexGuiTextures.ServerPingPending1, AlexGuiTextures.ServerPingPending2, AlexGuiTextures.ServerPingPending3,
			AlexGuiTextures.ServerPingPending4, AlexGuiTextures.ServerPingPending5,
		};

		private TextureSlice2D   _offlineTexture;
		private TextureSlice2D[] _qualityStateTextures    = new TextureSlice2D[5];
		private TextureSlice2D[] _connectingStateTextures = new TextureSlice2D[5];
		private TextElement   _playerCountElement;

		private bool   _isPending;
		private int    _animationFrame;
		private bool   _isPendingUpdate;
		private bool   _isOutdated    = false;
		private long   _ping          = 0;
		private bool   _renderLatency = false;
		private string _version       = null;

		private bool _showPlayerCountElement = true;

		public bool ShowPlayerCount
		{
			get
			{
				return _showPlayerCountElement;
			}
			set
			{
				if (value && !_showPlayerCountElement)
				{
					AddChild(_playerCountElement);
				}
				else if (!value && _showPlayerCountElement)
				{
					RemoveChild(_playerCountElement);
				}
				_showPlayerCountElement = value;
			}
		}

		public GuiConnectionPingIcon() : base()
	    {
		    Background = AlexGuiTextures.ServerPing0;
            SetFixedSize(10, 8);
            
            _playerCountElement = new TextElement(false)
            {
	            //Font = renderer.Font,
	            Text = string.Empty,
	            Anchor = Alignment.TopRight,
	            Margin = new Thickness(5, 0, Background.Width + 15, 0),
	            //			Enabled = false
            };
	    }

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);

            _offlineTexture = renderer.GetTexture(OfflineState);

            for (int i = 0; i < QualityStates.Length; i++)
            {
                _qualityStateTextures[i] = renderer.GetTexture(QualityStates[i]);
            }
            for (int i = 0; i < ConnectingStates.Length; i++)
            {
                _connectingStateTextures[i] = renderer.GetTexture(ConnectingStates[i]);
            }

            _playerCountElement.Font = renderer.Font;
            if (_showPlayerCountElement)
            {
	            AddChild(_playerCountElement);
            }
        }

        public void SetPending()
        {
            _isPending = true;
            Background.Texture = _connectingStateTextures[0];
        }

        public void SetVersion(string version)
        {
	        _version = version;
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
			        if (ms > QualityThresholds[i]) break;
		        }

		        GuiTexture2D bg = new GuiTexture2D(_qualityStateTextures[_qualityStateTextures.Length - index]);

		        if (!bg.HasValue && GuiRenderer != null)
		        {
			        bg = GuiRenderer.GetTexture(QualityStates[QualityStates.Length - index]);
		        }

		        Background = bg;
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

                _animationFrame = (int)((dt * 5) % ConnectingStates.Length);
            }
            else
            {
	            var mouseState = Mouse.GetState();

	            _cursorPosition = GuiRenderer.Unproject(new Vector2(mouseState.X, mouseState.Y)).ToPoint();
	            if (RenderBounds.Contains(_cursorPosition) || _playerCountElement.RenderBounds.Contains(_cursorPosition))
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
	            
	            //graphics.FillRectangle(RenderBounds, Background,  TextureRepeatMode.NoScaleCenterSlice);
	            
	            if (_renderLatency)
	            {
		            string text = $"{_ping}ms";

		            if (!string.IsNullOrWhiteSpace(_version))
		            {
			            text = $"Server Ping: {text}\nGame Version: {_version.Trim()}";
		            }
		            
		            var size = graphics.Font.MeasureString(text);
		            var position = _cursorPosition + new Point(5, 5);
		            
		            graphics.SpriteBatch.FillRectangle(new Rectangle(position, size.ToPoint()), Color.Black * 0.5f);
		            graphics.DrawString(graphics.Font, text, position.ToVector2(), TextColor.White, FontStyle.None);
	            }
            }
        }
    }
}
