using Alex.API.Gui;
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
        public BedrockFormManager(INetworkProvider networkProvider, GuiManager guiManager)
        {
            NetworkProvider = networkProvider;
            GuiManager = guiManager;
        }

        public void Show(uint id, Form form)
        {
            if (form is SimpleForm simpleForm)
            {
                GuiManager.ShowDialog(new SimpleFormDialog(id, this, GuiManager, simpleForm));
            }
            else
            {
                Log.Warn($"Form type not supported: {form.GetType()}");
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