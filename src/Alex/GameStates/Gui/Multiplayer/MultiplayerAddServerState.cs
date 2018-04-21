using System;
using Alex.API.Data.Servers;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.GameStates.Gui.Common;
using NLog;

namespace Alex.GameStates.Gui.Multiplayer
{
    public class MultiplayerAddServerState : GuiCallbackStateBase<SavedServerEntry>
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MultiplayerConnectState));
        
        private readonly GuiTextInput _nameInput;
		private readonly GuiTextInput _hostnameInput;
        private readonly GuiButton _connectButton;
        private readonly GuiButton _cancelButton;
        private readonly GuiTextElement _errorMessage;

        private readonly IListStorageProvider<SavedServerEntry> _savedServersStorage;

        public MultiplayerAddServerState(Action<SavedServerEntry> callbackAction) : base(callbackAction)
        {
            _savedServersStorage = GetService<IListStorageProvider<SavedServerEntry>>();

            Title = "Add Server";
            
            AddGuiElement(_nameInput = new GuiTextInput()
            {
                Width       = 200,
                Anchor      = Alignment.MiddleCenter,
                PlaceHolder = "Server Name"
            });

            AddGuiElement(_hostnameInput = new GuiTextInput()
            {
                Width = 200,
                Anchor = Alignment.MiddleCenter,
				PlaceHolder = "Server Address..."
            });
            AddGuiElement( _connectButton = new GuiButton("Add Server", OnAddButtonPressed)
            {
                Margin = new Thickness(5)
            });
            AddGuiElement( _cancelButton = new GuiButton("Cancel", OnCancelButtonPressed)
            {
				Margin = new Thickness(5)
            });
            AddGuiElement(_errorMessage = new GuiTextElement()
            {
                TextColor = TextColor.Red
            });
        }

        private void OnAddButtonPressed()
        {
            try
            {
                var name = _nameInput.Value;
                var hostname = _hostnameInput.Value;

                ushort port = 25565;

                var split = hostname.Split(':');
                if (split.Length == 2)
                {
                    if (ushort.TryParse(split[1], out port))
                    {
                        AddServer(name, hostname, port);
                    }
                    else
                    {
                        SetErrorMessage("Invalid Server Address!");
                    }
                }
                else if (split.Length == 1)
                {
                    AddServer(name, hostname, port);
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

        private void OnCancelButtonPressed()
        {
            InvokeCallback(null);
        }

        private void AddServer(string name, string hostname, ushort port)
        {
            var entry = new SavedServerEntry()
            {
                Name       = name,
                Host       = hostname,
                Port       = port,
                ServerType = ServerType.Java
            };
            _savedServersStorage.AddEntry(entry);

            InvokeCallback(entry);
        }
        
        private void SetErrorMessage(string error)
        {
            _errorMessage.Text = error;
        }
    }
}
