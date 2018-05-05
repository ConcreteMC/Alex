using System;
using System.Threading.Tasks;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.GameStates.Gui.Common;
using NLog;
using RocketUI;
using RocketUI.Elements;
using RocketUI.Elements.Controls;

namespace Alex.GameStates.Gui.Multiplayer
{
    public class MultiplayerConnectState : GuiMenuStateBase
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MultiplayerConnectState));

		private readonly GuiTextInput _hostnameInput;
        private readonly MCButton _connectButton;
        private readonly GuiMCTextElement _errorMessage;

        public MultiplayerConnectState()
        {
            Title = "Connect to Server";

            AddGuiElement(_hostnameInput = new GuiTextInput()
            {
                Width = 200,
                Anchor = Anchor.MiddleCenter,
				PlaceHolder = "Server Address..."
            });
            AddGuiElement( _connectButton = new MCButton("Join Server", OnConnectButtonPressed)
            {
				Margin = new Thickness(5)
            });
            AddGuiElement(_errorMessage = new GuiMCTextElement()
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
            queryProvider.QueryServerAsync(address, port).ContinueWith(ContinuationAction);
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

        private void ContinuationAction(Task<ServerQueryResponse> queryTask)
        {
            var response = queryTask.Result;
            
            if (response.Success)
            {
                Alex.ConnectToServer(response.Status.EndPoint);
			}
	        else
	        {
		        SetConnectingState(false);
	        }
        }
    }
}
