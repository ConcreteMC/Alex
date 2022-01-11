using Alex.Net;
using Alex.Net.Bedrock;
using Alex.Worlds.Multiplayer.Bedrock;
using MiNET.Net;
using MiNET.UI;
using NLog;
using RocketUI;
using RocketUI.Input;

namespace Alex.Gui.Forms
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
            else if (form is ModalForm modalForm)
            {
                GuiManager.ShowDialog(_activeForm = new ModalFormDialog(id, this, modalForm, InputManager));
            }
            else
            {
                Log.Warn($"Form type not supported: {form.GetType()}");
            }
        }

        public void Hide(uint id)
        {
            var activeForm = _activeForm;
            if (activeForm != null && activeForm.FormId == id)
            {
                _activeForm = null;
                activeForm?.Close();
            }
        }

        public void SendResponse(McpeModalFormResponse response)
        {
            if (NetworkProvider is BedrockClient client)
            {
                client.SendPacket(response);
            }
        }

        public void CloseAll()
        {
            var active = _activeForm;
            if (active != null) 
                Hide(active.FormId);
        }
    }
}