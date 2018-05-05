using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Microsoft.Xna.Framework;
using RocketUI;
using RocketUI.Elements;
using RocketUI.Graphics;
using RocketUI.Graphics.Textures;

namespace Alex.API.Gui.Elements.Icons
{
    public class GuiConnectionPingIcon : GuiImage
    {
        private string _offlineState = GuiTextures.ServerPing0;

        private long[] _qualityThresholds = new long[]
        {
            50,
            150,
            250,
            500,
            1000
        };

        private string[] _qualityStates = new[]
        {
            GuiTextures.ServerPing1,
            GuiTextures.ServerPing2,
            GuiTextures.ServerPing3,
            GuiTextures.ServerPing4,
            GuiTextures.ServerPing5,
        };

        private string[] _connectingStates = new[]
        {
            GuiTextures.ServerPingPending1,
            GuiTextures.ServerPingPending2,
            GuiTextures.ServerPingPending3,
            GuiTextures.ServerPingPending4,
            GuiTextures.ServerPingPending5,
        };

        private GuiTexture2D _offlineTexture;
        private GuiTexture2D[] _qualityStateTextures = new GuiTexture2D[5];
        private GuiTexture2D[] _connectingStateTextures = new GuiTexture2D[5];
	    private TextBlock _playerCountElement;

        private bool _isPending;
        private int _animationFrame;
	    private bool _isPendingUpdate;
	    private bool _isOutdated = false;
        public GuiConnectionPingIcon() : base(GuiTextures.ServerPing0)
        {
            SetFixedSize(10, 8);
        }

        protected override void OnInit()
        {
            base.OnInit();

            Resources.TryGetGuiTexture(_offlineState, out _offlineTexture);

            for (int i = 0; i < _qualityStates.Length; i++)
            {
                Resources.TryGetGuiTexture(_qualityStates[i], out _qualityStateTextures[i]);
            }
            for (int i = 0; i < _connectingStates.Length; i++)
            {
                Resources.TryGetGuiTexture(_connectingStates[i], out _connectingStateTextures[i]);
            }

			AddChild(_playerCountElement = new TextBlock(false)
			{
				Text = string.Empty,
                Anchor = Anchor.TopRight,
				Margin = new Thickness(5, 0, Background.Width + 15, 0)
			});
        }

        public void SetPending()
        {
            _isPending = true;
            Background = _connectingStateTextures[0];
        }

        public void SetPing(long ms)
        {
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

	    public void SetOutdated(string message)
	    {
		    _playerCountElement.Text = $"§4{message}";
		    _isOutdated = true;
			SetOffline();
	    }

        public void SetOffline()
        {
            _isPending = false;
            Background = _offlineTexture;
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            if (_isPending)
            {
                var dt = gameTime.ElapsedGameTime.TotalSeconds / 20.0f;

                _animationFrame = (int)((dt * 20.0f) % _connectingStates.Length);
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
            }
        }
    }
}
