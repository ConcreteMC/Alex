using System;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Gamestates.Common;
using NLog;
using RocketUI;

namespace Alex.Gamestates.Multiplayer
{
    public class MultiplayerConnectState : GuiMenuStateBase
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MultiplayerConnectState));

		private readonly GuiTextInput _hostnameInput;
        private readonly GuiButton _connectButton;
        private readonly GuiTextElement _errorMessage;

        public MultiplayerConnectState()
        {
            Title = "Connect to Server";

            AddGuiElement(_hostnameInput = new GuiTextInput()
            {
                Width = 200,
                Anchor = Alignment.MiddleCenter,
				PlaceHolder = "Server Address..."
            });
            AddGuiElement( _connectButton = new GuiButton("Join Server", OnConnectButtonPressed)
            {
				Margin = new Thickness(5)
            });
            AddGuiElement(_errorMessage = new GuiTextElement()
            {
                TextColor = TextColor.Red
            });
        }

        private void OnConnectButtonPressed()
        {
            try
            {
                var hostname = _hostnameInput.Value;

                ushort port = 25565;

                var split = hostname.Split(':');
                if (split.Length == 2)
                {
                    if (ushort.TryParse(split[1], out port))
                    {
                        QueryServer(split[0], port);
                    }
                    else
                    {
                        SetErrorMessage("Invalid Server Address!");
                    }
                }
                else if (split.Length == 1)
                {
                    QueryServer(split[0], port);
                }
                else
                {
                    SetErrorMessage("Invalid Server Address!");
                }
            }
            catch (Exception ex)
            {
				Log.Error($"Error: {ex.ToString()}");
                SetErrorMessage(ex.Message);
            }
        }

        private void QueryServer(string address, ushort port)
        {
            SetErrorMessage(null);
            SetConnectingState(true);

            var queryProvider = GetService<IServerQueryProvider>();
	        queryProvider.QueryServerAsync(address, port);
        }

        private void SetConnectingState(bool connecting)
        {
            if (connecting)
            {
                _connectButton.Text = "Connecting...";
            }
            else
            {
                _connectButton.Text = "Connect";
            }

            _hostnameInput.Enabled = !connecting;
            _connectButton.Enabled = !connecting;
        }

        private void SetErrorMessage(string error)
        {
            _errorMessage.Text = error;
        }
    }
}
