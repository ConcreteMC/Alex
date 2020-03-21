using Alex.API.Gui;
using Alex.API.Gui.Dialogs;
using Alex.API.Network;
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
        private INetworkProvider NetworkProvider { get; }

        private FormBase _activeForm = null;
        public BedrockFormManager(INetworkProvider networkProvider, GuiManager guiManager)
        {
            NetworkProvider = networkProvider;
            GuiManager = guiManager;
        }

        public void Show(uint id, Form form)
        {
            if (form is SimpleForm simpleForm)
            {
                if (_activeForm != null)
                {
                    GuiManager.HideDialog(_activeForm);
                }
                
                GuiManager.ShowDialog(_activeForm = new SimpleFormDialog(id, this, simpleForm));
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