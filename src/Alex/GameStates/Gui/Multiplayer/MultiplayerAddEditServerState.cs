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
    public class MultiplayerAddEditServerState : GuiCallbackStateBase<SavedServerEntry>
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MultiplayerConnectState));
        
        #region Gui Elements

        private readonly GuiTextInput _nameInput;
		private readonly GuiTextInput _hostnameInput;
        private readonly GuiTextElement _errorMessage;
        private readonly GuiButton _saveButton;

        #endregion

        private readonly SavedServerEntry _entry;
        private readonly IListStorageProvider<SavedServerEntry> _savedServersStorage;
        
        public MultiplayerAddEditServerState(Action<SavedServerEntry> callbackAction) : this(null, null, callbackAction)
        {

        }

        public MultiplayerAddEditServerState(string name, string address, Action<SavedServerEntry> callbackAction) :
            base(callbackAction)
        {
            _savedServersStorage = GetService<IListStorageProvider<SavedServerEntry>>();

            Title = "Add Server";
            
            AddGuiElement(_nameInput = new GuiTextInput(name)
            {
                Width       = 200,
                Anchor      = Alignment.MiddleCenter,
                PlaceHolder = "Server Name",
                Margin = new Thickness(5),
            });

            AddGuiElement(_hostnameInput = new GuiTextInput(address)
            {
                Width       = 200,
                Anchor      = Alignment.MiddleCenter,
                PlaceHolder = "Server Address...",
                Margin = new Thickness(5)
            });

            AddGuiElement(_saveButton = new GuiButton(OnSaveButtonPressed)
            {
                TranslationKey = "addServer.add",
                Margin = new Thickness(5)
            });

            AddGuiElement(new GuiButton(OnCancelButtonPressed)
            {
                TranslationKey = "gui.cancel",
                Margin = new Thickness(5)
            });

            AddGuiElement(_errorMessage = new GuiTextElement()
            {
                TextColor = TextColor.Red
            });
        }

        public MultiplayerAddEditServerState(SavedServerEntry entry, Action<SavedServerEntry> callbackAction) : this(entry.Name, entry.Host + ":" + entry.Port, callbackAction)
        {
            if (entry != null)
            {
                _entry = entry;
            }
        }

        private void OnSaveButtonPressed()
        {
            try
            {
                var name = _nameInput.Value;
                var address = _hostnameInput.Value;

                ushort port = 25565;

                var split = address.Split(':', StringSplitOptions.RemoveEmptyEntries);
                var hostname = split[0];

                if (split.Length == 2)
                {
                    if (ushort.TryParse(split[1], out port))
                    {
                        SaveServer(name, hostname, port);
                    }
                    else
                    {
                        SetErrorMessage("Invalid Server Address!");
                    }
                }
                else if (split.Length == 1)
                {
                    SaveServer(name, hostname, port);
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

        private void SaveServer(string name, string hostname, ushort port)
        {
            var entry = new SavedServerEntry()
            {
                Name       = name,
                Host       = hostname,
                Port       = port,
                ServerType = _entry.ServerType,
				CachedIcon = _entry.CachedIcon,
				ListIndex = _entry.ListIndex
            };
            
            if (_entry != null)
            {
                _savedServersStorage.RemoveEntry(_entry);
            }
            _savedServersStorage.AddEntry(entry);

            InvokeCallback(entry);
        }
        
        private void SetErrorMessage(string error)
        {
            _errorMessage.Text = error;
        }
    }
}
