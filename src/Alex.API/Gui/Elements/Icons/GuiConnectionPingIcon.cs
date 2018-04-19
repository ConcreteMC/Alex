using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui.Rendering;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements.Icons
{
    public class GuiConnectionPingIcon : GuiImage
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
        public GuiConnectionPingIcon() : base(GuiTextures.ServerPing0)
        {
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
				Margin = new Thickness(5, 0, Background.Width + 5, 0)
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
            int index = 0;
            for (int i = _qualityStateTextures.Length - 1; i > 0; i--)
            {
                if(ms > _qualityThresholds[i]) break;
                index = i;
            }

            Background = _qualityStateTextures[index];
        }

	    public void SetPlayerCount(int players, int max)
	    {
		    _playerCountElement.Text = $"§7{players}/{max}";
		    UpdateKids();
		    //_isPending = true;
	    }

	    public void SetOutdated(string message)
	    {
		    _playerCountElement.Text = $"§4{message}";
		    UpdateKids();
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
                var dt = gameTime.ElapsedGameTime.Seconds / 20.0f;

                _animationFrame = (int)((dt * 20.0f) % _connectingStates.Length);

            }
        }

        protected override void OnDraw(GuiRenderArgs args)
        {
            if (_isPending)
            {
                args.Draw(_connectingStateTextures[_animationFrame], RenderBounds, TextureRepeatMode.NoScaleCenterSlice);
            }
            else
            {
                base.OnDraw(args);
            }
        }

	    protected override void OnUpdateLayout()
	    {
		    base.OnUpdateLayout();
			UpdateKids();
	    }

	    private void UpdateKids()
	    {
		    if (_playerCountElement != null)
		    {
				
			    //_playerCountElement.X = -_playerCountElement.Width - 4;
		    }
		}
    }
}
