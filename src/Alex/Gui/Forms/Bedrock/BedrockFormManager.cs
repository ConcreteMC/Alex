using Alex.API.Gui;
using Alex.API.Input;
using Alex.API.Network;
using Alex.Net;
using Alex.Worlds.Bedrock;
using GLib;
using MiNET.Net;
using MiNET.UI;
using NLog;

namespace Alex.Gui.Forms.Bedrock
{
    public class BedrockFormManager
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        
        private GuiManager GuiManager { get; }
        private NetworkProvider NetworkProvider { get; }
        private InputManager InputManager { get; }
        
        private FormBase _activeForm = null;
        public BedrockFormManager(NetworkProvider networkProvider, GuiManager guiManager, InputManager input)
        {
            NetworkProvider = networkProvider;
            GuiManager = guiManager;
            InputManager = input;
        }

        public bool IsShowingForm => _activeForm != null;

        public void Show(uint id, Form form)
        {
            if (_activeForm != null)
            {
                GuiManager.HideDialog(_activeForm);
            }
            
            if (form is SimpleForm simpleForm)
            {
                GuiManager.ShowDialog(_activeForm = new SimpleFormDialog(id, this, simpleForm, InputManager));
            }
            else if (form is CustomForm customForm)
            {
                GuiManager.ShowDialog(_activeForm = new CustomFormDialog(id, this, customForm, InputManager));
            }
            else
            {
                Log.Warn($"Form type not supported: {form.GetType()}");
            }
        }

        public void Hide(uint id)
        {
            if (_activeForm != null)
            {
                if (_activeForm.FormId == id)
                {
                    GuiManager.HideDialog(_activeForm);
                    _activeForm = null;
                }
            }
        }

        public void SendResponse(McpeModalFormResponse response)
        {
            if (NetworkProvider is BedrockClient client)
            {
                client.SendPacket(response);
            }
        }
    }
}